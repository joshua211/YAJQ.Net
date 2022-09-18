using System.Collections.Immutable;
using Fastjob.Core.Archive;
using Fastjob.Core.Common;
using Fastjob.Core.Persistence;

namespace Fastjob.Persistence.Memory;

public class MemoryPersistence : IJobPersistence
{
    private readonly IJobArchive archive;
    private readonly object jobLock;
    private JobCursor cursor;
    private ImmutableList<PersistedJob> jobs;

    public MemoryPersistence(IJobArchive archive)
    {
        this.archive = archive;
        jobLock = new object();
        jobs = ImmutableList<PersistedJob>.Empty;
        cursor = JobCursor.Empty;
    }

    public event EventHandler<string>? NewJob;

    public Task<ExecutionResult<Success>> SaveJobAsync(PersistedJob persistedJob)
    {
        lock (jobLock)
        {
            var jobCopy = persistedJob.DeepCopy();
            jobs = jobs.Append(jobCopy).ToImmutableList();
            cursor = cursor.IncreaseMax();
        }

        NewJob?.Invoke(this, persistedJob.Id);

        return Task.FromResult(ExecutionResult<Success>.Success);
    }

    public async Task<ExecutionResult<PersistedJob>> GetJobAsync(string id)
    {
        PersistedJob job;
        lock (jobLock)
        {
            job = jobs.FirstOrDefault(j => j.Id == id);
        }

        return job is null ? Error.NotFound() : job.DeepCopy();
    }

    public async Task<ExecutionResult<Success>> UpdateStateAsync(PersistedJob persistedJob, JobState expectedState)
    {
        lock (jobLock)
        {
            var index = jobs.FindIndex(j => Equals(j.Id, persistedJob.Id));
            if (index == -1)
                return Error.NotFound();

            var list = jobs.ToList();
            var currentTrackedJob = list[index];
            if (currentTrackedJob.State != expectedState)
                return Error.OutdatedUpdate();

            persistedJob.Refresh();
            list[index] = persistedJob;
            jobs = list.ToImmutableList();
        }

        return ExecutionResult<Success>.Success;
    }

    public async Task<ExecutionResult<Success>> UpdateTokenAsync(PersistedJob persistedJob, string expectedToken)
    {
        lock (jobLock)
        {
            var index = jobs.FindIndex(j => Equals(j.Id, persistedJob.Id));
            if (index == -1)
                return Error.NotFound();

            var list = jobs.ToList();
            var currentTrackedJob = list[index];
            if (currentTrackedJob.ConcurrencyToken != expectedToken)
                return Error.OutdatedUpdate();

            persistedJob.Refresh();
            list[index] = persistedJob;
            jobs = list.ToImmutableList();
        }

        return ExecutionResult<Success>.Success;
    }

    public async Task<ExecutionResult<Success>> RemoveJobAsync(string jobId)
    {
        var job = await GetJobAsync(jobId);
        if (!job.WasSuccess)
            return job.Error;

        lock (jobLock)
        {
            jobs = jobs.Remove(job.Value);
            cursor = cursor.DecreaseMax();
        }

        return ExecutionResult<Success>.Success;
    }

    public Task<ExecutionResult<Success>> RemoveAllJobsAsync()
    {
        lock (jobs)
        {
            jobs = ImmutableList<PersistedJob>.Empty;
            cursor = JobCursor.Empty;
        }

        return Task.FromResult(ExecutionResult<Success>.Success);
    }

    public async Task<ExecutionResult<Success>> ArchiveJobAsync(ArchivedJob job)
    {
        var remove = await RemoveJobAsync(job.Id);
        if (!remove.WasSuccess)
            return remove.Error;

        var arch = await archive.AddToArchiveAsync(job);

        return !arch.WasSuccess ? arch.Error : ExecutionResult<Success>.Success;
    }

    public Task<ExecutionResult<JobCursor>> IncreaseCursorAsync()
    {
        lock (jobLock)
        {
            cursor = cursor.Increase();
        }

        return Task.FromResult<ExecutionResult<JobCursor>>(cursor);
    }

    public Task<ExecutionResult<PersistedJob>> GetJobAtCursorAsync()
    {
        lock (jobLock)
        {
            if (cursor.MaxCursor == 0)
                return Task.FromResult<ExecutionResult<PersistedJob>>(Error.CursorOutOfRange());

            var job = jobs[cursor.CurrentCursor - 1];
            cursor = cursor.Increase();
            return Task.FromResult<ExecutionResult<PersistedJob>>(job);
        }
    }

    public Task<ExecutionResult<JobCursor>> GetCursorAsync()
    {
        return Task.FromResult<ExecutionResult<JobCursor>>(cursor);
    }


    public async Task<ExecutionResult<Success>> UpdateJobAsync(PersistedJob persistedJob)
    {
        lock (jobLock)
        {
            var index = jobs.FindIndex(j => Equals(j.Id, persistedJob.Id));
            if (index == -1)
                return Error.NotFound();

            var list = jobs.ToList();
            var currentTrackedJob = list[index];
            persistedJob.Refresh();
            list[index] = persistedJob;
            jobs = list.ToImmutableList();
        }

        return ExecutionResult<Success>.Success;
    }
}