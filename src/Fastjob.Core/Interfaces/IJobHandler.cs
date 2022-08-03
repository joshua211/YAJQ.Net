namespace Fastjob.Core.Interfaces;

public interface IJobHandler
{
    Task Start(CancellationToken cancellationToken);
}