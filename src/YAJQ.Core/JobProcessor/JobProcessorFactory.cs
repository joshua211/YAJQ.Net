using YAJQ.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace YAJQ.Core.JobProcessor;

public class JobProcessorFactory : IJobProcessorFactory
{
    private readonly IServiceProvider serviceProvider;

    public JobProcessorFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IJobProcessor New()
    {
        var moduleHelper = serviceProvider.GetRequiredService<IModuleHelper>();
        var logger = serviceProvider.GetRequiredService<ILogger<DefaultJobProcessor>>();
        var faultHandler = serviceProvider.GetRequiredService<ITransientFaultHandler>();

        return new DefaultJobProcessor(moduleHelper, serviceProvider, logger, faultHandler);
    }
}