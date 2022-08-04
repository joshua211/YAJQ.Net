using Fastjob.Core.Common;

namespace Fastjob.Core.Interfaces;

public interface IJobStorage
{
    Task<ExecutionResult<string>> AddJobAsync(IJobDescriptor descriptor, CancellationToken token = default);
    Task<ExecutionResult<IPersistedJob>> GetNextJobAsync();
    Task<ExecutionResult<IPersistedJob>> GetJobAsync(string id);
    Task<ExecutionResult<IPersistedJob>> TryMarkAndGetJobAsync(string jobId, string concurrencyMark);
    Task<ExecutionResult<Success>> RemoveJobAsync(string jobId);
    Task<ExecutionResult<Success>> ClearAsync();
}