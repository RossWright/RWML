using NSubstitute;
using RossWright.MetalGuardian.MetalNexus;
using Shouldly;
using static RossWright.MetalGuardian.Login;

namespace RossWright.MetalGuardian.Server.Tests.Authentication.Internal.MetalNexus;

public class LoginRequestHandlerTests
{
    [Fact]
    public void Constructor_StoresAuthenticationService()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();

        // Act
        var sut = new LoginRequestHandler(authentSvc);

        // Assert - verify by calling Handle and checking the service is used
        var request = new Request { UserIdentity = "user", Password = "pass" };
        var cancellationToken = CancellationToken.None;
        _ = sut.Handle(request, cancellationToken);

        authentSvc.Received(1).Login(request.UserIdentity, request.Password, request.DeviceFingerprint, cancellationToken);
    }

    [Fact]
    public async Task Handle_CallsAuthenticationServiceLogin()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var sut = new LoginRequestHandler(authentSvc);
        var request = new Request { UserIdentity = "testuser", Password = "testpass" };
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await authentSvc.Received(1).Login(request.UserIdentity, request.Password, request.DeviceFingerprint, cancellationToken);
    }

    [Fact]
    public async Task Handle_PassesUserIdentityToAuthenticationService()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var sut = new LoginRequestHandler(authentSvc);
        var request = new Request { UserIdentity = "testuser", Password = "testpass" };
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await authentSvc.Received(1).Login(Arg.Is<string>(u => u == "testuser"), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PassesPasswordToAuthenticationService()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var sut = new LoginRequestHandler(authentSvc);
        var request = new Request { UserIdentity = "testuser", Password = "testpass" };
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await authentSvc.Received(1).Login(Arg.Any<string>(), Arg.Is<string>(p => p == "testpass"), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PassesDeviceFingerprintToAuthenticationService()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var sut = new LoginRequestHandler(authentSvc);
        var request = new Request { UserIdentity = "testuser", Password = "testpass", DeviceFingerprint = "device123" };
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await authentSvc.Received(1).Login(Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string?>(d => d == "device123"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PassesNullDeviceFingerprintToAuthenticationService()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var sut = new LoginRequestHandler(authentSvc);
        var request = new Request { UserIdentity = "testuser", Password = "testpass", DeviceFingerprint = null };
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await authentSvc.Received(1).Login(Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string?>(d => d == null), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToAuthenticationService()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var sut = new LoginRequestHandler(authentSvc);
        var request = new Request { UserIdentity = "testuser", Password = "testpass" };
        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await authentSvc.Received(1).Login(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Is<CancellationToken>(ct => ct == cancellationToken));
    }

    [Fact]
    public async Task Handle_ReturnsAuthenticationTokensFromService()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var expectedTokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
        authentSvc.Login(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(expectedTokens);
        var sut = new LoginRequestHandler(authentSvc);
        var request = new Request { UserIdentity = "testuser", Password = "testpass" };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.Handle(request, cancellationToken);

        // Assert
        result.ShouldBe(expectedTokens);
    }
}
