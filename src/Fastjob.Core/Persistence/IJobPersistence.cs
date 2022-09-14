using Fastjob.Core.Common;

namespace Fastjob.Core.Persistence;

public interface IJobPersistence
{
    public event EventHandler<string>? NewJob;
    Task<ExecutionResult<Success>> SaveJobAsync(PersistedJob persistedJob);
    Task<ExecutionResult<PersistedJob>> GetJobAsync(string id);
    Task<ExecutionResult<Success>> UpdateJobAsync(PersistedJob persistedJob);
    Task<ExecutionResult<Success>> RemoveJobAsync(string jobId);
    Task<ExecutionResult<Success>> RemoveAllJobsAsync();
    Task<ExecutionResult<Success>> ArchiveJobAsync(PersistedJob job);
    Task<ExecutionResult<JobCursor>> IncreaseCursorAsync();
    Task<ExecutionResult<PersistedJob>> GetJobAtCursorAsync();

    Task<ExecutionResult<JobCursor>> GetCursorAsync();

    //TODO Create IJobArchive
    Task<ExecutionResult<IEnumerable<PersistedJob>>> GetCompletedJobsAsync();
    Task<ExecutionResult<IEnumerable<PersistedJob>>> GetArchivedJobsAsync();
    Task<ExecutionResult<IEnumerable<PersistedJob>>> GetFailedJobsAsync();
}