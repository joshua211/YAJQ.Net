using YAJQ.Core.Interfaces;

namespace YAJQ.Core.JobQueue;

public class JobDescriptor : IJobDescriptor
{
    public string JobName { get; }
    public string FullTypeName { get; }
    public string ModuleName { get; }
    public IEnumerable<object> Args { get; }

    public JobDescriptor(string jobName, string fullTypeName, string moduleName, IEnumerable<object> args)
    {
        JobName = jobName;
        FullTypeName = fullTypeName;
        ModuleName = moduleName;
        Args = args;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not JobDescriptor desc)
            return false;

        return JobName == desc.JobName && FullTypeName == desc.FullTypeName && ModuleName == desc.ModuleName;
    }
}