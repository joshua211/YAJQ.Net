using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;
using YAJQ.Core.JobHandler;
using YAJQ.Core.JobHandler.Interfaces;
using YAJQ.Core.JobProcessor;
using YAJQ.Core.JobProcessor.Interfaces;

namespace YAJQ.Tests.Unit.SelectionStrategy;

public class RoundRobinTests : SelectionStrategyTests
{
    public override IProcessorSelectionStrategy GetStrategy()
    {
        return new RoundRobinProcessorSelectionStrategy();
    }

    [Fact]
    public async Task ReturnsAllProcessorsOnce()
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
        var next3 = await strategy.GetNextProcessorAsync();


        //Assert
        next1.Should().Be(processor1);
        next2.Should().Be(processor2);
        next3.Should().Be(processor1);
    }
}