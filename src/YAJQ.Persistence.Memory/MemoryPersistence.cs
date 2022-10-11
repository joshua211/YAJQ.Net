using System.Collections.Immutable;
using YAJQ.Core.Archive.Interfaces;
using YAJQ.Core.Common;
using YAJQ.Core.Persistence;
using YAJQ.Core.Persistence.Interfaces;

namespace YAJQ.Persistence.Memory;

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

    public async Task<ExecutionResult<PersistedJob>> UpdateStateAsync(PersistedJob persistedJob, JobState expectedState)
    {
        PersistedJob modifiedJob;
        lock (jobLock)
        {
            var index = jobs.FindIndex(j => Equals(j.Id, persistedJob.Id));
            if (index == -1)
                return Error.NotFound();

            var list = jobs.ToList();
            var currentTrackedJob = list[index];
            if (currentTrackedJob.State != expectedState)
                return Error.OutdatedUpdate();

            modifiedJob = persistedJob.DeepCopy();
            modifiedJob.Refresh();
            list[index] = modifiedJob;
            jobs = list.ToImmutableList();
        }

        return modifiedJob;
    }

    public async Task<ExecutionResult<PersistedJob>> UpdateTokenAsync(PersistedJob persistedJob, string expectedToken)
    {
        PersistedJob modifiedJob;
        lock (jobLock)
        {
            var index = jobs.FindIndex(j => Equals(j.Id, persistedJob.Id));
            if (index == -1)
                return Error.NotFound();

            var list = jobs.ToList();
            var currentTrackedJob = list[index];
            if (currentTrackedJob.ConcurrencyToken != expectedToken)
                return Error.OutdatedUpdate();

            modifiedJob = persistedJob.DeepCopy();
            modifiedJob.Refresh();
            list[index] = modifiedJob;
            jobs = list.ToImmutableList();
        }

        return modifiedJob;
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

            var job = jobs[cursor.CurrentCursor];
            cursor = cursor.Increase();

            return Task.FromResult<ExecutionResult<PersistedJob>>(job.DeepCopy());
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
            var modifiedJob = persistedJob.DeepCopy();
            modifiedJob.Refresh();
            list[index] = modifiedJob;
            jobs = list.ToImmutableList();
        }

        return ExecutionResult<Success>.Success;
    }
}