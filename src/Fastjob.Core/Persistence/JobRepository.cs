﻿using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;

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

    public async Task<ExecutionResult<string>> AddJobAsync(IJobDescriptor descriptor, string? id = null)
    {
        var jobId = id is null ? JobId.New : JobId.With(id);
        var job = PersistedJob.Asap(jobId, descriptor);
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

    public async Task<ExecutionResult<PersistedJob>> TryGetAndMarkJobAsync(string jobId, string concurrencyMark)
    {
        var job = await GetJobAsync(jobId);
        if (!job.WasSuccess)
            return job.Error;

        if (!string.IsNullOrWhiteSpace(job.Value.ConcurrencyToken))
            return Error.AlreadyMarked();

        job.Value.SetTag(concurrencyMark);
        var updateResult = await persistence.UpdateJobAsync(job.Value);

        return updateResult.Match<ExecutionResult<PersistedJob>>(success => job.Value, error => error);
    }

    public async Task<ExecutionResult<Success>> CompleteJobAsync(string jobId, bool wasSuccess = true)
    {
        var get = await persistence.GetJobAsync(jobId);
        if (!get.WasSuccess) return get.Error;

        var job = get.Value;
        if (wasSuccess)
            job.Completed();
        else
            job.Failed();

        var update = await persistence.UpdateJobAsync(job);
        if (!update.WasSuccess)
            return update.Error;

        var result = await persistence.ArchiveJobAsync(job);
        Update?.Invoke(this, new JobEvent(JobId.With(jobId), wasSuccess ? JobState.Completed : JobState.Failed));

        return result.WasSuccess ? ExecutionResult<Success>.Success : result.Error;
    }

    public async Task<ExecutionResult<Success>> RefreshTokenAsync(JobId jobId, string token)
    {
        var job = await persistence.GetJobAsync(jobId);
        if (!job.WasSuccess)
            return job.Error;

        if (job.Value.ConcurrencyToken != token)
            return Error.WrongToken();

        var update = await persistence.UpdateJobAsync(job.Value);

        return !update.WasSuccess ? update.Error : ExecutionResult<Success>.Success;
    }

    private void OnNewJob(object? sender, string e)
    {
        Update?.Invoke(this, new JobEvent(JobId.With(e), JobState.Pending));
    }
}