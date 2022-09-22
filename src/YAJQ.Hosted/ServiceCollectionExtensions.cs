using Microsoft.Extensions.DependencyInjection;

namespace YAJQ.Hosted;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHostedJobHandler(this IServiceCollection services)
    {
        services.AddHostedService<HostedJobHandler>();

        return services;
    }
}