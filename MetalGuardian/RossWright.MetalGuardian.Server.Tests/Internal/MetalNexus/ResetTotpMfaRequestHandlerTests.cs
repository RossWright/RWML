using NSubstitute;
using static RossWright.MetalGuardian.ResetTotpMfa;

namespace RossWright.MetalGuardian.Server.Tests.Internal.MetalNexus;

public class ResetTotpMfaRequestHandlerTests
{
    [Fact]
    public async Task Handle_CallsResetUserWithCorrectParameters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var totpService = Substitute.For<IMetalGuardianTotpMfaService>();
        var sut = new ResetTotpMfaRequestHandler(totpService);
        var request = new Request { UserId = userId };
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await totpService.Received(1).ResetUser(userId, cancellationToken);
    }

    [Fact]
    public async Task Handle_PassesUserIdFromRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var totpService = Substitute.For<IMetalGuardianTotpMfaService>();
        var sut = new ResetTotpMfaRequestHandler(totpService);
        var request = new Request { UserId = userId };
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await totpService.Received(1).ResetUser(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToTotpService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var totpService = Substitute.For<IMetalGuardianTotpMfaService>();
        var sut = new ResetTotpMfaRequestHandler(totpService);
        var request = new Request { UserId = userId };
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await totpService.Received(1).ResetUser(userId, cancellationToken);
    }
}
