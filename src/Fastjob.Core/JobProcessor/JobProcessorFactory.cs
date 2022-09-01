using Fastjob.Core.Utils;
using Microsoft.Extensions.Logging;

namespace Fastjob.Core.JobProcessor;

public class JobProcessorFactory : IJobProcessorFactory
{
    private readonly ILogger<DefaultJobProcessor> logger;
    private readonly IModuleHelper moduleHelper;
    private readonly IServiceProvider serviceProvider;

    public JobProcessorFactory(IModuleHelper moduleHelper, IServiceProvider serviceProvider,
        ILogger<DefaultJobProcessor> logger)
    {
        this.moduleHelper = moduleHelper;
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    public IJobProcessor New()
    {
        return new DefaultJobProcessor(moduleHelper, serviceProvider, logger);
    }
}