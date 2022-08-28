using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;

namespace Fastjob.Core.Persistence;

public interface IJobPersistence
{
    public event EventHandler<string>? OnJobEvent;
    Task<ExecutionResult<Success>> SaveJobAsync(PersistedJob persistedJob);
    Task<ExecutionResult<PersistedJob>> GetJobAsync(string id);
    Task<ExecutionResult<Success>> UpdateJobAsync(PersistedJob persistedJob);
    Task<ExecutionResult<Success>> RemoveJobAsync(string jobId);
    Task<ExecutionResult<Success>> RemoveAllJobsAsync();
    Task<ExecutionResult<JobCursor>> IncreaseCursorAsync();
    Task<ExecutionResult<PersistedJob>> GetJobAtCursorAsync();
    Task<ExecutionResult<JobCursor>> GetCursorAsync();
}