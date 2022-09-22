using System.Linq.Expressions;
using YAJQ.Core.Common;

namespace YAJQ.Core.JobQueue;

public interface IJobQueue
{
    Task<ExecutionResult<Success>> EnqueueJobAsync(Expression<Action> expression, string? jobId = null);
    Task<ExecutionResult<Success>> EnqueueJobAsync<T>(Expression<Func<T>> expression, string? jobId = null);

    Task<ExecutionResult<Success>> ScheduleJobAsync(Expression<Action> expression, DateTimeOffset scheduleTime,
        string? jobId = null);

    Task<ExecutionResult<Success>> ScheduleJobAsync<T>(Expression<Func<T>> expression, DateTimeOffset scheduleTime,
        string? jobId = null);
}