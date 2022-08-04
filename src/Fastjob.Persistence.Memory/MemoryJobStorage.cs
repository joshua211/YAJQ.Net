using System.Collections.Concurrent;
using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;
using Fastjob.Core.Persistence;

namespace Fastjob.Persistence.Memory;

public class MemoryJobStorage : IJobStorage
{
    private static ConcurrentDictionary<string, PersistedJob> Storage =
        new ConcurrentDictionary<string, PersistedJob>();

    private static ConcurrentQueue<PersistedJob> Queue = new ConcurrentQueue<PersistedJob>();

    public async Task<ExecutionResult<string>> AddJobAsync(IJobDescriptor descriptor, CancellationToken token = default)
    {
        var jobId = Guid.NewGuid().ToString();
        var job = new PersistedJob(jobId, descriptor, string.Empty);
        var result = Storage.TryAdd(jobId, job);
        if (!result)
            return Error.StorageError();

        Queue.Enqueue(job);

        return new ExecutionResult<string>(jobId);
    }

    public async Task<ExecutionResult<IPersistedJob>> GetNextJobAsync()
    {
        if (!Queue.Any())
            Queue = new ConcurrentQueue<PersistedJob>(Storage.Values);

        var result = Queue.TryDequeue(out var nextJob);
        if (!result)
            return Error.NotFound();

        return nextJob;
    }

    public Task<ExecutionResult<IPersistedJob>> GetJobAsync(string id)
    {
        var result = Storage.TryGetValue(id, out var job);
        if (!result)
            return Task.FromResult(new ExecutionResult<IPersistedJob>(Error.NotFound()));

        return Task.FromResult(new ExecutionResult<IPersistedJob>(job as IPersistedJob));
    }

    public async Task<ExecutionResult<IPersistedJob>> TryMarkAndGetJobAsync(string jobId, string concurrencyMark)
    {
        var result = await GetJobAsync(jobId);
        if (!result.WasSuccess)
            return result.Error;

        if (!string.IsNullOrEmpty(result.Value.ConcurrencyTag))
            return Error.AlreadyMarked();

        result.Value.ConcurrencyTag = concurrencyMark;

        return result;
    }

    public async Task<ExecutionResult<Success>> RemoveJobAsync(string jobId)
    {
        var result = Storage.TryRemove(jobId, out var _);
        if (!result)
            return Error.NotFound();

        return new Success();
    }

    public Task<ExecutionResult<Success>> ClearAsync()
    {
        Storage.Clear();
        Queue.Clear();

        return Task.FromResult(ExecutionResult<Success>.Success);
    }
}