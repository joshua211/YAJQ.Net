using YAJQ.Core.Common;

namespace YAJQ.Core.Persistence.Interfaces;

public interface IJobPersistence
{
    public event EventHandler<string>? NewJob;
    Task<ExecutionResult<Success>> SaveJobAsync(PersistedJob persistedJob);
    Task<ExecutionResult<PersistedJob>> GetJobAsync(string id);
    Task<ExecutionResult<PersistedJob>> UpdateStateAsync(PersistedJob persistedJob, JobState expectedState);
    Task<ExecutionResult<PersistedJob>> UpdateTokenAsync(PersistedJob persistedJob, string expectedToken);
    Task<ExecutionResult<Success>> RemoveJobAsync(string jobId);
    Task<ExecutionResult<Success>> RemoveAllJobsAsync();
    Task<ExecutionResult<Success>> ArchiveJobAsync(ArchivedJob job);
    Task<ExecutionResult<JobCursor>> IncreaseCursorAsync();
    Task<ExecutionResult<PersistedJob>> GetJobAtCursorAsync();
    Task<ExecutionResult<JobCursor>> GetCursorAsync();
}