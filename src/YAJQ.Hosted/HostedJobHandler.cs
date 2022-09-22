using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YAJQ.Core.JobHandler;
using YAJQ.Core.JobHandler.Interfaces;

namespace YAJQ.Hosted;

public class HostedJobHandler : BackgroundService
{
    private readonly IJobHandler handler;
    private readonly ILogger<HostedJobHandler> logger;

    public HostedJobHandler(IJobHandler handler, ILogger<HostedJobHandler> logger)
    {
        this.handler = handler;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting Hosted JobHandler");
        await handler.Start(stoppingToken);
        logger.LogInformation("Stopping Hosted JobHandler");
    }
}