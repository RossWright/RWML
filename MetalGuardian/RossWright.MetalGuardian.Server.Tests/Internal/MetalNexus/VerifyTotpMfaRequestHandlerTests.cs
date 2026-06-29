using NSubstitute;
using Shouldly;
using static RossWright.MetalGuardian.VerifyTotpMfa;

namespace RossWright.MetalGuardian.Server.Tests.Internal.MetalNexus;

public class VerifyTotpMfaRequestHandlerTests
{
    [Fact]
    public async Task Handle_CallsTotpServiceVerifyCodeWithCorrectParameters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(userId);
        var totpService = Substitute.For<IMetalGuardianTotpMfaService>();
        var sut = new VerifyTotpMfaRequestHandler(user, totpService);
        var request = new Request { Code = "123456", DeviceFingerprint = "fingerprint123" };
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await totpService.Received(1).VerifyCode(userId, request.Code, request.DeviceFingerprint, cancellationToken);
    }

    [Fact]
    public async Task Handle_ReturnsTokensFromTotpService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(userId);
        var totpService = Substitute.For<IMetalGuardianTotpMfaService>();
        var expectedTokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
        totpService.VerifyCode(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(expectedTokens);
        var sut = new VerifyTotpMfaRequestHandler(user, totpService);
        var request = new Request { Code = "123456", DeviceFingerprint = "fingerprint123" };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.Handle(request, cancellationToken);

        // Assert
        result.ShouldBe(expectedTokens);
    }

    [Fact]
    public async Task Handle_ReturnsNullWhenVerificationFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(userId);
        var totpService = Substitute.For<IMetalGuardianTotpMfaService>();
        totpService.VerifyCode(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((AuthenticationTokens?)null);
        var sut = new VerifyTotpMfaRequestHandler(user, totpService);
        var request = new Request { Code = "invalid", DeviceFingerprint = "fingerprint123" };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.Handle(request, cancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_PassesNullDeviceFingerprintWhenNotProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(userId);
        var totpService = Substitute.For<IMetalGuardianTotpMfaService>();
        var sut = new VerifyTotpMfaRequestHandler(user, totpService);
        var request = new Request { Code = "123456", DeviceFingerprint = null };
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await totpService.Received(1).VerifyCode(userId, request.Code, null, cancellationToken);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToTotpService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(userId);
        var totpService = Substitute.For<IMetalGuardianTotpMfaService>();
        var sut = new VerifyTotpMfaRequestHandler(user, totpService);
        var request = new Request { Code = "123456", DeviceFingerprint = "fingerprint123" };
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await totpService.Received(1).VerifyCode(userId, request.Code, request.DeviceFingerprint, cancellationToken);
    }

    [Fact]
    public async Task Handle_PassesUserIdFromCurrentUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(userId);
        var totpService = Substitute.For<IMetalGuardianTotpMfaService>();
        var sut = new VerifyTotpMfaRequestHandler(user, totpService);
        var request = new Request { Code = "123456", DeviceFingerprint = "fingerprint123" };
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Handle(request, cancellationToken);

        // Assert
        await totpService.Received(1).VerifyCode(userId, Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }
}
