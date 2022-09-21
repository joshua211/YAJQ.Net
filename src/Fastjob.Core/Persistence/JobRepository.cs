namespace Fastjob.Core.Persistence;

public class JobRepository : IJobRepository
{
    private readonly IJobPersistence persistence;

    public JobRepository(IJobPersistence persistence)
    {
        this.persistence = persistence;
        persistence.NewJob += OnNewJob;
    }

    public event EventHandler<JobEvent> Update;

    public async Task<ExecutionResult<string>> AddJobAsync(IJobDescriptor descriptor, string? id = null,
        DateTimeOffset? scheduledTime = null)
    {
        var jobId = id is null ? JobId.New : JobId.With(id);
        var job = scheduledTime is null
            ? PersistedJob.Asap(jobId, descriptor)
            : PersistedJob.Scheduled(jobId, descriptor, scheduledTime.Value);
        var result = await persistence.SaveJobAsync(job);

        Update?.Invoke(this, new JobEvent(jobId, JobState.Pending));

        return result.WasSuccess ? jobId.Value : Error.StorageError();
    }

    public async Task<ExecutionResult<PersistedJob>> GetNextJobAsync()
    {
        var job = await persistence.GetJobAtCursorAsync();
        if (!job.WasSuccess)
            return job.Error;

        return job.Value;
    }

    public async Task<ExecutionResult<PersistedJob>> GetJobAsync(string id)
    {
        var job = await persistence.GetJobAsync(id);
        return job.Match<ExecutionResult<PersistedJob>>(job => job, err => err);
    }

    public async Task<ExecutionResult<PersistedJob>> TryGetAndMarkJobAsync(PersistedJob job, string concurrencyMark)
    {
        var oldToken = job.ConcurrencyToken;
        job.SetToken(concurrencyMark);

        var updateResult = await persistence.UpdateTokenAsync(job, oldToken);

        return updateResult.Match<ExecutionResult<PersistedJob>>(modified => modified, error => error.Code switch
        {
            //OutdatedUpdate means it was already marked
            11 => Error.AlreadyMarked(),
            _ => error
        });
    }

    public async Task<ExecutionResult<Success>> RefreshTokenAsync(JobId jobId, string token)
    {
        var job = await persistence.GetJobAsync(jobId);
        if (!job.WasSuccess)
            return job.Error;

        if (job.Value.ConcurrencyToken != token)
            return Error.WrongToken();

        var update = await persistence.UpdateTokenAsync(job.Value, job.Value.ConcurrencyToken);

        return update.Match<ExecutionResult<Success>>(persistedJob => new Success(), error => error);
    }

    public async Task<ExecutionResult<Success>> CompleteJobAsync(string jobId, string handlerId, string processorId,
        TimeSpan executionTime, Error? lastError, Exception? lastException, bool wasSuccess = true)
    {
        var get = await persistence.GetJobAsync(jobId);
        if (!get.WasSuccess) return get.Error;

        var job = get.Value;
        if (wasSuccess)
            job.Complete();
        else
            job.Fail();

        var update = await persistence.UpdateStateAsync(job, JobState.Pending);
        if (!update.WasSuccess)
            return update.Error;

        var result = await persistence.ArchiveJobAsync(job.Archive(handlerId, processorId, executionTime));
        Update?.Invoke(this, new JobEvent(JobId.With(jobId), wasSuccess ? JobState.Completed : JobState.Failed));

        return result.WasSuccess ? ExecutionResult<Success>.Success : result.Error;
    }

    private void OnNewJob(object? sender, string e)
    {
        Update?.Invoke(this, new JobEvent(JobId.With(e), JobState.Pending));
    }
}