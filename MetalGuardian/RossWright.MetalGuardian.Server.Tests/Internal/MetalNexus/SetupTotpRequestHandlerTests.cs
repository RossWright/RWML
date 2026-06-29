using NSubstitute;
using Shouldly;
using static RossWright.MetalGuardian.SetupTotp;

namespace RossWright.MetalGuardian.Server.Tests.Internal.MetalNexus;

public class SetupTotpRequestHandlerTests
{
    [Fact]
    public async Task Handle_CallsGetSetupQrCodeWithCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(userId);
        var totpService = Substitute.For<IMetalGuardianTotpMfaService>();
        var sut = new SetupTotpRequestHandler(user, totpService);
        var request = new Request();
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await totpService.Received(1).GetSetupQrCode(userId, cancellationToken);
    }

    [Fact]
    public async Task Handle_ReturnsResponseWithQrCodeFromService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(userId);
        var totpService = Substitute.For<IMetalGuardianTotpMfaService>();
        var expectedQrCode = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
        totpService.GetSetupQrCode(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(expectedQrCode);
        var sut = new SetupTotpRequestHandler(user, totpService);
        var request = new Request();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.Handle(request, cancellationToken);

        // Assert
        result.QrCode.ShouldBe(expectedQrCode);
    }

    [Fact]
    public async Task Handle_PropagatesCancellationToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(userId);
        var totpService = Substitute.For<IMetalGuardianTotpMfaService>();
        var sut = new SetupTotpRequestHandler(user, totpService);
        var request = new Request();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await totpService.Received(1).GetSetupQrCode(Arg.Any<Guid>(), cancellationToken);
    }

    [Fact]
    public async Task Handle_ReturnsNewResponseInstance()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(userId);
        var totpService = Substitute.For<IMetalGuardianTotpMfaService>();
        totpService.GetSetupQrCode(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns("qr-code-data");
        var sut = new SetupTotpRequestHandler(user, totpService);
        var request = new Request();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.Handle(request, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Response>();
    }
}
