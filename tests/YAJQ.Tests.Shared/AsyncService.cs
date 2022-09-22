﻿using YAJQ.Core.Interfaces;
using YAJQ.Core.JobQueue;

namespace YAJQ.Tests.Shared;

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

    public async Task DoLongRunningAsync(string id)
    {
        await Task.Delay(1000);
        CallReceiver.AddCall(id);
    }

    public static IJobDescriptor Descriptor(string id = "XXX") => new JobDescriptor(nameof(DoAsync),
        typeof(AsyncService).FullName, typeof(AsyncService).Module.Name, new object[] {id});

    public static IJobDescriptor LongRunningDescriptor(string id = "XXX") => new JobDescriptor(
        nameof(DoLongRunningAsync),
        typeof(AsyncService).FullName, typeof(AsyncService).Module.Name, new object[] {id});

    public static IJobDescriptor ExceptionDescriptor() => new JobDescriptor(nameof(DoExceptionAsync),
        typeof(AsyncService).FullName, typeof(AsyncService).Module.Name, Array.Empty<object>());
}