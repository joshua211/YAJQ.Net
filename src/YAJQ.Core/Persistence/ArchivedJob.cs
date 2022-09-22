using YAJQ.Core.JobQueue;
using YAJQ.Core.JobQueue.Interfaces;

namespace YAJQ.Core.Persistence;

/// <summary>
/// Model class for jobs that failed or completed
/// </summary>
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

    public JobId Id { get; }
    public IJobDescriptor Descriptor { get; }
    public DateTimeOffset CreationTime { get; }
    public DateTimeOffset ScheduledTime { get; }
    public DateTimeOffset LastUpdated { get; }
    public string LastConcurrencyToken { get; }
    public JobType JobType { get; }
    public JobState State { get; }
    public DateTimeOffset ArchiveTime { get; }
    public string HandlerId { get; }
    public string ProcessorId { get; }
    public TimeSpan ExecutionTime { get; }
    public TimeSpan TotalQueueTime { get; }
}