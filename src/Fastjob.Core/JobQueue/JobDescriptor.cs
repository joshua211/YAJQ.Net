using Fastjob.Core.Interfaces;

namespace Fastjob.Core.JobQueue;

public record JobDescriptor
    (string JobName, string FullTypeName, string ModuleName, bool IsAsync, IEnumerable<object> Args) : IJobDescriptor
{
}