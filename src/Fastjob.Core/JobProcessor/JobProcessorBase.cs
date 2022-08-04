using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;

namespace Fastjob.Core.JobProcessor;

public abstract class JobProcessorBase : IJobProcessor
{
    public string ProcessorId { get; init; }

    public JobProcessorBase()
    {
        ProcessorId = Guid.NewGuid().ToString().Split('-').First();
    }

    public abstract Task<ExecutionResult<Success>> ProcessJobAsync(IJobDescriptor descriptor, CancellationToken cancellationToken = default);
}