using Fastjob.Core.Common;

namespace Fastjob.Core.Interfaces;

public interface IJobQueue
{
     Task<ExecutionResult<Success>> EnqueueJob(Delegate d, params object[] args);
}