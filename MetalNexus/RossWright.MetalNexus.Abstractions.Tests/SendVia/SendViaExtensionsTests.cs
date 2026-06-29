using NSubstitute;
using RossWright.MetalChain;
using RossWright.MetalNexus;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests.SendVia;

public class SendViaExtensionsTests
{
    private class TestRequest : IRequest
    {
    }

    private class TestRequestWithResponse : IRequest<TestResponse>
    {
    }

    private class TestResponse
    {
        public string Value { get; init; } = string.Empty;
    }

    [Fact]
    public async Task SendVia_WithRequest_ShouldCallMediatorSendWithSendViaWrapper()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var request = new TestRequest();
        var connectionName = "TestConnection";
        var cancellationToken = new CancellationToken();
        object? capturedRequest = null;
        mediator.Send(Arg.Do<object>(x => capturedRequest = x), Arg.Any<CancellationToken>()).Returns(Task.FromResult<object?>(null));

        // Act
        await mediator.SendVia(connectionName, request, cancellationToken);

        // Assert
        capturedRequest.ShouldNotBeNull();
        capturedRequest.ShouldBeOfType<SendVia<TestRequest>>();
        var sendViaRequest = (SendVia<TestRequest>)capturedRequest;
        sendViaRequest.ConnectionName.ShouldBe(connectionName);
        sendViaRequest.Request.ShouldBe(request);
    }

    [Fact]
    public async Task SendVia_WithRequestAndEmptyConnectionName_ShouldPassEmptyConnectionName()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var request = new TestRequest();
        var connectionName = string.Empty;
        var cancellationToken = CancellationToken.None;
        object? capturedRequest = null;
        mediator.Send(Arg.Do<object>(x => capturedRequest = x), Arg.Any<CancellationToken>()).Returns(Task.FromResult<object?>(null));

        // Act
        await mediator.SendVia(connectionName, request, cancellationToken);

        // Assert
        capturedRequest.ShouldNotBeNull();
        var sendViaRequest = (SendVia<TestRequest>)capturedRequest;
        sendViaRequest.ConnectionName.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task SendVia_WithRequestAndCancellationToken_ShouldPassCancellationTokenToMediator()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var request = new TestRequest();
        var connectionName = "TestConnection";
        var cancellationToken = new CancellationToken(true);
        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<object?>(null));

        // Act
        await mediator.SendVia(connectionName, request, cancellationToken);

        // Assert
        await mediator.Received(1).Send(Arg.Any<object>(), cancellationToken);
    }

    [Fact]
    public async Task SendVia_WithResponse_ShouldCallMediatorSendWithSendViaWrapper()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var request = new TestRequestWithResponse();
        var connectionName = "TestConnection";
        var cancellationToken = new CancellationToken();
        var expectedResponse = new TestResponse { Value = "TestValue" };
        object? capturedRequest = null;
        mediator.Send(Arg.Do<object>(x => capturedRequest = x), Arg.Any<CancellationToken>()).Returns(Task.FromResult<object?>(expectedResponse));

        // Act
        var result = await mediator.SendVia(connectionName, request, cancellationToken);

        // Assert
        capturedRequest.ShouldNotBeNull();
        capturedRequest.ShouldBeOfType<SendVia<TestRequestWithResponse, TestResponse>>();
        var sendViaRequest = (SendVia<TestRequestWithResponse, TestResponse>)capturedRequest;
        sendViaRequest.ConnectionName.ShouldBe(connectionName);
        sendViaRequest.Request.ShouldBe(request);
    }

    [Fact]
    public async Task SendVia_WithResponse_ShouldReturnResponseFromMediator()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var request = new TestRequestWithResponse();
        var connectionName = "TestConnection";
        var cancellationToken = CancellationToken.None;
        var expectedResponse = new TestResponse { Value = "TestValue" };
        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<object?>(expectedResponse));

        // Act
        var result = await mediator.SendVia(connectionName, request, cancellationToken);

        // Assert
        result.ShouldBe(expectedResponse);
    }

    [Fact]
    public async Task SendVia_WithResponse_WhenMediatorReturnsNull_ShouldReturnNull()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var request = new TestRequestWithResponse();
        var connectionName = "TestConnection";
        var cancellationToken = CancellationToken.None;
        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<object?>(null));

        // Act
        var result = await mediator.SendVia(connectionName, request, cancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task SendVia_WithResponseAndEmptyConnectionName_ShouldPassEmptyConnectionName()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var request = new TestRequestWithResponse();
        var connectionName = string.Empty;
        var cancellationToken = CancellationToken.None;
        object? capturedRequest = null;
        mediator.Send(Arg.Do<object>(x => capturedRequest = x), Arg.Any<CancellationToken>()).Returns(Task.FromResult<object?>(null));

        // Act
        await mediator.SendVia(connectionName, request, cancellationToken);

        // Assert
        capturedRequest.ShouldNotBeNull();
        var sendViaRequest = (SendVia<TestRequestWithResponse, TestResponse>)capturedRequest;
        sendViaRequest.ConnectionName.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task SendVia_WithResponseAndCancellationToken_ShouldPassCancellationTokenToMediator()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var request = new TestRequestWithResponse();
        var connectionName = "TestConnection";
        var cancellationToken = new CancellationToken(true);
        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<object?>(null));

        // Act
        await mediator.SendVia(connectionName, request, cancellationToken);

        // Assert
        await mediator.Received(1).Send(Arg.Any<object>(), cancellationToken);
    }
}
