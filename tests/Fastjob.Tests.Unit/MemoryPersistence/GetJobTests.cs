using System.Threading.Tasks;
using Fastjob.Core.Common;
using FluentAssertions;
using Xunit;

namespace Fastjob.Tests.Unit.MemoryPersistence;

public class GetJobTests : TestBase
{
    [Fact]
    public async Task ReturnsJob()
    {
        //Arrange
        var job = PersistedSyncJob();
        var pers = new Persistence.Memory.MemoryPersistence();
        await pers.SaveJobAsync(job);

        //Act
        var persistedJob = await pers.GetJobAsync(job.Id);
        
        //Arrange
        persistedJob.WasSuccess.Should().BeTrue();
        persistedJob.Value.Id.Should().Be(job.Id);
    }

    [Fact]
    public async Task ReturnsNotFoundError()
    {
        //Arrange
        var pers = new Persistence.Memory.MemoryPersistence();

        //Act
        var persistedJob = await pers.GetJobAsync(JobId);
        
        //Arrange
        persistedJob.WasSuccess.Should().BeFalse();
        persistedJob.Error.Should().Be(Error.NotFound());
    }
    
}