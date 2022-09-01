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

    public async Task<ExecutionResult<string>> AddJobAsync(IJobDescriptor descriptor, string? id = null)
    {
        var jobId = id is null ? JobId.New : JobId.With(id);
        var job = new PersistedJob(jobId, descriptor, string.Empty);
        var result = await persistence.SaveJobAsync(job);

        return result.WasSuccess ? jobId.Value : Error.StorageError();
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

    public async Task<ExecutionResult<Success>> CompleteJobAsync(string jobId, bool wasSuccess = true)
    {
        var get = await persistence.GetJobAsync(jobId);
        if (!get.WasSuccess) return get.Error;

        var job = get.Value;
        job.State = wasSuccess ? JobState.Completed : JobState.Failed;

        var update = await persistence.UpdateJobAsync(job);
        if (!update.WasSuccess)
            return update.Error;

        var result = await persistence.ArchiveJobAsync(job);

        return result.WasSuccess ? ExecutionResult<Success>.Success : result.Error;
    }

    private void HandleJobEvent(object? sender, string e)
    {
        OnJobEvent?.Invoke(this, e);
    }
}