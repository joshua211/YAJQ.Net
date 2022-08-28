using System;
using System.Threading.Tasks;
using Fastjob.Core.Interfaces;
using Fastjob.Core.JobQueue;

namespace Fastjob.Tests.Unit;

public class TestService
{
    public int SyncCalls = 0;
    public int AsyncCalls = 0;
    public int ExceptionCalls = 0;

    public static IJobDescriptor SyncDescriptor() =>
        new JobDescriptor("Something", typeof(TestService).FullName, typeof(TestService).Module.Name, Array.Empty<object>());

    public static IJobDescriptor ExceptionDescriptor() =>
        new JobDescriptor("SomethingAsync", typeof(TestService).FullName, typeof(TestService).Module.Name, Array.Empty<object>());

    public static IJobDescriptor AsyncDescriptor() =>
        new JobDescriptor("SomethingException", typeof(TestService).FullName, typeof(TestService).Module.Name, Array.Empty<object>());

    public void Something()
    {
        SyncCalls++;
    }

    public Task SomethingAsync()
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