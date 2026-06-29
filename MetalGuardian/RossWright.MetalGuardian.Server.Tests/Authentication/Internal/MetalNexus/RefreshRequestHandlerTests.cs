using NSubstitute;
using RossWright.MetalGuardian.MetalNexus;
using Shouldly;
using static RossWright.MetalGuardian.Refresh;

namespace RossWright.MetalGuardian.Server.Tests.Authentication.Internal.MetalNexus;

public class RefreshRequestHandlerTests
{
    [Fact]
    public void Constructor_StoresAuthenticationService()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();

        // Act
        var sut = new RefreshRequestHandler(authentSvc);

        // Assert - verify by calling Handle and checking the service is used
        var request = new Request { AccessToken = "access", RefreshToken = "refresh" };
        var cancellationToken = CancellationToken.None;
        _ = sut.Handle(request, cancellationToken);

        authentSvc.Received(1).Refresh(request, cancellationToken);
    }

    [Fact]
    public async Task Handle_CallsAuthenticationServiceRefresh()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var sut = new RefreshRequestHandler(authentSvc);
        var request = new Request { AccessToken = "test-access", RefreshToken = "test-refresh" };
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await authentSvc.Received(1).Refresh(request, cancellationToken);
    }

    [Fact]
    public async Task Handle_PassesRequestToAuthenticationService()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var sut = new RefreshRequestHandler(authentSvc);
        var request = new Request { AccessToken = "access", RefreshToken = "refresh" };
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await authentSvc.Received(1).Refresh(Arg.Is<AuthenticationTokens>(r => r == request), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToAuthenticationService()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var sut = new RefreshRequestHandler(authentSvc);
        var request = new Request { AccessToken = "access", RefreshToken = "refresh" };
        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await authentSvc.Received(1).Refresh(Arg.Any<AuthenticationTokens>(), Arg.Is<CancellationToken>(ct => ct == cancellationToken));
    }

    [Fact]
    public async Task Handle_ReturnsAuthenticationTokensFromService()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var expectedTokens = new AuthenticationTokens { AccessToken = "new-access", RefreshToken = "new-refresh" };
        authentSvc.Refresh(Arg.Any<AuthenticationTokens>(), Arg.Any<CancellationToken>())
            .Returns(expectedTokens);
        var sut = new RefreshRequestHandler(authentSvc);
        var request = new Request { AccessToken = "old-access", RefreshToken = "old-refresh" };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.Handle(request, cancellationToken);

        // Assert
        result.ShouldBe(expectedTokens);
    }
}
