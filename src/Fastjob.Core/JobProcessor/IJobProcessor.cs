using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;

namespace Fastjob.Core.JobProcessor;

public interface IJobProcessor
{
    string ProcessorId { get; }
    bool IsProcessing { get; }

    ExecutionResult<Success> ProcessJob(IJobDescriptor descriptor,
        CancellationToken cancellationToken = default);
}