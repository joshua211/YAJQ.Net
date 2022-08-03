using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Fastjob.Core.Interfaces;
using Fastjob.Core.Utils;
using Microsoft.Extensions.Logging;

namespace Fastjob.Core.JobProcessor;

public class DefaultJobProcessor : JobProcessorBase
{
    private readonly IModuleHelper moduleHelper;
    private readonly ILogger logger;
    private readonly IServiceProvider serviceProvider;

    public DefaultJobProcessor(IModuleHelper moduleHelper, IServiceProvider serviceProvider,
        ILogger logger)
    {
        this.moduleHelper = moduleHelper;
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    public override async Task ProcessJobAsync(IJobDescriptor descriptor, CancellationToken cancellationToken = default)
    {
        if (!moduleHelper.IsModuleLoaded(descriptor.ModuleName))
        {
            logger.LogWarning("The required Module {Module} is not loaded in the current process",
                descriptor.ModuleName);
            return;
        }

        var jobType = Type.GetType(descriptor.FullTypeName);
        if (jobType is null)
        {
            logger.LogWarning("Failed to create Type {Type}, aborting job", descriptor.FullTypeName);
            return;
        }

        var methodBase = (MethodBase?) jobType.FindMembers(MemberTypes.Method,
            BindingFlags.Public | BindingFlags.Instance,
            (info, _) => info.Name == descriptor.JobName, null).FirstOrDefault();

        if (methodBase is null)
        {
            logger.LogWarning("Failed to find Method {Method} on Type {Type}, aborting job", descriptor.JobName,
                jobType.Name);
            return;
        }

        var jobObject = serviceProvider.GetService(jobType);
        var invokeResult = methodBase.Invoke(jobObject, descriptor.Args.ToArray());

        if (invokeResult is Task task)
        {
            await task;
        }
    }
}