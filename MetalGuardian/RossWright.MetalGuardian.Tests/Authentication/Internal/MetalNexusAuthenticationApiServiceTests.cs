using NSubstitute;
using RossWright.MetalChain;
using RossWright.MetalGuardian;
using RossWright.MetalGuardian.Authentication;
using RossWright.MetalNexus;
using Shouldly;

namespace RossWright.MetalGuardian.Tests.Authentication.Internal;

public class MetalNexusAuthenticationApiServiceTests
{
    [Fact]
    public async Task Login_WithDeviceFingerprintService_ReturnsTokens()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var deviceFingerprintSvc = Substitute.For<IDeviceFingerprintService>();
        var expectedTokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
        var deviceFingerprint = "device123";

        deviceFingerprintSvc.GetFingerprint().Returns(Task.FromResult(deviceFingerprint));
        
        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<object?>(expectedTokens));

        var service = new MetalNexusAuthenticationApiService(mediator, deviceFingerprintSvc);

        // Act
        var result = await service.Login("user1", "pass123", "default");

        // Assert
        result.ShouldBe(expectedTokens);
        await deviceFingerprintSvc.Received(1).GetFingerprint();
        await mediator.Received(1).Send(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Login_WithoutDeviceFingerprintService_ReturnsTokens()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var expectedTokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };

        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<object?>(expectedTokens));

        var service = new MetalNexusAuthenticationApiService(mediator, null);

        // Act
        var result = await service.Login("user1", "pass123", "default");

        // Assert
        result.ShouldBe(expectedTokens);
        await mediator.Received(1).Send(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Login_WhenNotAuthenticatedExceptionThrown_ReturnsNull()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns<object?>(_ => throw new RossWright.NotAuthenticatedException());

        var service = new MetalNexusAuthenticationApiService(mediator);

        // Act
        var result = await service.Login("user1", "pass123", "default");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task Login_PassesCancellationToken()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var cts = new CancellationTokenSource();
        var expectedTokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };

        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<object?>(expectedTokens));

        var service = new MetalNexusAuthenticationApiService(mediator);

        // Act
        await service.Login("user1", "pass123", "default", cts.Token);

        // Assert
        await mediator.Received(1).Send(Arg.Any<object>(), cts.Token);
    }

    [Fact]
    public async Task Logout_CallsMediator()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var tokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };

        var service = new MetalNexusAuthenticationApiService(mediator);

        // Act
        await service.Logout(tokens, "default");

        // Assert
        await mediator.Received(1).Send(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Logout_PassesCancellationToken()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var tokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
        var cts = new CancellationTokenSource();

        var service = new MetalNexusAuthenticationApiService(mediator);

        // Act
        await service.Logout(tokens, "default", cts.Token);

        // Assert
        await mediator.Received(1).Send(Arg.Any<object>(), cts.Token);
    }

    [Fact]
    public async Task Refresh_Success_ReturnsNewTokens()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var tokens = new AuthenticationTokens { AccessToken = "oldAccess", RefreshToken = "oldRefresh" };
        var newTokens = new AuthenticationTokens { AccessToken = "newAccess", RefreshToken = "newRefresh" };

        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<object?>(newTokens));

        var service = new MetalNexusAuthenticationApiService(mediator);

        // Act
        var result = await service.Refresh(tokens, "default");

        // Assert
        result.ShouldBe(newTokens);
        await mediator.Received(1).Send(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Refresh_WhenNotAuthenticatedExceptionThrown_ReturnsNull()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var tokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };

        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns<object?>(_ => throw new RossWright.NotAuthenticatedException());

        var service = new MetalNexusAuthenticationApiService(mediator);

        // Act
        var result = await service.Refresh(tokens, "default");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task Refresh_PassesCancellationToken()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();
        var tokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
        var cts = new CancellationTokenSource();
        var newTokens = new AuthenticationTokens { AccessToken = "newAccess", RefreshToken = "newRefresh" };

        mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<object?>(newTokens));

        var service = new MetalNexusAuthenticationApiService(mediator);

        // Act
        await service.Refresh(tokens, "default", cts.Token);

        // Assert
        await mediator.Received(1).Send(Arg.Any<object>(), cts.Token);
    }
}
