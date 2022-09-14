using System.Linq.Expressions;
using System.Reflection;
using Fastjob.Core.Common;
using Fastjob.Core.Persistence;

namespace Fastjob.Core.JobQueue;

public class JobQueue : IJobQueue
{
    private readonly IJobRepository jobRepository;

    public JobQueue(IJobRepository jobRepository)
    {
        this.jobRepository = jobRepository;
    }

    public async Task<ExecutionResult<Success>> EnqueueJobAsync(Expression<Action> expression, string? jobId = null)
    {
        var descriptor = ToJobDescriptor(expression);
        var result = await jobRepository.AddJobAsync(descriptor, jobId);

        return result.WasSuccess ? new Success() : result.Error;
    }

    public async Task<ExecutionResult<Success>> EnqueueJobAsync<T>(Expression<Func<T>> expression, string? jobId = null)
    {
        var descriptor = ToJobDescriptor(expression);
        var result = await jobRepository.AddJobAsync(descriptor, jobId);

        return result.WasSuccess ? new Success() : result.Error;
    }

    public async Task<ExecutionResult<Success>> ScheduleJobAsync(Expression<Action> expression,
        DateTimeOffset scheduleTime, string? jobId = null)
    {
        var descriptor = ToJobDescriptor(expression);
        var result = await jobRepository.AddJobAsync(descriptor, jobId, scheduleTime);

        return result.WasSuccess ? new Success() : result.Error;
    }

    public async Task<ExecutionResult<Success>> ScheduleJobAsync<T>(Expression<Func<T>> expression,
        DateTimeOffset scheduleTime, string? jobId = null)
    {
        var descriptor = ToJobDescriptor(expression);
        var result = await jobRepository.AddJobAsync(descriptor, jobId, scheduleTime);

        return result.WasSuccess ? new Success() : result.Error;
    }

    private static JobDescriptor ToJobDescriptor<T>(Expression<Func<T>> expression)
    {
        var visitor = new FieldToConstantArgumentVisitor();
        var resolvedExpression = (Expression<Func<T>>) visitor.Visit(expression);

        var methodExpression = (MethodCallExpression) resolvedExpression.Body;
        var method = methodExpression.Method;
        var args = methodExpression.Arguments.Select(arg =>
        {
            var value = ((ConstantExpression) arg).Value!;
            return value;
        });

        return ToJobDescriptor(method, args);
    }

    private static JobDescriptor ToJobDescriptor(Expression<Action> expression)
    {
        var visitor = new FieldToConstantArgumentVisitor();
        var resolvedExpression = (Expression<Action>) visitor.Visit(expression);

        var methodExpression = (MethodCallExpression) resolvedExpression.Body;
        var method = methodExpression.Method;
        var args = methodExpression.Arguments.Select(arg =>
        {
            var value = ((ConstantExpression) arg).Value!;
            return value;
        });

        return ToJobDescriptor(method, args);
    }

    private static JobDescriptor ToJobDescriptor(MethodInfo method, IEnumerable<object> args)
    {
        var methodParams = method.GetParameters();
        var argTypes = new Type[methodParams.Length];

        for (var i = 0; i < methodParams.Length; ++i)
        {
            argTypes[i] = methodParams[i].ParameterType;
        }

        var jobName = method.Name;
        var typeName = method.DeclaringType!.FullName;
        var moduleName = method.DeclaringType.Module.Name;

        return new JobDescriptor(jobName, typeName, moduleName, args);
    }
}