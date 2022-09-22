namespace YAJQ.Core.Interfaces;

public interface IJob<T>
{
    T Execute();
}