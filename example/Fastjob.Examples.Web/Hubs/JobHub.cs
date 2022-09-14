using Microsoft.AspNetCore.SignalR;

namespace Fastjob.Examples.Web.Hubs;

public class JobHub : Hub<IJobHub>
{
    public async Task CompleteJob(string jobId)
    {
        await Clients.All.CompleteJob(jobId);
    }
}