using Fastjob.Core.Interfaces;
using Fastjob.Core.JobQueue;

namespace Fastjob.Tests.Shared;

public interface IAsyncService
{
    Task DoAsync(string id);
    Task DoExceptionAsync();
}

public class AsyncService : IAsyncService
{
    public async Task DoAsync(string id)
    {
        await Task.Delay(10);
        CallReceiver.AddCall(id);
    }

    public async Task DoExceptionAsync()
    {
        await Task.Delay(10);
        throw new Exception("Test");
    }

    public static IJobDescriptor Descriptor(string id = "XXX") => new JobDescriptor(nameof(DoAsync),
        typeof(AsyncService).FullName, typeof(AsyncService).Module.Name, new object[] {id});

    public static IJobDescriptor ExceptionDescriptor() => new JobDescriptor(nameof(DoExceptionAsync),
        typeof(AsyncService).FullName, typeof(AsyncService).Module.Name, Array.Empty<object>());
}