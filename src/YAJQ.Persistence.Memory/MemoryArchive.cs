using YAJQ.Core.Archive.Interfaces;
using YAJQ.Core.Common;
using YAJQ.Core.Persistence;

namespace YAJQ.Persistence.Memory;

public class MemoryArchive : IJobArchive
{
    private readonly List<ArchivedJob> archivedJobs;

    public MemoryArchive()
    {
        archivedJobs = new List<ArchivedJob>();
    }

    public Task<ExecutionResult<Success>> AddToArchiveAsync(ArchivedJob job)
    {
        archivedJobs.Add(job);

        return Task.FromResult(new ExecutionResult<Success>());
    }

    public Task<ExecutionResult<IEnumerable<ArchivedJob>>> GetCompletedJobsAsync()
    {
        var completed = archivedJobs.Where(a => a.State == JobState.Completed);
        return Task.FromResult(new ExecutionResult<IEnumerable<ArchivedJob>>(completed));
    }

    public Task<ExecutionResult<IEnumerable<ArchivedJob>>> GetArchivedJobsAsync()
    {
        return Task.FromResult(new ExecutionResult<IEnumerable<ArchivedJob>>(archivedJobs));
    }

    public Task<ExecutionResult<IEnumerable<ArchivedJob>>> GetFailedJobsAsync()
    {
        var failed = archivedJobs.Where(a => a.State == JobState.Failed);
        return Task.FromResult(new ExecutionResult<IEnumerable<ArchivedJob>>(failed));
    }
}