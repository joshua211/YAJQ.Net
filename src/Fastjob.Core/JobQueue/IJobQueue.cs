using System.Linq.Expressions;
using Fastjob.Core.Common;

namespace Fastjob.Core.JobQueue;

public interface IJobQueue
{
     Task<ExecutionResult<Success>> EnqueueJob(Expression<Action> expression, string? jobId = null);
     Task<ExecutionResult<Success>> EnqueueJob<T>(Expression<Func<T>> expression, string? jobId = null);
}