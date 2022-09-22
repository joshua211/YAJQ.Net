using System;
using System.Threading.Tasks;
using YAJQ.Core.JobQueue;
using YAJQ.Core.JobQueue.Interfaces;

namespace YAJQ.Tests.Unit;

public class TestService
{
    public int AsyncCalls;
    public int ExceptionCalls;
    public int SyncCalls;

    public static IJobDescriptor SyncDescriptor()
    {
        return new JobDescriptor("Something", typeof(TestService).FullName, typeof(TestService).Module.Name,
            Array.Empty<object>());
    }

    public static IJobDescriptor SyncWithValueDescriptor()
    {
        return new JobDescriptor("SomethingWithValue", typeof(TestService).FullName, typeof(TestService).Module.Name,
            new object[] {1});
    }

    public static IJobDescriptor ExceptionDescriptor()
    {
        return new JobDescriptor("SomethingException", typeof(TestService).FullName, typeof(TestService).Module.Name,
            Array.Empty<object>());
    }

    public static IJobDescriptor AsyncDescriptor()
    {
        return new JobDescriptor("SomethingAsync", typeof(TestService).FullName, typeof(TestService).Module.Name,
            Array.Empty<object>());
    }

    public static IJobDescriptor AsyncWithValueDescriptor()
    {
        return new JobDescriptor("SomethingWithValueAsync", typeof(TestService).FullName,
            typeof(TestService).Module.Name, new object[] {1});
    }

    public void Something()
    {
        SyncCalls++;
    }

    public void SomethingWithValue(int i)
    {
        SyncCalls++;
    }

    public Task SomethingAsync()
    {
        AsyncCalls++;

        return Task.CompletedTask;
    }

    public Task SomethingWithValueAsync(int i)
    {
        AsyncCalls++;

        return Task.CompletedTask;
    }

    public Task SomethingException()
    {
        ExceptionCalls++;
        throw new Exception();
    }
}