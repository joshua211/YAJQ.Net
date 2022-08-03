namespace Fastjob.Core.Interfaces;

public interface IPersistedJob
{
    string Id { get; }
    IJobDescriptor Descriptor { get; }
    string ConcurrencyTag { get; set; }
}