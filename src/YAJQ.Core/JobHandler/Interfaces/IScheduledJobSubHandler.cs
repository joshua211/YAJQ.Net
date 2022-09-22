using YAJQ.Core.JobProcessor.Interfaces;
using YAJQ.Core.Persistence;

namespace YAJQ.Core.JobHandler.Interfaces;

public interface IScheduledJobSubHandler
{
    void Start(string handlerId, IProcessorSelectionStrategy selectionStrategy,
        Func<PersistedJob, IJobProcessor, CancellationToken, Task> processJob,
        CancellationToken cancellationToken = default);

    void AddScheduledJob(PersistedJob job);
}