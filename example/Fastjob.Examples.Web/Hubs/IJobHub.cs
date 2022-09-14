namespace Fastjob.Examples.Web.Hubs;

public interface IJobHub
{
    Task CompleteJob(string jobId);
}