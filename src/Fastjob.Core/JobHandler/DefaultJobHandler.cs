using System.Collections.Concurrent;
using Fastjob.Core.Interfaces;
using Fastjob.Core.JobProcessor;
using Fastjob.Core.Persistence;
using Fastjob.Core.Utils;
using Microsoft.Extensions.Logging;

namespace Fastjob.Core.JobHandler;

public class DefaultJobHandler : IJobHandler
{
    private readonly ConcurrentQueue<string> openJobs;
    private readonly ILogger<DefaultJobHandler> logger;
    private readonly IJobProcessor processor;
    private readonly IJobRepository repository;
    private CancellationTokenSource source;
    private readonly FastjobOptions options;

    public DefaultJobHandler(ILogger<DefaultJobHandler> logger, IJobProcessor processor, IModuleHelper moduleHelper,
        IJobRepository repository, FastjobOptions options)
    {
        this.logger = logger;
        this.repository = repository;
        this.options = options;

        openJobs = new ConcurrentQueue<string>();
        source = new CancellationTokenSource();
        this.processor = processor;

        repository.OnJobEvent += HandleJobEvent;
    }

    private void HandleJobEvent(object? sender, string e)
    {
        openJobs.Enqueue(e);
        source.Cancel();
        source = new CancellationTokenSource();
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!openJobs.TryDequeue(out var nextJob))
            {
                var nextPersistedJob = await repository.GetNextJobAsync();
                if (!nextPersistedJob.WasSuccess || !string.IsNullOrWhiteSpace(nextPersistedJob.Value.ConcurrencyTag))
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

            logger.LogTrace("Found open job: {Id}", nextJob);

            var result = await repository.TryGetAndMarkJobAsync(nextJob, processor.ProcessorId);
            if (!result.WasSuccess)
            {
                logger.LogTrace("Job with id {Id} was already claimed, skipping", nextJob);
                continue;
            }

            var processingResult = await processor.ProcessJobAsync(result.Value.Descriptor, cancellationToken);
            if (!processingResult.WasSuccess)
            {
                logger.LogWarning("Failed to process Job {Name} with Id {Id}: {Error}",
                    result.Value.Descriptor.JobName, result.Value.Id, processingResult.Error);
                continue;
            }

            logger.LogTrace("Completing Job {Name} with Id {Id}", result.Value.Descriptor.JobName, result.Value.Id);
            await repository.CompleteJobAsync(result.Value.Id);
        }
    }
}