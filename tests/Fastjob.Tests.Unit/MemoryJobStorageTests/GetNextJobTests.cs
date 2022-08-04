using System.Reflection;
using System.Threading.Tasks;
using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;
using Fastjob.Persistence.Memory;
using FluentAssertions;
using Xunit;

namespace Fastjob.Tests.Unit.MemoryJobStorageTests;

public class GetNextJobTests 
{
    private readonly TestService testService;
    private readonly IJobStorage storage;
    
    public GetNextJobTests()
    {
        this.testService = new TestService();
        this.storage = new MemoryJobStorage();
    }

    [Fact]
    public async Task GetNextJobShouldReturnFirstAddedJob()
    {
        var jobDescriptor1 = testService.SyncDescriptor();
        var jobDescriptor2 = testService.AsyncDescriptor();
        await storage.AddJobAsync(jobDescriptor1);
        await storage.AddJobAsync(jobDescriptor2);

        var result = await storage.GetNextJobAsync();

        result.WasSuccess.Should().BeTrue();
        result.Value.Descriptor.Should().Be(jobDescriptor1);
    }
    
    [Fact]
    public async Task GetNextJobOnEmptyStorageShouldReturnNotFound()
    {
        await storage.ClearAsync();
        var result = await storage.GetNextJobAsync();

        result.WasSuccess.Should().BeFalse();
        result.Error.Should().Be(Error.NotFound());
    }
    
}