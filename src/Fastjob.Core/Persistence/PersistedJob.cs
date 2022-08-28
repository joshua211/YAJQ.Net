using Fastjob.Core.Interfaces;

namespace Fastjob.Core.Persistence;

public record PersistedJob(string Id, IJobDescriptor Descriptor, string ConcurrencyTag) 
{
    public string ConcurrencyTag { get; set; } = ConcurrencyTag;
}