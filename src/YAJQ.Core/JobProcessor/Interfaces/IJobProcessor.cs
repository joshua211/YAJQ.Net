using YAJQ.Core.Common;
using YAJQ.Core.JobQueue.Interfaces;

namespace YAJQ.Core.JobProcessor.Interfaces;

public interface IJobProcessor
{
    string ProcessorId { get; }
    bool IsProcessing { get; }

    ExecutionResult<ProcessingResult> ProcessJob(IJobDescriptor descriptor,
        CancellationToken cancellationToken = default);
}

public record ProcessingResult(TimeSpan ProcessingTime, Exception? LastException, int FailedAttempts,
    bool WasSuccess = true);