using Fastjob.Core.Interfaces;

namespace Fastjob.Core.Persistence;

public class ArchivedJob
{
    public ArchivedJob(JobId id, IJobDescriptor descriptor, DateTimeOffset creationTime, DateTimeOffset scheduledTime,
        DateTimeOffset lastUpdated, string lastConcurrencyToken, JobType jobType, DateTimeOffset archiveTime,
        string handlerId, string processorId, TimeSpan executionTime, TimeSpan totalQueueTime, JobState state)
    {
        Id = id;
        Descriptor = descriptor;
        CreationTime = creationTime;
        ScheduledTime = scheduledTime;
        LastUpdated = lastUpdated;
        LastConcurrencyToken = lastConcurrencyToken;
        JobType = jobType;
        ArchiveTime = archiveTime;
        HandlerId = handlerId;
        ProcessorId = processorId;
        ExecutionTime = executionTime;
        TotalQueueTime = totalQueueTime;
        State = state;
    }

    public JobId Id { get; private set; }
    public IJobDescriptor Descriptor { get; private set; }
    public DateTimeOffset CreationTime { get; private set; }
    public DateTimeOffset ScheduledTime { get; private set; }
    public DateTimeOffset LastUpdated { get; private set; }
    public string LastConcurrencyToken { get; private set; }
    public JobType JobType { get; private set; }
    public JobState State { get; private set; }
    public DateTimeOffset ArchiveTime { get; private set; }
    public string HandlerId { get; private set; }
    public string ProcessorId { get; private set; }
    public TimeSpan ExecutionTime { get; private set; }
    public TimeSpan TotalQueueTime { get; private set; }
}