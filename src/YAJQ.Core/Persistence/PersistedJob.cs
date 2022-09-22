using YAJQ.Core.JobQueue;
using YAJQ.Core.JobQueue.Interfaces;

namespace YAJQ.Core.Persistence;

public class PersistedJob
{
    public PersistedJob(JobId id, IJobDescriptor descriptor, DateTimeOffset creationTime, DateTimeOffset scheduledTime,
        DateTimeOffset lastUpdated, string concurrencyToken, JobState state, JobType jobType)
    {
        Id = id;
        Descriptor = descriptor;
        CreationTime = creationTime;
        ScheduledTime = scheduledTime;
        LastUpdated = lastUpdated;
        ConcurrencyToken = concurrencyToken;
        State = state;
        JobType = jobType;
    }

    public JobId Id { get; }
    public IJobDescriptor Descriptor { get; }
    public DateTimeOffset CreationTime { get; }
    public DateTimeOffset ScheduledTime { get; }
    public DateTimeOffset LastUpdated { get; private set; }
    public string ConcurrencyToken { get; private set; }
    public JobState State { get; private set; }
    public JobType JobType { get; }

    public void Refresh()
    {
        LastUpdated = DateTimeOffset.Now;
    }

    public void SetToken(string tag)
    {
        ConcurrencyToken = tag;
    }

    public void Complete()
    {
        if (State != JobState.Pending)
            throw new Exception("Only a pending job can be completed");

        State = JobState.Completed;
    }

    public void Fail()
    {
        if (State != JobState.Pending)
            throw new Exception("Only a pending job can be failed");

        State = JobState.Failed;
    }

    public ArchivedJob Archive(string handlerId, string processorId, TimeSpan executionTime)
    {
        return new ArchivedJob(Id, Descriptor, CreationTime, ScheduledTime, LastUpdated, ConcurrencyToken, JobType,
            DateTimeOffset.Now, handlerId, processorId, executionTime, DateTimeOffset.Now - CreationTime, State);
    }

    public static PersistedJob Asap(JobId id, IJobDescriptor descriptor)
    {
        return new(id, descriptor,
            DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now, string.Empty, JobState.Pending,
            JobType.Instant);
    }

    public static PersistedJob Scheduled(JobId id, IJobDescriptor descriptor, DateTimeOffset scheduledTime)
    {
        return new(id, descriptor, DateTimeOffset.Now, scheduledTime, DateTimeOffset.Now, string.Empty,
            JobState.Pending, JobType.Scheduled);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PersistedJob job)
            return false;

        return job.Id == Id && job.State == State && job.ConcurrencyToken == ConcurrencyToken &&
               job.LastUpdated == LastUpdated && job.CreationTime == CreationTime && job.ScheduledTime == ScheduledTime;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, CreationTime, ScheduledTime, LastUpdated, ConcurrencyToken, (int) State,
            (int) JobType);
    }

    public PersistedJob DeepCopy()
    {
        return new(Id, Descriptor, CreationTime, ScheduledTime, LastUpdated, ConcurrencyToken, State,
            JobType);
    }
}