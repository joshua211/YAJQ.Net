using Fastjob.Core;
using Fastjob.Core.JobHandler;
using Fastjob.Core.JobProcessor;
using Fastjob.Core.JobQueue;
using Fastjob.Core.Persistence;
using Fastjob.Core.Utils;
using Fastjob.Persistence.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fastjob.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFastjob(this IServiceCollection services)
    {
        RegisterServices(services);
        services.Configure<FastjobOptions>(_ => { });

        return services;
    }

    public static IServiceCollection AddFastjob(this IServiceCollection services, Action<FastjobOptions> configure)
    {
        RegisterServices(services);
        services.Configure<FastjobOptions>(configure);

        return services;
    }

    public static IServiceCollection AddFastjob(this IServiceCollection services, IConfigurationSection section)
    {
        RegisterServices(services);
        services.Configure<FastjobOptions>(section);

        return services;
    }

    private static IServiceCollection RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IJobQueue, JobQueue>();
        services.AddTransient<IJobProcessor, DefaultJobProcessor>();
        services.AddTransient<IJobRepository, JobRepository>();
        services.AddTransient<IJobHandler, MultiProcessorJobHandler>();
        services.AddTransient<IModuleHelper, ModuleHelper>();
        services.AddSingleton<IJobPersistence>(new MemoryPersistence());
        services.AddTransient<FastjobOptions>(provider =>
            provider.GetRequiredService<IOptions<FastjobOptions>>().Value);

        return services;
    }
}