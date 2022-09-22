using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YAJQ.Core;
using YAJQ.Core.Archive.Interfaces;
using YAJQ.Core.JobHandler;
using YAJQ.Core.JobHandler.Interfaces;
using YAJQ.Core.JobProcessor;
using YAJQ.Core.JobProcessor.Interfaces;
using YAJQ.Core.JobQueue;
using YAJQ.Core.JobQueue.Interfaces;
using YAJQ.Core.Persistence;
using YAJQ.Core.Persistence.Interfaces;
using YAJQ.Core.Utils;
using YAJQ.Persistence.Memory;

namespace YAJQ.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddYAJQ(this IServiceCollection services)
    {
        RegisterServices(services);
        services.Configure<YAJQOptions>(_ => { });

        return services;
    }

    public static IServiceCollection AddYAJQ(this IServiceCollection services, Action<YAJQOptions> configure)
    {
        RegisterServices(services);
        services.Configure(configure);

        return services;
    }

    public static IServiceCollection AddYAJQ(this IServiceCollection services, IConfigurationSection section)
    {
        RegisterServices(services);
        services.Configure<YAJQOptions>(section);

        return services;
    }

    private static IServiceCollection RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IJobQueue, JobQueue>();
        services.AddTransient<IJobProcessor, DefaultJobProcessor>();
        services.AddTransient<IJobRepository, JobRepository>();
        services.AddTransient<IJobHandler, MultiProcessorJobHandler>();
        services.AddTransient<IModuleHelper, ModuleHelper>();
        services.AddTransient<IJobProcessorFactory, JobProcessorFactory>();
        services.AddTransient<IProcessorSelectionStrategy, RoundRobinProcessorSelectionStrategy>();
        services.AddTransient<ITransientFaultHandler, DefaultTransientFaultHandler>();
        services.AddTransient<IOpenJobProvider, OpenJobProvider>();
        services.AddTransient<IScheduledJobSubHandler, ScheduledJobSubHandler>();
        services.AddSingleton<IJobPersistence, MemoryPersistence>();
        services.AddSingleton<IJobArchive, MemoryArchive>();
        services.AddTransient(provider =>
            provider.GetRequiredService<IOptions<YAJQOptions>>().Value);

        return services;
    }
}