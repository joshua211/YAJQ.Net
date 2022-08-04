using System;
using System.Threading.Tasks;
using Fastjob.Core.Interfaces;
using Fastjob.Core.JobQueue;

namespace Fastjob.Tests.Unit;

public class TestService
{
    public static int SyncCalls = 0;
    public static int AsyncCalls = 0;
    public static int ExceptionCalls = 0;
    
    public IJobDescriptor SyncDescriptor() =>
        new JobDescriptor("Something", GetType().FullName, GetType().Module.Name, new object[] {1});
    
    public IJobDescriptor ExceptionDescriptor() =>
        new JobDescriptor("SomethingException", GetType().FullName, GetType().Module.Name, new object[] {1});
    
    public IJobDescriptor AsyncDescriptor() =>
        new JobDescriptor("SomethingAsync", GetType().FullName, GetType().Module.Name, new object[] {1});

    public void Something(int i)
    {
        SyncCalls++;
    }

    public Task SomethingAsync(int i)
    {
        AsyncCalls++;

        return Task.CompletedTask;
    }

    public Task SomethingException(int i)
    {
        ExceptionCalls++;
        throw new Exception();
    }
}