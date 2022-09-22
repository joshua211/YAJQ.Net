using System;
using System.Threading.Tasks;
using YAJQ.Core.Interfaces;
using YAJQ.Core.JobQueue;

namespace YAJQ.Tests.Unit;

public class TestService
{
    public int SyncCalls = 0;
    public int AsyncCalls = 0;
    public int ExceptionCalls = 0;

    public static IJobDescriptor SyncDescriptor() =>
        new JobDescriptor("Something", typeof(TestService).FullName, typeof(TestService).Module.Name,
            Array.Empty<object>());

    public static IJobDescriptor SyncWithValueDescriptor() =>
        new JobDescriptor("SomethingWithValue", typeof(TestService).FullName, typeof(TestService).Module.Name, new object[] {1});

    public static IJobDescriptor ExceptionDescriptor() =>
        new JobDescriptor("SomethingException", typeof(TestService).FullName, typeof(TestService).Module.Name,
            Array.Empty<object>());
    
    public static IJobDescriptor AsyncDescriptor() =>
        new JobDescriptor("SomethingAsync", typeof(TestService).FullName, typeof(TestService).Module.Name,
            Array.Empty<object>());
    
    public static IJobDescriptor AsyncWithValueDescriptor() =>
        new JobDescriptor("SomethingWithValueAsync", typeof(TestService).FullName, typeof(TestService).Module.Name, new object[] {1});

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