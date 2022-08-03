using Fastjob.Core.Interfaces;

namespace Fastjob.Core.JobQueue;

public class JobQueue : IJobQueue
{
    private readonly IJobStorage jobStorage;

    public JobQueue(IJobStorage jobStorage)
    {
        this.jobStorage = jobStorage;
    }

    public async Task EnqueueJob(Delegate d, params object[] args)
    {
        var jobName = d.Method.Name;
        var typeName = d.Method.ReflectedType.FullName;
        var moduleName = d.Method.Module.Name;

        var descriptor = new JobDescriptor(jobName, typeName, moduleName, false, args);

        await jobStorage.AddJobAsync(descriptor);
    }
}