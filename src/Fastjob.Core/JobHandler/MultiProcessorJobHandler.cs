using Fastjob.Core.JobProcessor;
using Fastjob.Core.Persistence;
using Fastjob.Core.Utils;
using Microsoft.Extensions.Logging;

namespace Fastjob.Core.JobHandler;

public class MultiProcessorJobHandler : IJobHandler, IDisposable
{
    private readonly ILogger<MultiProcessorJobHandler> logger;
    private readonly FastjobOptions options;
    private readonly IJobProcessorFactory processorFactory;
    private readonly IJobRepository repository;
    private readonly List<PersistedJob> scheduledJobs;
    private readonly IProcessorSelectionStrategy selectionStrategy;
    private CancellationTokenSource source;


    public MultiProcessorJobHandler(ILogger<MultiProcessorJobHandler> logger, IModuleHelper moduleHelper,
        IJobRepository repository, FastjobOptions options, IJobProcessorFactory processorFactory,
        IProcessorSelectionStrategy selectionStrategy)
    {
        this.logger = logger;
        this.repository = repository;
        this.options = options;
        this.processorFactory = processorFactory;
        this.selectionStrategy = selectionStrategy;

        source = new CancellationTokenSource();
        scheduledJobs = new List<PersistedJob>();

        HandlerId = $"{Environment.MachineName}:{Guid.NewGuid().ToString().Split("-").First()}";

        foreach (var _ in Enumerable.Range(0, options.NumberOfProcessors))
        {
            selectionStrategy.AddProcessor(processorFactory.New());
        }

        repository.Update += OnJobUpdate;
    }

    public void Dispose()
    {
        source.Dispose();
    }

    public string HandlerId { get; private set; }

    public async Task Start(CancellationToken cancellationToken)
    {
        Task.Run(() => HandleScheduledJobs(cancellationToken), cancellationToken).ContinueWith(_ => { });

        while (!cancellationToken.IsCancellationRequested)
        {
            var nextJob = await WaitForNextJobIdAsync(cancellationToken);
            logger.LogTrace("Found open job: {Id}", nextJob);

            var result = await repository.TryGetAndMarkJobAsync(nextJob, HandlerId);
            if (!result.WasSuccess)
            {
                logger.LogTrace("Job with id {Id} was already claimed, skipping", nextJob);
                continue;
            }

            if (result.Value.JobType == JobType.Scheduled)
            {
                logger.LogTrace("Scheduled job, adding to backlog");
                scheduledJobs.Add(result.Value);
                continue;
            }

            var processor = await selectionStrategy.GetNextProcessorAsync();
            var _ = Task.Run(() => ProcessJobAsync(result.Value, processor, cancellationToken), cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task<string> WaitForNextJobIdAsync(CancellationToken cancellationToken)
    {
        //TODO check last job id == current job id
        string? nextJob = null;
        while (nextJob is null)
        {
            var nextPersistedJob = await repository.GetNextJobAsync();
            if (!nextPersistedJob.WasSuccess)
            {
                logger.LogTrace("No job in the database, waiting for {Timeout} ms", options.HandlerTimeout);

                await Task.Delay(options.HandlerTimeout, source.Token).ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        logger.LogTrace("Waking up from waiting");
                }, cancellationToken);

                continue;
            }

            if (nextPersistedJob.Value.JobType == JobType.Scheduled)
            {
                if (IsHandlerResponsibleForJob(nextPersistedJob.Value))
                    //This handler is already responsible for this job, no need to do anything
                    continue;

                if (!IsUpdateOverdue(nextPersistedJob.Value))
                    //Another handler is responsible and keeps updating the job
                    continue;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(nextPersistedJob.Value.ConcurrencyToken) ||
                    nextPersistedJob.Value.State != JobState.Pending)
                    //Another handler is already handling this job or it was already handled
                    continue;
            }

            nextJob = nextPersistedJob.Value.Id;
        }

        return nextJob;
    }

    private async Task ProcessJobAsync(PersistedJob job, IJobProcessor processor, CancellationToken cancellationToken)
    {
        var processingResult = processor.ProcessJob(job.Descriptor, cancellationToken);
        if (!processingResult.WasSuccess)
        {
            logger.LogWarning("Failed to process Job {Name} with Id {Id}: {Error}",
                job.Descriptor.JobName, job.Id, processingResult.Error);

            var complete = await repository.CompleteJobAsync(job.Id, HandlerId, processor.ProcessorId, TimeSpan.Zero,
                processingResult.Error, null, false);
            if (!complete.WasSuccess)
                logger.LogWarning("Failed to complete failed job {Name} with Id {Id}: {Error}",
                    job.Descriptor.JobName, job.Id, complete.Error);

            return;
        }

        logger.LogTrace("Completing Job {Name} with Id {Id}", job.Descriptor.JobName, job.Id);
        await repository.CompleteJobAsync(job.Id);
    }


    private void OnJobUpdate(object? sender, JobEvent e)
    {
        if (e.State != JobState.Pending)
            return;

        source.Cancel();
        source = new CancellationTokenSource();
    }

    private async void HandleScheduledJobs(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            foreach (var job in scheduledJobs.ToList())
            {
                if (job.ScheduledTime < DateTimeOffset.Now)
                {
                    var processor = await selectionStrategy.GetNextProcessorAsync();

                    var _ = Task.Run(() => ProcessJobAsync(job, processor, token), token)
                        .ConfigureAwait(false);
                    scheduledJobs.Remove(job);
                }
                else
                {
                    var result = await repository.RefreshTokenAsync(job.Id, HandlerId);
                    if (!result.WasSuccess)
                        logger.LogWarning("Failed to refresh claimed job with Id {Id}", job.Id);
                }
            }

            await Task.Delay(options.ScheduledJobTimerInterval, token).ContinueWith(_ => { });
        }
    }

    private bool IsHandlerResponsibleForJob(PersistedJob job) =>
        job.ConcurrencyToken == HandlerId;

    private bool IsUpdateOverdue(PersistedJob job) =>
        DateTimeOffset.Now > job.LastUpdated.AddSeconds(options.MaxOverdueTimeout);
}