using Fastjob.Core.Interfaces;
using Fastjob.Core.JobQueue;

namespace Fastjob.Tests.Shared;

public interface IAsyncService
{
    Task DoAsync(string id);
}

public class AsyncService : IAsyncService
{
    public static IJobDescriptor Descriptor(string id = "XXX") => new JobDescriptor(nameof(DoAsync),
        typeof(AsyncService).FullName, typeof(AsyncService).Module.Name, new object[] {id});

    public async Task DoAsync(string id)
    {
        await Task.Delay(10);
        CallReceiver.AddCall(id);
    }
}