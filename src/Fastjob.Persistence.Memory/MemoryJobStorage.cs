using System.Collections.Concurrent;
using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;
using Fastjob.Core.Persistence;

namespace Fastjob.Persistence.Memory;

public class MemoryJobStorage : IJobStorage
{
    private static readonly BlockingCollection<PersistedJob> List = new();

    public Task AddJobAsync(IJobDescriptor descriptor, CancellationToken token = default)
    {
        var jobId = Guid.NewGuid().ToString();
        List.TryAdd(new PersistedJob(jobId, descriptor, string.Empty));

        return Task.CompletedTask;
    }

    public Task<string> GetNextJobIdAsync()
    {
        var nextJob = List.FirstOrDefault();

        return Task.FromResult(nextJob is null ? string.Empty : nextJob.Id);
    }

    public Task<ExecutionResult<IPersistedJob>> GetJobAsync(string id)
    {
        var job = List.FirstOrDefault(j => j.Id == id);
        if (job is null)
            return Task.FromResult(new ExecutionResult<IPersistedJob>(Error.NotFound()));

        return Task.FromResult(new ExecutionResult<IPersistedJob>(job as IPersistedJob));
    }

    public async Task<ExecutionResult<IPersistedJob>> TryMarkJobAsync(string jobId, string concurrencyMark)
    {
        var result = await GetJobAsync(jobId);
        if (!result.WasSuccess)
            return result.Error;

        if (!string.IsNullOrEmpty(result.Value.ConcurrencyTag))
            return Error.AlreadyMarked();

        result.Value.ConcurrencyTag = concurrencyMark;

        return result;
    }
}