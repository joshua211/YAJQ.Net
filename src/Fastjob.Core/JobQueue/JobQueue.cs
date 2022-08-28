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

    public async Task<ExecutionResult<Success>> EnqueueJob(Delegate d, params object[] args)
    {
        if (!d.Method.IsPublic || d.Method.IsAbstract)
            return Error.InvalidDelegate();
        //TODO validate descriptor here
        var jobName = d.Method.Name;
        var typeName = d.Method.ReflectedType.FullName;
        var moduleName = d.Method.Module.Name;

        var descriptor = new JobDescriptor(jobName, typeName, moduleName, args);

        await jobRepository.AddJobAsync(descriptor);

        return new Success();
    }
}