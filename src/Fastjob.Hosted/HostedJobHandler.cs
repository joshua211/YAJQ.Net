using Fastjob.Core.JobHandler;
using Microsoft.Extensions.Hosting;

namespace Fastjob.Hosted;

public class HostedJobHandler : BackgroundService
{
    private readonly IJobHandler handler;

    public HostedJobHandler(IJobHandler handler)
    {
        this.handler = handler;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await handler.Start(stoppingToken);
    }
}