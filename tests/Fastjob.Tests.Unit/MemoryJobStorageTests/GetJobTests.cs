using System.Threading.Tasks;
using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;
using Fastjob.Persistence.Memory;
using FluentAssertions;
using Xunit;

namespace Fastjob.Tests.Unit.MemoryJobStorageTests;

public class GetJobTests
{
    private readonly TestService testService;
    private readonly IJobStorage storage;
    
    public GetJobTests()
    {
        this.testService = new TestService();
        this.storage = new MemoryJobStorage();
    }

    [Fact]
    public async Task GetJobShouldReturnCorrectJob()
    {
        var descriptor = testService.SyncDescriptor();
        var id = await storage.AddJobAsync(descriptor);

        var job = await storage.GetJobAsync(id.Value);

        job.WasSuccess.Should().BeTrue();
        job.Value.Descriptor.Should().Be(descriptor);
    }
    
    [Fact]
    public async Task GetJobWithWrongIdShouldReturnNotFoundError()
    {
        var id = "INVALIDID";

        var job = await storage.GetJobAsync(id);

        job.WasSuccess.Should().BeFalse();
        job.Error.Should().Be(Error.NotFound());
    }
}