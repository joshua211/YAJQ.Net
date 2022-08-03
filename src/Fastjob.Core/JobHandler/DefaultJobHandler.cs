using Fastjob.Core.Interfaces;
using Fastjob.Core.JobProcessor;
using Fastjob.Core.Utils;
using Microsoft.Extensions.Logging;

namespace Fastjob.Core.JobHandler;

public class DefaultJobHandler : IJobHandler
{
    private readonly ILogger<DefaultJobHandler> logger;
    private readonly IServiceProvider provider;
    private readonly IModuleHelper moduleHelper;
    private readonly IJobProcessor processor;
    private readonly IJobStorage storage;

    public DefaultJobHandler(ILogger<DefaultJobHandler> logger, IServiceProvider provider, IModuleHelper moduleHelper,
        IJobStorage storage)
    {
        this.logger = logger;
        this.provider = provider;
        this.moduleHelper = moduleHelper;
        this.storage = storage;

        processor = new DefaultJobProcessor(moduleHelper, provider, logger);
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellationToken);

            var id = await storage.GetNextJobIdAsync();
            if (string.IsNullOrEmpty(id))
                continue;

            var result = await storage.TryMarkJobAsync(id, processor.ProcessorId);
            if (!result.WasSuccess)
                continue;

            var descriptor = result.Value.Descriptor;
            await processor.ProcessJobAsync(descriptor, cancellationToken);
        }
    }
}