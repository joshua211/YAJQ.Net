namespace Fastjob.Core.JobHandler;

public interface IJobHandler
{
    Task Start(CancellationToken cancellationToken);
}