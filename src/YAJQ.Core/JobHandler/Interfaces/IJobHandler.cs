namespace YAJQ.Core.JobHandler.Interfaces;
public interface IJobHandler
{
    /// <summary>
    /// A unique Id to identify each handler.
    /// Defaults to {Machine-Name}:{RandomString}
    /// </summary>
    public string HandlerId { get; }
    
    /// <summary>
    /// Starts the JobHandler
    /// </summary>
    /// <param name="cancellationToken">Required CancellationToken to properly stop the handler</param>
    /// <returns></returns>
    Task Start(CancellationToken cancellationToken);
}