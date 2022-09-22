namespace YAJQ.Core.JobQueue.Interfaces;

/// <summary>
/// Model Class that holds all required data to execute a job
/// </summary>
public interface IJobDescriptor
{
    string JobName { get; }
    string FullTypeName { get; }
    string ModuleName { get; }
    IEnumerable<object> Args { get; }
}