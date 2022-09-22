using YAJQ.Core.JobProcessor;
using YAJQ.Core.Persistence;

namespace YAJQ.Core.JobHandler;

public interface IScheduledJobSubHandler
{
    void Start(string handlerId, IProcessorSelectionStrategy selectionStrategy,
        Func<PersistedJob, IJobProcessor, CancellationToken, Task> processJob,
        CancellationToken cancellationToken = default);

    void AddScheduledJob(PersistedJob job);
}