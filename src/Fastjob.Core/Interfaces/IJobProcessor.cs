using Fastjob.Core.Common;

namespace Fastjob.Core.Interfaces;

public interface IJobProcessor
{
    string ProcessorId { get; }
    
    Task<ExecutionResult<Success>> ProcessJobAsync(IJobDescriptor descriptor, CancellationToken cancellationToken = default);
}