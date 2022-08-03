namespace Fastjob.Core.Interfaces;

public interface IJobProcessor
{
    string ProcessorId { get; }
    
    Task ProcessJobAsync(IJobDescriptor descriptor, CancellationToken cancellationToken = default);
}