namespace YAJQ.Core.JobHandler;

public interface IJobHandler
{
    public string HandlerId { get; }
    Task Start(CancellationToken cancellationToken);
}