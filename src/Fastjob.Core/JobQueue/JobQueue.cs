using System.Linq.Expressions;
using System.Reflection;
using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;
using Fastjob.Core.Persistence;

namespace Fastjob.Core.JobQueue;

public class JobQueue : IJobQueue
{
    private readonly IJobRepository jobRepository;

    public JobQueue(IJobRepository jobRepository)
    {
        this.jobRepository = jobRepository;
    }
    
    public async Task<ExecutionResult<Success>> EnqueueJob(Expression<Action> expression, string? jobId = null)
    {
        var visitor = new FieldToConstantArgumentVisitor();
        var resolvedExpression = (Expression<Action>) visitor.Visit(expression);

        var methodExpression = (MethodCallExpression) resolvedExpression.Body;
        var method = methodExpression.Method;
        var args = methodExpression.Arguments.Select(arg =>
        {
            var value = ((ConstantExpression) arg).Value!;
            return value;
        }).ToList();

        var descriptor = ToJobDescriptor(method, args);
        
        await jobRepository.AddJobAsync(descriptor, jobId);

        return new Success();
    }

    public async Task<ExecutionResult<Success>> EnqueueJob<T>(Expression<Func<T>> expression, string? jobId = null)
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

        var descriptor = ToJobDescriptor(method, args);
        
        await jobRepository.AddJobAsync(descriptor, jobId);

        return new Success();
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