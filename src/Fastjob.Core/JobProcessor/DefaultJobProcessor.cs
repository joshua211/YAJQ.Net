﻿using System.Reflection;
using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;
using Fastjob.Core.Utils;
using Microsoft.Extensions.Logging;

namespace Fastjob.Core.JobProcessor;

public class DefaultJobProcessor : JobProcessorBase
{
    private readonly ITransientFaultHandler faultHandler;
    private readonly ILogger<DefaultJobProcessor> logger;
    private readonly IModuleHelper moduleHelper;
    private readonly IServiceProvider serviceProvider;

    public DefaultJobProcessor(IModuleHelper moduleHelper, IServiceProvider serviceProvider,
        ILogger<DefaultJobProcessor> logger, ITransientFaultHandler faultHandler)
    {
        this.moduleHelper = moduleHelper;
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        this.faultHandler = faultHandler;
    }

    public override bool IsProcessing { get; protected set; }

    public override ExecutionResult<Success> ProcessJob(IJobDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        IsProcessing = true;
        try
        {
            if (!moduleHelper.IsModuleLoaded(descriptor.ModuleName))
            {
                logger.LogWarning("The required Module {Module} is not loaded in the current process",
                    descriptor.ModuleName);
                return Error.ModuleNotLoaded();
            }

            var jobType = GetType(descriptor.FullTypeName);
            if (jobType is null)
            {
                logger.LogWarning("Failed to get Type {Type}, aborting job", descriptor.FullTypeName);
                return Error.TypeNotFound();
            }

            var methodBase = (MethodBase?) jobType.FindMembers(MemberTypes.Method,
                BindingFlags.Public | BindingFlags.Instance,
                (info, _) => info.Name == descriptor.JobName, null).FirstOrDefault();

            if (methodBase is null)
            {
                logger.LogWarning("Failed to find Method {Method} on Type {Type}, aborting job", descriptor.JobName,
                    jobType.Name);
                return Error.MethodBaseNotFound();
            }

            var jobObject = serviceProvider.GetService(jobType);
            if (jobObject is null)
                return Error.ServiceNotFound();

            var result = faultHandler.Try(() => Execute(methodBase, jobObject, descriptor.Args.ToArray()));
            if (!result.WasSuccess)
            {
                logger.LogWarning(result.LastException, "Exception while trying to process Job {Name}",
                    descriptor.JobName);

                return Error.ExecutionFailed();
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to execute job");
            return Error.ExecutionFailed();
        }
        finally
        {
            IsProcessing = false;
        }

        return new Success();
    }

    private void Execute(MethodBase method, object jobObject, object[] args)
    {
        object invokeResult = null;
        invokeResult = method.Invoke(jobObject, args);
        if (invokeResult is Task task)
        {
            task.Wait();
        }
    }

    private static Type? GetType(string name)
    {
        var type = Type.GetType(name);
        if (type != null) return type;

        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = a.GetType(name);
            if (type != null)
                return type;
        }

        return null;
    }
}