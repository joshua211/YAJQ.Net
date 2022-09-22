using YAJQ.Core.Common;
using YAJQ.Core.Interfaces;

namespace YAJQ.Core.Persistence;

public interface IJobRepository
{
    public event EventHandler<JobEvent> Update;

    Task<ExecutionResult<string>> AddJobAsync(IJobDescriptor descriptor, string? id = null,
        DateTimeOffset? scheduledTime = null);

    Task<ExecutionResult<PersistedJob>> GetNextJobAsync();
    Task<ExecutionResult<PersistedJob>> GetJobAsync(string id);
    Task<ExecutionResult<PersistedJob>> TryGetAndMarkJobAsync(PersistedJob job, string concurrencyMark);

    Task<ExecutionResult<Success>> CompleteJobAsync(string jobId, string handlerId, string processorId,
        TimeSpan executionTime, Error? lastError = null, Exception? lastException = null, bool wasSuccess = true);

    Task<ExecutionResult<Success>> RefreshTokenAsync(JobId jobId, string token);
}