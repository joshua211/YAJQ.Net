namespace Fastjob.Core.Interfaces;

public interface IJobQueue
{
     Task EnqueueJob(Delegate d, params object[] args);
}