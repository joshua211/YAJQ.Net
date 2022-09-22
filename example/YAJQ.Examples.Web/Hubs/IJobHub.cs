namespace YAJQ.Examples.Web.Hubs;

public interface IJobHub
{
    Task CompleteJob(string jobId);
}