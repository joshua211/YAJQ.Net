using Fastjob.Core.Interfaces;

namespace Fastjob.Core.Persistence;

public record PersistedJob(JobId Id, IJobDescriptor Descriptor, string ConcurrencyTag,
    JobState State = JobState.Pending)
{
    public string ConcurrencyTag { get; set; } = ConcurrencyTag;
    public JobState State { get; set; }
}