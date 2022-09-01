using Fastjob.Core.JobHandler;
using Fastjob.Core.JobProcessor;
using Fastjob.Core.JobQueue;
using Fastjob.Core.Persistence;
using Fastjob.Core.Utils;
using Fastjob.Hosted;
using Fastjob.Persistence.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fastjob.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFastjob(this IServiceCollection services)
    {
        services.AddTransient<IJobQueue, JobQueue>();
        services.AddTransient<IJobProcessor, DefaultJobProcessor>();
        services.AddTransient<IJobRepository, JobRepository>();
        services.AddTransient<IJobHandler, DefaultJobHandler>();
        services.AddTransient<IModuleHelper, ModuleHelper>();
        services.AddSingleton<IJobPersistence>(new MemoryPersistence());
        return services;
    }
}