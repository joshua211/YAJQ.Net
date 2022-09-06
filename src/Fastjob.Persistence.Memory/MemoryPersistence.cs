using System.Collections.Immutable;
using Fastjob.Core.Common;
using Fastjob.Core.Persistence;

namespace Fastjob.Persistence.Memory;

public class MemoryPersistence : IJobPersistence
{
    private readonly object jobLock;
    private ImmutableList<PersistedJob> archivedJobs;
    private JobCursor cursor;
    private ImmutableList<PersistedJob> jobs;
    private string s;

    public MemoryPersistence()
    {
        jobLock = new object();
        jobs = ImmutableList<PersistedJob>.Empty;
        archivedJobs = ImmutableList<PersistedJob>.Empty;
        cursor = JobCursor.Empty;
        s = Guid.NewGuid().ToString();
    }

    public event EventHandler<string>? NewJob;

    public Task<ExecutionResult<Success>> SaveJobAsync(PersistedJob persistedJob)
    {
        lock (jobLock)
        {
            jobs = jobs.Append(persistedJob).ToImmutableList();
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

        return job is null ? Error.NotFound() : job;
    }

    public async Task<ExecutionResult<Success>> UpdateJobAsync(PersistedJob persistedJob)
    {
        lock (jobLock)
        {
            var index = jobs.FindIndex(j => Equals(j.Id, persistedJob.Id));
            if (index == -1)
                return Error.NotFound();

            var list = jobs.ToList();
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

    public async Task<ExecutionResult<Success>> ArchiveJobAsync(PersistedJob job)
    {
        var remove = await RemoveJobAsync(job.Id);
        if (!remove.WasSuccess)
            return remove.Error;

        archivedJobs = archivedJobs.Add(job);

        return ExecutionResult<Success>.Success;
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

    public async Task<ExecutionResult<IEnumerable<PersistedJob>>> GetCompletedJobsAsync()
    {
        var completedJobs = archivedJobs.Where(j => j.State == JobState.Completed);

        return new ExecutionResult<IEnumerable<PersistedJob>>(completedJobs);
    }

    public Task<ExecutionResult<IEnumerable<PersistedJob>>> GetArchivedJobsAsync()
    {
        return Task.FromResult<ExecutionResult<IEnumerable<PersistedJob>>>(archivedJobs);
    }

    public async Task<ExecutionResult<IEnumerable<PersistedJob>>> GetFailedJobsAsync()
    {
        var completedJobs = archivedJobs.Where(j => j.State == JobState.Failed);

        return new ExecutionResult<IEnumerable<PersistedJob>>(completedJobs);
    }
}