using Fastjob.Core.Interfaces;
using Fastjob.Core.JobProcessor;
using Fastjob.Core.Utils;
using Microsoft.Extensions.Logging;

namespace Fastjob.Core.JobHandler;

public class DefaultJobHandler : IJobHandler
{
    private readonly ILogger<DefaultJobHandler> logger;
    private readonly IJobProcessor processor;
    private readonly IJobStorage storage;
    private int index = 0;

    public DefaultJobHandler(ILogger<DefaultJobHandler> logger, IServiceProvider provider, IModuleHelper moduleHelper,
        IJobStorage storage)
    {
        this.logger = logger;
        this.storage = storage;

        processor = new DefaultJobProcessor(moduleHelper, provider, logger);
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await storage.GetNextJobAsync();
            if (!result.WasSuccess)
            {
                await Task.Delay(1000, cancellationToken);
                continue;
            }

            result = await storage.TryMarkAndGetJobAsync(result.Value.Id, processor.ProcessorId);
            if (!result.WasSuccess)
                continue;

            var descriptor = result.Value.Descriptor;
            var processingResult = await processor.ProcessJobAsync(descriptor, cancellationToken);
            if (!processingResult.WasSuccess)
            {
                //TODO handle errors maybe?
                continue;
            }

            await storage.RemoveJobAsync(result.Value.Id);
        }
    }
}