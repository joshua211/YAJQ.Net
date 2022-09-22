namespace YAJQ.Core.Interfaces;

public interface IJobDescriptor
{
    string JobName { get; }
    string FullTypeName { get; }
    string ModuleName { get; }
    IEnumerable<object> Args { get; }
}