using YAJQ.Core.Common;
using YAJQ.Core.JobProcessor.Interfaces;
using YAJQ.Core.JobQueue;
using YAJQ.Core.JobQueue.Interfaces;

namespace YAJQ.Core.JobProcessor;

public abstract class JobProcessorBase : IJobProcessor
{
    public JobProcessorBase()
    {
        ProcessorId = Guid.NewGuid().ToString().Split('-').First();
    }

    public string ProcessorId { get; init; }
    public abstract bool IsProcessing { get; protected set; }

    public abstract ExecutionResult<ProcessingResult> ProcessJob(IJobDescriptor descriptor,
        CancellationToken cancellationToken = default);
}