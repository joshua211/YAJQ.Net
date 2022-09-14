using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;

namespace Fastjob.Core.JobProcessor;

public interface IJobProcessor
{
    string ProcessorId { get; }
    bool IsProcessing { get; }

    ExecutionResult<ProcessingResult> ProcessJob(IJobDescriptor descriptor,
        CancellationToken cancellationToken = default);
}

public record ProcessingResult(TimeSpan ProcessingTime, Exception? LastException, int FailedAttempts);