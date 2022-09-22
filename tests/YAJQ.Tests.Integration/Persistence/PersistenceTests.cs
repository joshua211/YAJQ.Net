using System.Linq;
using System.Threading.Tasks;
using YAJQ.Core.Common;
using YAJQ.Core.Persistence;
using YAJQ.Tests.Shared;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace YAJQ.Tests.Integration.Persistence;

public abstract class PersistenceTests : IntegrationTest
{
    public PersistenceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task CanSaveJob()
    {
        //Arrange
        var wasSaved = false;
        Persistence.NewJob += (s, e) => wasSaved = true;

        //Act
        await Repository.AddJobAsync(AsyncService.Descriptor());

        //Assert
        wasSaved.Should().BeTrue();
    }

    [Fact]
    public async Task CanGetSavedJob()
    {
        //Arrange
        var jobId = JobId.New;
        await Repository.AddJobAsync(AsyncService.Descriptor(), jobId);

        //Act
        var job = await Repository.GetJobAsync(jobId);

        //Assert
        job.WasSuccess.Should().BeTrue();
        job.Value.Id.Should().Be(jobId);
    }

    [Fact]
    public async Task CanMarkJob()
    {
        //Arrange
        var jobId = JobId.New;
        await Repository.AddJobAsync(AsyncService.Descriptor(), jobId);
        var get = await Repository.GetJobAsync(jobId);

        //Act
        var result = await Repository.TryGetAndMarkJobAsync(get.Value, "XXX");

        //Assert
        result.WasSuccess.Should().BeTrue();
        var job = (await Repository.GetJobAsync(jobId)).Value;
        job.ConcurrencyToken.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CanRemoveJob()
    {
        //Arrange
        var jobId = JobId.New;
        await Repository.AddJobAsync(AsyncService.Descriptor(), jobId);

        //Act
        var result = await Persistence.RemoveJobAsync(jobId);

        //Assert
        result.WasSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CanRemoveAllJobs()
    {
        //Arrange
        var jobId1 = JobId.New;
        var jobId2 = JobId.New;
        await Repository.AddJobAsync(AsyncService.Descriptor(), jobId1);
        await Repository.AddJobAsync(AsyncService.Descriptor(), jobId2);

        //Act
        var result = await Persistence.RemoveAllJobsAsync();

        //Assert
        result.WasSuccess.Should().BeTrue();
        (await Repository.GetJobAsync(jobId1)).Error.Should().Be(Error.NotFound());
        (await Repository.GetJobAsync(jobId2)).Error.Should().Be(Error.NotFound());
    }

    [Fact]
    public async Task CanGetJobAtCursor()
    {
        //Arrange
        var ids = await PublishJobs(5);
        await Persistence.IncreaseCursorAsync(); //cursor at 1
        await Persistence.IncreaseCursorAsync(); //cursor at 2

        //Act
        var result = await Persistence.GetJobAtCursorAsync();

        //Assert
        result.WasSuccess.Should().BeTrue();
        result.Value.Id.Value.Should().Be(ids.ToList()[2]);
    }

    [Fact]
    public async Task CanIncreaseCursor()
    {
        //Arrange
        await PublishJobs(5);

        //Act
        var result = await Persistence.IncreaseCursorAsync();

        //Assert
        result.WasSuccess.Should().BeTrue();
        result.Value.CurrentCursor.Should().Be(2);
    }
}