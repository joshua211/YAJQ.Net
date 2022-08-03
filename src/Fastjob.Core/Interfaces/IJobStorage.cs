using Fastjob.Core.Common;

namespace Fastjob.Core.Interfaces;

public interface IJobStorage
{
    Task AddJobAsync(IJobDescriptor descriptor, CancellationToken token = default);
    Task<string> GetNextJobIdAsync();
    Task<ExecutionResult<IPersistedJob>> GetJobAsync(string id);
    Task<ExecutionResult<IPersistedJob>> TryMarkJobAsync(string jobId, string concurrencyMark);
}