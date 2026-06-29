using NSubstitute;
using RossWright.MetalGuardian.MetalNexus;
using static RossWright.MetalGuardian.Logout;

namespace RossWright.MetalGuardian.Server.Tests.Authentication.Internal.MetalNexus;

public class LogoutRequestHandlerTests
{
    [Fact]
    public void Constructor_StoresAuthenticationService()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();

        // Act
        var sut = new LogoutRequestHandler(authentSvc);

        // Assert - verify by calling Handle and checking the service is used
        var request = new Request();
        var cancellationToken = CancellationToken.None;
        _ = sut.Handle(request, cancellationToken);

        authentSvc.Received(1).Logout(request, cancellationToken);
    }

    [Fact]
    public async Task Handle_CallsAuthenticationServiceLogout()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var sut = new LogoutRequestHandler(authentSvc);
        var request = new Request();
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await authentSvc.Received(1).Logout(request, cancellationToken);
    }

    [Fact]
    public async Task Handle_PassesRequestToAuthenticationService()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var sut = new LogoutRequestHandler(authentSvc);
        var request = new Request();
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await authentSvc.Received(1).Logout(Arg.Is<Request>(r => r == request), cancellationToken);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToAuthenticationService()
    {
        // Arrange
        var authentSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var sut = new LogoutRequestHandler(authentSvc);
        var request = new Request();
        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await authentSvc.Received(1).Logout(request, Arg.Is<CancellationToken>(ct => ct == cancellationToken));
    }
}
