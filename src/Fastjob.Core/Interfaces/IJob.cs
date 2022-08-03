namespace Fastjob.Core.Interfaces;

public interface IJob<T>
{
    T Execute();
}