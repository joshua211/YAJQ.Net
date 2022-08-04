using System.Threading.Tasks;
using Fastjob.Core.Interfaces;
using Fastjob.Core.JobQueue;
using NSubstitute;
using Xunit;

namespace Fastjob.Tests.Unit.JobQueueTests;

public class EnqueueJobTests
{
    private readonly IJobStorage storage;
    private readonly TestService testService;

    public EnqueueJobTests()
    {
        storage = Substitute.For<IJobStorage>();
        testService = new TestService();
    }

    [Fact]
    public async Task EnqueueSyncJobSavesDescriptorToStorage()
    {
        var param = 1;
        var queue = new JobQueue(storage);
        var job = testService.Something;
        var descriptor = testService.SyncDescriptor();

        await queue.EnqueueJob(job, param);

        await storage.Received().AddJobAsync(descriptor);
    }
}