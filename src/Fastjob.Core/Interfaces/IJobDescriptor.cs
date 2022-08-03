namespace Fastjob.Core.Interfaces;

public interface IJobDescriptor
{
    string JobName { get; }
    string FullTypeName { get; }
    string ModuleName { get; }
    bool IsAsync { get; }
    IEnumerable<object> Args { get; }
}