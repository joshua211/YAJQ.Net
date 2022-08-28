using Fastjob.Core.Common;

namespace Fastjob.Core.JobQueue;

public interface IJobQueue
{
     Task<ExecutionResult<Success>> EnqueueJob(Delegate d, params object[] args);
}