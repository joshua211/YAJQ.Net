using System.Threading.Tasks;
using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;
using Fastjob.Persistence.Memory;
using FluentAssertions;
using Xunit;

namespace Fastjob.Tests.Unit.MemoryJobStorageTests;

public class MarkJobTests
{
    private readonly TestService testService;
    private readonly IJobStorage storage;

    public MarkJobTests()
    {
        this.testService = new TestService();
        this.storage = new MemoryJobStorage();
    }

    [Fact]
    public async Task MarkJobShouldMarkJobInStorage()
    {
        var mark = "XXX";
        var descriptor = testService.SyncDescriptor();
        var id = (await storage.AddJobAsync(descriptor)).Value;

        var markResult = await storage.TryMarkAndGetJobAsync(id, mark);
        var result = await storage.GetJobAsync(id);

        markResult.WasSuccess.Should().BeTrue();
        result.WasSuccess.Should().BeTrue();
        result.Value.ConcurrencyTag.Should().Be(mark);
    }

    [Fact]
    public async Task MarkingJobAgainShouldReturnError()
    {
        var mark = "XXX";
        var mark2 = "XXXX";
        var descriptor = testService.SyncDescriptor();
        var id = (await storage.AddJobAsync(descriptor)).Value;

        var firstMark = await storage.TryMarkAndGetJobAsync(id, mark);
        var secondMark = await storage.TryMarkAndGetJobAsync(id, mark2);

        firstMark.WasSuccess.Should().BeTrue();
        secondMark.WasSuccess.Should().BeFalse();
        secondMark.Error.Should().Be(Error.AlreadyMarked());
    }

    [Fact]
    public async Task MarkingInvalidJobShouldReturnNotFound()
    {
        var id = "INVALID";
        var mark = "XXX";

        var result = await storage.TryMarkAndGetJobAsync(id, mark);

        result.WasSuccess.Should().BeFalse();
        result.Error.Should().Be(Error.NotFound());
    }
}