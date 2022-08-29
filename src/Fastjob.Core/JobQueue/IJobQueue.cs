using System.Linq.Expressions;
using Fastjob.Core.Common;

namespace Fastjob.Core.JobQueue;

public interface IJobQueue
{
     Task<ExecutionResult<Success>> EnqueueJob(Expression<Action> expression);
     Task<ExecutionResult<Success>> EnqueueJob<T>(Expression<Func<T>> expression);
}