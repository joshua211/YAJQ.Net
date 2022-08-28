using System.Threading.Tasks;
using Fastjob.Core.Persistence;
using FluentAssertions;
using Xunit;

namespace Fastjob.Tests.Unit.MemoryPersistence;

public class GetJobAtCursorTests : TestBase
{
    [Fact]
    public async Task ReturnsJobAtCursor()
    {
        //Arrange
        var job = PersistedSyncJob();
        var pers = new Persistence.Memory.MemoryPersistence();
        await pers.SaveJobAsync(job);

        //Act
        var result = await pers.GetJobAtCursorAsync();

        //Arrange
        result.WasSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(JobId);
    }

    [Fact]
    public async Task ReturnsBothJobsAfterIncrease()
    {
        //Arrange
        var job1 = PersistedSyncJob();
        var job2 = PersistedSyncJob("ASDF");
        var pers = new Persistence.Memory.MemoryPersistence();
        await pers.SaveJobAsync(job1);
        await pers.SaveJobAsync(job2);

        //Act
        var result1 = await pers.GetJobAtCursorAsync();
        var result2 = await pers.GetJobAtCursorAsync();
        await pers.IncreaseCursorAsync();
        var result3 = await pers.GetJobAtCursorAsync();

        //Arrange
        result1.WasSuccess.Should().BeTrue();
        result1.Value.Id.Should().Be(JobId);
        result2.WasSuccess.Should().BeTrue();
        result2.Value.Id.Should().Be(JobId);
        
        result3.WasSuccess.Should().BeTrue();
        result3.Value.Id.Should().Be("ASDF");
    }
}