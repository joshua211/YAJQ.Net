using System.Text.Json.Serialization.Metadata;
using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;

namespace Fastjob.Core.Persistence;

public class JobRepository : IJobRepository
{
    private readonly IJobPersistence persistence;

    public JobRepository(IJobPersistence persistence)
    {
        this.persistence = persistence;
        persistence.OnJobEvent += HandleJobEvent;
    }

    public event EventHandler<string>? OnJobEvent;

    public async Task<ExecutionResult<string>> AddJobAsync(IJobDescriptor descriptor)
    {
        var jobId = Guid.NewGuid().ToString();
        var job = new PersistedJob(jobId, descriptor, string.Empty);
        var result = await persistence.SaveJobAsync(job);

        return result.WasSuccess ? jobId : Error.StorageError();
    }

    public async Task<ExecutionResult<PersistedJob>> GetNextJobAsync()
    {
        var job = await persistence.GetJobAtCursorAsync();
        if (!job.WasSuccess)
            return job.Error;

        await persistence.IncreaseCursorAsync();

        return job.Value;
    }

    public async Task<ExecutionResult<PersistedJob>> GetJobAsync(string id)
    {
        var job = await persistence.GetJobAsync(id);
        return job.Match<ExecutionResult<PersistedJob>>(job => job, err => err);
    }

    public async Task<ExecutionResult<PersistedJob>> TryGetAndMarkJobAsync(string jobId, string concurrencyMark)
    {
        var job = await GetJobAsync(jobId);
        if (!job.WasSuccess)
            return job.Error;

        if (!string.IsNullOrWhiteSpace(job.Value.ConcurrencyTag))
            return Error.AlreadyMarked();

        job.Value.ConcurrencyTag = concurrencyMark;
        var updateResult = await persistence.UpdateJobAsync(job.Value);

        return updateResult.Match<ExecutionResult<PersistedJob>>(success => job.Value, error => error);
    }

    public Task<ExecutionResult<Success>> CompleteJobAsync(string jobId)
    {
        throw new NotImplementedException();
    }

    private void HandleJobEvent(object? sender, string e)
    {
        OnJobEvent?.Invoke(this, e);
    }
}