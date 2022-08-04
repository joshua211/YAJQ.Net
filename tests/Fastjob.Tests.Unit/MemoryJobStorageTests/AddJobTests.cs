using System.Threading.Tasks;
using Fastjob.Core.Interfaces;
using Fastjob.Persistence.Memory;
using FluentAssertions;
using Xunit;

namespace Fastjob.Tests.Unit.MemoryJobStorageTests;

public class AddJobTests
{
    private readonly TestService testService;
    private readonly IJobStorage storage;
    
    public AddJobTests()
    {
        this.testService = new TestService();
        this.storage = new MemoryJobStorage();
    }

    [Fact]
    public async Task AddDescriptorShouldReturnSuccess()
    {
        var descriptor = testService.SyncDescriptor();

        var result = await storage.AddJobAsync(descriptor);

        result.WasSuccess.Should().BeTrue();
    }
    
    
    
}