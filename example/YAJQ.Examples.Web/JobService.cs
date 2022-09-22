using Microsoft.AspNetCore.SignalR;
using YAJQ.Core.JobQueue;
using YAJQ.Core.JobQueue.Interfaces;
using YAJQ.Examples.Web.Hubs;

namespace YAJQ.Examples.Web;

public class JobService
{
    private readonly IHubContext<JobHub, IJobHub> hubContext;
    private readonly IJobQueue jobQueue;

    public JobService(IJobQueue jobQueue, IHubContext<JobHub, IJobHub> hubContext)
    {
        this.jobQueue = jobQueue;
        this.hubContext = hubContext;
    }

    public async Task AddJob(string id)
    {
        await jobQueue.EnqueueJobAsync(() => SendJobCompleteMessage(id), id);
    }

    public async Task AddScheduledJob(string id, TimeSpan delay)
    {
        await jobQueue.ScheduleJobAsync(() => SendJobCompleteMessage(id), DateTimeOffset.Now.Add(delay),
            id);
    }

    public async Task SendJobCompleteMessage(string id)
    {
        await hubContext.Clients.All.CompleteJob(id);
    }

    public async Task AddExceptionJob(string id)
    {
        await jobQueue.EnqueueJobAsync(() => Throw());
    }

    public void Throw()
    {
        throw new Exception("This was a planned exception");
    }
}