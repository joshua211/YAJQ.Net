using System.Collections.Concurrent;
using Fastjob.Core.JobProcessor;
using Fastjob.Core.Persistence;
using Fastjob.Core.Utils;
using Microsoft.Extensions.Logging;

namespace Fastjob.Core.JobHandler;

public class DefaultJobHandler : IJobHandler
{
    private readonly ILogger<DefaultJobHandler> logger;
    private readonly ConcurrentQueue<string> openJobs;
    private readonly FastjobOptions options;
    private readonly IJobProcessorFactory processorFactory;
    private readonly IJobRepository repository;
    private readonly IProcessorSelectionStrategy selectionStrategy;
    private CancellationTokenSource source;

    public DefaultJobHandler(ILogger<DefaultJobHandler> logger, IModuleHelper moduleHelper,
        IJobRepository repository, FastjobOptions options, IJobProcessorFactory processorFactory,
        IProcessorSelectionStrategy selectionStrategy)
    {
        this.logger = logger;
        this.repository = repository;
        this.options = options;
        this.processorFactory = processorFactory;
        this.selectionStrategy = selectionStrategy;

        openJobs = new ConcurrentQueue<string>();
        source = new CancellationTokenSource();

        foreach (var _ in Enumerable.Range(0, options.NumberOfProcessors))
        {
            selectionStrategy.AddProcessor(processorFactory.New());
        }

        repository.Update += OnJobUpdate;
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var nextJob = await WaitForNextJobIdAsync(cancellationToken);
            logger.LogTrace("Found open job: {Id}", nextJob);

            var processor = await selectionStrategy.GetNextProcessorAsync();

            var result = await repository.TryGetAndMarkJobAsync(nextJob, processor.ProcessorId);
            if (!result.WasSuccess)
            {
                logger.LogTrace("Job with id {Id} was already claimed, skipping", nextJob);
                continue;
            }

            var _ = Task.Run(() => ProcessJobAsync(result.Value, processor, cancellationToken), cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task<string> WaitForNextJobIdAsync(CancellationToken cancellationToken)
    {
        string? nextJob = null;
        while (nextJob is null)
        {
            if (openJobs.TryDequeue(out nextJob))
                continue;

            var nextPersistedJob = await repository.GetNextJobAsync();
            if (!nextPersistedJob.WasSuccess
                || !string.IsNullOrWhiteSpace(nextPersistedJob.Value.ConcurrencyTag)
                || nextPersistedJob.Value.State != JobState.Pending)
            {
                logger.LogTrace("No job in the database, waiting for {Timeout} ms", options.HandlerTimeout);

                await Task.Delay(options.HandlerTimeout, source.Token).ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        logger.LogTrace("Waking up from waiting");
                }, cancellationToken);

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

            var complete = await repository.CompleteJobAsync(job.Id, false);
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

        openJobs.Enqueue(e.JobId);
        source.Cancel();
        source = new CancellationTokenSource();
    }
}