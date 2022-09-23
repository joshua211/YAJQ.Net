using Microsoft.Extensions.Logging;
using YAJQ.Core.JobHandler.Interfaces;
using YAJQ.Core.JobProcessor;
using YAJQ.Core.JobProcessor.Interfaces;
using YAJQ.Core.Persistence;
using YAJQ.Core.Persistence.Interfaces;
using YAJQ.Core.Utils;

namespace YAJQ.Core.JobHandler;

public class MultiProcessorJobHandler : IJobHandler, IDisposable
{
    private readonly ILogger<MultiProcessorJobHandler> logger;
    private readonly IOpenJobProvider openJobProvider;
    private readonly YAJQOptions options;
    private readonly IJobProcessorFactory processorFactory;
    private readonly IJobRepository repository;
    private readonly IProcessorSelectionStrategy selectionStrategy;
    private readonly IScheduledJobSubHandler subHandler;
    private CancellationTokenSource source;


    public MultiProcessorJobHandler(ILogger<MultiProcessorJobHandler> logger, IModuleHelper moduleHelper,
        IJobRepository repository, YAJQOptions options, IJobProcessorFactory processorFactory,
        IProcessorSelectionStrategy selectionStrategy, IOpenJobProvider openJobProvider,
        IScheduledJobSubHandler subHandler)
    {
        this.logger = logger;
        this.repository = repository;
        this.options = options;
        this.processorFactory = processorFactory;
        this.selectionStrategy = selectionStrategy;
        this.openJobProvider = openJobProvider;
        this.subHandler = subHandler;

        source = new CancellationTokenSource();
        HandlerId = $"{Environment.MachineName}:{Guid.NewGuid().ToString().Split("-").First()}";

        foreach (var _ in Enumerable.Range(0, options.NumberOfProcessors))
            selectionStrategy.AddProcessor(processorFactory.New());

        repository.Update += OnJobUpdate;
    }

    public void Dispose()
    {
        source.Dispose();
    }

    public string HandlerId { get; }

    public async Task Start(CancellationToken cancellationToken)
    {
        subHandler.Start(HandlerId, selectionStrategy, ProcessJobAsync, cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            var nextJob = await openJobProvider.GetNextJobAsync(HandlerId, source, cancellationToken);
            LogTrace("Found open job: {Id}", nextJob.Id);

            var result = await repository.TryGetAndMarkJobAsync(nextJob, HandlerId);
            if (!result.WasSuccess)
            {
                LogTrace("Job with id {Id} was already claimed, skipping", nextJob.Id);
                continue;
            }

            LogTrace("Claimed Job with Id {Id}", result.Value.Id);

            if (result.Value.JobType == JobType.Scheduled)
            {
                LogTrace("{Id} is a scheduled job, adding to backlog", result.Value.Id);
                subHandler.AddScheduledJob(result.Value);
                continue;
            }

            var processor = await selectionStrategy.GetNextProcessorAsync();
            var _ = Task.Run(() => ProcessJobAsync(result.Value, processor, cancellationToken), cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task ProcessJobAsync(PersistedJob job, IJobProcessor processor, CancellationToken cancellationToken)
    {
        var processingResult = processor.ProcessJob(job.Descriptor, cancellationToken);
        if (!processingResult.WasSuccess)
        {
            logger.LogWarning("Failed to process Job {Name} with Id {Id}: {Error}",
                job.Descriptor.JobName, job.Id, processingResult.Error);

            var complete = await repository.CompleteJobAsync(job.Id, HandlerId, processor.ProcessorId,
                processingResult.Value.ProcessingTime,
                processingResult.Error, processingResult.Value.LastException, false);
            if (!complete.WasSuccess)
                logger.LogWarning("Failed to complete failed job {Name} with Id {Id}: {Error}",
                    job.Descriptor.JobName, job.Id, complete.Error);

            return;
        }

        logger.LogTrace("Completing Job {Name} with Id {Id}", job.Descriptor.JobName, job.Id);
        await repository.CompleteJobAsync(job.Id, HandlerId, processor.ProcessorId,
            processingResult.Value.ProcessingTime, wasSuccess: processingResult.Value.WasSuccess);
    }


    private void OnJobUpdate(object? sender, JobEvent e)
    {
        if (e.State != JobState.Pending)
            return;

        source.Cancel();
        source = new CancellationTokenSource();
    }

    private void LogTrace(string message, params object[] args)
    {
        logger.LogTrace($"[{HandlerId}]: " + message, args);
    }

    private void LogWarning(string message, params object[] args)
    {
        logger.LogWarning($"[{HandlerId}]: " + message, args);
    }

    private bool IsHandlerResponsibleForJob(PersistedJob job)
    {
        return job.ConcurrencyToken == HandlerId;
    }

    private bool IsUpdateOverdue(PersistedJob job)
    {
        return DateTimeOffset.Now > job.LastUpdated.AddSeconds(options.MaxOverdueTimeout);
    }
}