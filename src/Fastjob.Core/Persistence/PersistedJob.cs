using Fastjob.Core.Interfaces;

namespace Fastjob.Core.Persistence;

public class PersistedJob
{
    public PersistedJob(JobId id, IJobDescriptor descriptor, DateTimeOffset creationTime, DateTimeOffset scheduledTime,
        DateTimeOffset lastUpdated, string concurrencyTag, JobState state, JobType jobType)
    {
        Id = id;
        Descriptor = descriptor;
        CreationTime = creationTime;
        ScheduledTime = scheduledTime;
        LastUpdated = lastUpdated;
        ConcurrencyTag = concurrencyTag;
        State = state;
        JobType = jobType;
    }

    public JobId Id { get; private set; }
    public IJobDescriptor Descriptor { get; private set; }
    public DateTimeOffset CreationTime { get; private set; }
    public DateTimeOffset ScheduledTime { get; private set; }
    public DateTimeOffset LastUpdated { get; private set; }
    public string ConcurrencyTag { get; private set; }
    public JobState State { get; private set; }
    public JobType JobType { get; private set; }

    public void Refresh() => LastUpdated = DateTimeOffset.Now;

    public void SetTag(string tag) => ConcurrencyTag = tag;

    public void Completed() => State = JobState.Completed;

    public void Failed() => State = JobState.Failed;

    public static PersistedJob Asap(JobId id, IJobDescriptor descriptor) => new PersistedJob(id, descriptor,
        DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now, string.Empty, JobState.Pending, JobType.Asap);

    public static PersistedJob Scheduled(JobId id, IJobDescriptor descriptor, DateTimeOffset scheduledTime) =>
        new PersistedJob(id, descriptor, DateTimeOffset.Now, scheduledTime, DateTimeOffset.Now, string.Empty,
            JobState.Pending, JobType.Scheduled);
}