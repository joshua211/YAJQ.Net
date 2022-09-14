using Fastjob.Core.JobHandler;
using Fastjob.Core.JobQueue;
using Fastjob.Core.Persistence;
using Fastjob.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Fastjob.Tests.Unit.DependencyInjection;

public class DITests
{
    [Fact]
    public void AllRequiredServicesAreRegistered()
    {
        //Arrange
        var provider = new ServiceCollection().AddLogging().AddFastjob().BuildServiceProvider();

        //Act
        var queue = provider.GetService<IJobQueue>();
        var handler = provider.GetService<IJobHandler>();
        var repo = provider.GetService<IJobRepository>();
        var persistence = provider.GetService<IJobPersistence>();

        //Assert
        queue.Should().NotBeNull();
        handler.Should().NotBeNull();
        repo.Should().NotBeNull();
        persistence.Should().NotBeNull();
    }
}