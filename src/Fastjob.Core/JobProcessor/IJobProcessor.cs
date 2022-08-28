using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;

namespace Fastjob.Core.JobProcessor;

public interface IJobProcessor
{
    string ProcessorId { get; }
    
    Task<ExecutionResult<Success>> ProcessJobAsync(IJobDescriptor descriptor, CancellationToken cancellationToken = default);
}