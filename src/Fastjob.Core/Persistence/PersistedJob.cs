using Fastjob.Core.Interfaces;

namespace Fastjob.Core.Persistence;

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

    public JobId Id { get; private set; }
    public IJobDescriptor Descriptor { get; private set; }
    public DateTimeOffset CreationTime { get; private set; }
    public DateTimeOffset ScheduledTime { get; private set; }
    public DateTimeOffset LastUpdated { get; private set; }
    public string ConcurrencyToken { get; private set; }
    public JobState State { get; private set; }
    public JobType JobType { get; private set; }

    public void Refresh() => LastUpdated = DateTimeOffset.Now;

    public void SetTag(string tag) => ConcurrencyToken = tag;

    public void Completed() => State = JobState.Completed;

    public void Failed() => State = JobState.Failed;

    public ArchivedJob Archive(string handlerId, string processorId, TimeSpan executionTime)
    {
        return new ArchivedJob(Id, Descriptor, CreationTime, ScheduledTime, LastUpdated, ConcurrencyToken, JobType,
            DateTimeOffset.Now, "", "", TimeSpan.Zero, DateTimeOffset.Now - CreationTime);
    }

    public static PersistedJob Asap(JobId id, IJobDescriptor descriptor) => new PersistedJob(id, descriptor,
        DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now, string.Empty, JobState.Pending, JobType.Asap);

    public static PersistedJob Scheduled(JobId id, IJobDescriptor descriptor, DateTimeOffset scheduledTime) =>
        new PersistedJob(id, descriptor, DateTimeOffset.Now, scheduledTime, DateTimeOffset.Now, string.Empty,
            JobState.Pending, JobType.Scheduled);
}