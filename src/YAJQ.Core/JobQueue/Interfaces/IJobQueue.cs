using System.Linq.Expressions;
using YAJQ.Core.Common;
using YAJQ.Core.JobHandler.Interfaces;

namespace YAJQ.Core.JobQueue.Interfaces;

public interface IJobQueue
{
    /// <summary>
    /// Enqueues a job that is run instantly on the next available <see cref="IJobHandler"></see>
    /// </summary>
    /// <param name="expression">The action to be executed. Must point to a class method that is available to all JobHandlers:
    /// <code>() => myService.DoSomething(123)</code>
    /// </param>
    /// <param name="jobId">Optional id to identify this job</param>
    /// <returns>An <see cref="ExecutionResult{Success}">Execution Result</see> to indicate success or failure</returns>
    Task<ExecutionResult<Success>> EnqueueJobAsync(Expression<Action> expression, string? jobId = null);

    /// <summary>
    /// Enqueues a job that is run instantly on the next available <see cref="IJobHandler"></see>
    /// </summary>
    /// <param name="expression">The function to be executed. Must point to a class method that is available to all JobHandlers:
    /// <code>() => myService.DoSomethingAsync(123)</code>
    /// </param>
    /// <param name="jobId">Optional id to identify this job</param>
    /// <returns>An <see cref="ExecutionResult{Success}">Execution Result</see> to indicate success or failure</returns>
    Task<ExecutionResult<Success>> EnqueueJobAsync<T>(Expression<Func<T>> expression, string? jobId = null);

    /// <summary>
    /// Schedules a job that is run at the scheduled time on the next available <see cref="IJobHandler"></see>
    /// </summary>
    /// <param name="expression">The action to be executed. Must point to a class method that is available to all JobHandlers:
    /// <code>() => myService.DoSomething(123)</code>
    /// </param>
    /// <param name="scheduleTime">The <see cref="DateTimeOffset"/> at which the job should execute</param>
    /// <param name="jobId">Optional id to identify this job</param>
    /// <returns>An <see cref="ExecutionResult{Success}">Execution Result</see> to indicate success or failure</returns>
    Task<ExecutionResult<Success>> ScheduleJobAsync(Expression<Action> expression, DateTimeOffset scheduleTime,
        string? jobId = null);

    /// <summary>
    /// Schedules a job that is run at the scheduled time on the next available <see cref="IJobHandler"></see>
    /// </summary>
    /// <param name="expression">The function to be executed. Must point to a class method that is available to all JobHandlers:
    /// <code>() => myService.DoSomethingAsync(123)</code>
    /// </param>
    /// <param name="scheduleTime">The <see cref="DateTimeOffset"/> at which the job should execute</param>
    /// <param name="jobId">Optional id to identify this job</param>
    /// <returns>An <see cref="ExecutionResult{Success}">Execution Result</see> to indicate success or failure</returns>
    Task<ExecutionResult<Success>> ScheduleJobAsync<T>(Expression<Func<T>> expression, DateTimeOffset scheduleTime,
        string? jobId = null);
}