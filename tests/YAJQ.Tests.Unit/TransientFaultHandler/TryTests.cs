using System;
using YAJQ.Core;
using YAJQ.Core.JobProcessor;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace YAJQ.Tests.Unit.TransientFaultHandler;

public class TryTests : TestBase
{
    [Fact]
    public void CanHandleNonThrowingAction()
    {
        //Arrange
        var wasCalled = false;
        Action action = () => wasCalled = true;

        //Act
        var result = transientFaultHandler.Try(action);

        //Assert
        result.WasSuccess.Should().BeTrue();
        wasCalled.Should().BeTrue();
    }

    [Fact]
    public void ReturnsLastException()
    {
        //Arrange
        var options = new YAJQOptions
        {
            TransientFaultBaseDelay = 1,
            TransientFaultMaxTries = 1
        };
        var handler =
            new DefaultTransientFaultHandler(Substitute.For<ILogger<DefaultTransientFaultHandler>>(), options);
        Action action = () => throw new ApplicationException();

        //Act
        var result = handler.Try(action);

        //Assert
        result.WasSuccess.Should().BeFalse();
        result.LastException.Should().BeOfType<ApplicationException>();
    }

    [Fact]
    public void CanHandleTransientFaultAction()
    {
        //Arrange
        var calls = 0;
        var wasCalled = false;
        Action action = () =>
        {
            calls++;
            if (calls == 1)
                throw new Exception();

            wasCalled = true;
        };

        //Act
        var result = transientFaultHandler.Try(action);

        //Assert
        result.WasSuccess.Should().BeTrue();
        calls.Should().Be(2);
        wasCalled.Should().BeTrue();
    }
}