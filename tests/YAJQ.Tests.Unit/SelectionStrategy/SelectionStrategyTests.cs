using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;
using YAJQ.Core.JobHandler;
using YAJQ.Core.JobHandler.Interfaces;
using YAJQ.Core.JobProcessor;
using YAJQ.Core.JobProcessor.Interfaces;

namespace YAJQ.Tests.Unit.SelectionStrategy;

public abstract class SelectionStrategyTests
{
    public abstract IProcessorSelectionStrategy GetStrategy();

    [Fact]
    public async Task CanAddProcessor()
    {
        //Arrange
        var strategy = GetStrategy();
        var processor = Substitute.For<IJobProcessor>();

        //Act
        strategy.AddProcessor(processor);

        //Assert
        var next = await strategy.GetNextProcessorAsync();
        next.Should().Be(processor);
    }

    [Fact]
    public async Task CanRemoveProcessor()
    {
        //Arrange
        var strategy = GetStrategy();
        var processor = Substitute.For<IJobProcessor>();
        strategy.AddProcessor(processor);

        //Act
        strategy.RemoveProcessor();

        //Assert
        var next = await strategy.GetNextProcessorAsync();
        next.Should().BeNull();
    }

    [Fact]
    public async Task CanGetNextProcessor()
    {
        //Arrange
        var strategy = GetStrategy();
        var processor = Substitute.For<IJobProcessor>();
        strategy.AddProcessor(processor);

        //Act
        var next = await strategy.GetNextProcessorAsync();

        //Assert
        next.Should().Be(processor);
    }

    [Fact]
    public async Task CanGetNextFromMultipleProcessors()
    {
        //Arrange
        var strategy = GetStrategy();
        var processor1 = Substitute.For<IJobProcessor>();
        var processor2 = Substitute.For<IJobProcessor>();
        strategy.AddProcessor(processor1);
        strategy.AddProcessor(processor2);

        //Act
        var next1 = await strategy.GetNextProcessorAsync();
        var next2 = await strategy.GetNextProcessorAsync();

        //Assert
        next1.Should().NotBeNull();
        next2.Should().NotBeNull();
    }

    [Fact]
    public async Task CanGetNextWithBusyProcessor()
    {
        //Arrange
        var strategy = GetStrategy();
        var processor1 = Substitute.For<IJobProcessor>();
        processor1.IsProcessing.Returns(true, true, true, false);
        var processor2 = Substitute.For<IJobProcessor>();
        strategy.AddProcessor(processor1);
        strategy.AddProcessor(processor2);

        //Act
        var next1 = await strategy.GetNextProcessorAsync();
        var next2 = await strategy.GetNextProcessorAsync();

        //Assert
        next1.Should().NotBeNull();
        next2.Should().NotBeNull();
    }
}