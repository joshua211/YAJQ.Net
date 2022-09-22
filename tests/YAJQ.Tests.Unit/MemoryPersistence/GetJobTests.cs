using System.Threading.Tasks;
using YAJQ.Core.Archive;
using YAJQ.Core.Common;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace YAJQ.Tests.Unit.MemoryPersistence;

public class GetJobTests : TestBase
{
    [Fact]
    public async Task ReturnsJob()
    {
        //Arrange
        var job = PersistedSyncJob();
        var arch = Substitute.For<IJobArchive>();
        var pers = new Persistence.Memory.MemoryPersistence(arch);
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
        var arch = Substitute.For<IJobArchive>();
        var pers = new Persistence.Memory.MemoryPersistence(arch);

        //Act
        var persistedJob = await pers.GetJobAsync(DefaultJobId);

        //Arrange
        persistedJob.WasSuccess.Should().BeFalse();
        persistedJob.Error.Should().Be(Error.NotFound());
    }
}