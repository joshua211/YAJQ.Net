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

    public DefaultJobHandler(ILogger<DefaultJobHandler> logger, IServiceProvider provider, IModuleHelper moduleHelper,
        IJobRepository repository)
    {
        this.logger = logger;
        this.repository = repository;
        
        openJobs = new ConcurrentQueue<string>();
        source = new CancellationTokenSource();
        processor = new DefaultJobProcessor(moduleHelper, provider, logger);

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
                if (!nextPersistedJob.WasSuccess || string.IsNullOrWhiteSpace(nextPersistedJob.Value.ConcurrencyTag))
                {
                    //no job in the database, lets wait some time but wake up if we get a new jobEvent
                    await Task.Delay(1000, source.Token);
                    continue;
                }

                nextJob = nextPersistedJob.Value.Id;
            }

            var result = await repository.TryGetAndMarkJobAsync(nextJob, processor.ProcessorId);
            if (!result.WasSuccess)
            {
                //seems like job was already claimed, lets move on
                continue;
            }

            var processingResult = await processor.ProcessJobAsync(result.Value.Descriptor, cancellationToken);
            if (!processingResult.WasSuccess)
            {
                //job failed, damn
                continue;
            }

            await repository.CompleteJobAsync(result.Value.Id);
        }
        /*while (!cancellationToken.IsCancellationRequested)
        {
            var result = await repository.GetNextJobAsync();
            if (!result.WasSuccess)
            {
                await Task.Delay(1000, cancellationToken);
                continue;
            }

            result = await repository.TryMarkAndGetJobAsync(result.Value.Id, processor.ProcessorId);
            if (!result.WasSuccess)
                continue;

            var descriptor = result.Value.Descriptor;
            var processingResult = await processor.ProcessJobAsync(descriptor, cancellationToken);
            if (!processingResult.WasSuccess)
            {
                //TODO handle errors maybe?
                continue;
            }

            await repository.RemoveJobAsync(result.Value.Id);
        }*/
    }
}