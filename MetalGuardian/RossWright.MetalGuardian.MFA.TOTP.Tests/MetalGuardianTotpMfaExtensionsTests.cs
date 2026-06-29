using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;

namespace RossWright.MetalGuardian.MFA.TOTP.UnitTests;

public class MetalGuardianTotpMfaExtensionsTests
{
    // ── UseMetalNexusTotpMfaEndpoints ────────────────────────────────────────────

    [Fact]
    public void UseMetalNexusTotpMfaEndpoints_ShouldCastToIOptionsBuilder_WhenCalled()
    {
        // Arrange
        var mockBuilder = Substitute.For<IMetalGuardianClientOptionsBuilder, IOptionsBuilder>();
        var addServicesCallbackCalled = false;

        ((IOptionsBuilder)mockBuilder).AddServices(Arg.Do<Action<IServiceCollection>>(callback =>
        {
            addServicesCallbackCalled = true;
            var services = new ServiceCollection();
            callback(services);
        }));

        // Act
        mockBuilder.UseMetalNexusTotpMfaEndpoints();

        // Assert
        addServicesCallbackCalled.ShouldBeTrue();
        ((IOptionsBuilder)mockBuilder).Received(1).AddServices(Arg.Any<Action<IServiceCollection>>());
    }

    [Fact]
    public void UseMetalNexusTotpMfaEndpoints_ShouldNotThrow_WhenCalled()
    {
        // Arrange
        var mockBuilder = Substitute.For<IMetalGuardianClientOptionsBuilder, IOptionsBuilder>();

        // Act & Assert
        Should.NotThrow(() => mockBuilder.UseMetalNexusTotpMfaEndpoints());
    }

    // ── NeedsToSetupTotpMfa ──────────────────────────────────────────────────────

    [Fact]
    public void NeedsToSetupTotpMfa_NullTokens_ReturnsFalse()
    {
        // Arrange
        IAuthenticationInformation? tokens = null;

        // Act
        var result = tokens.NeedsToSetupTotpMfa();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void NeedsToSetupTotpMfa_ClaimReturnsTrue_ReturnsTrue()
    {
        // Arrange
        var tokens = Substitute.For<IAuthenticationInformation>();
        tokens.GetAdditionalClaim(MetalGuardianTotpMfaClaimTypes.NeedsToSetupTotpMfaClaimType).Returns("true");

        // Act
        var result = tokens.NeedsToSetupTotpMfa();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void NeedsToSetupTotpMfa_ClaimReturnsFalse_ReturnsFalse()
    {
        // Arrange
        var tokens = Substitute.For<IAuthenticationInformation>();
        tokens.GetAdditionalClaim(MetalGuardianTotpMfaClaimTypes.NeedsToSetupTotpMfaClaimType).Returns("false");

        // Act
        var result = tokens.NeedsToSetupTotpMfa();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void NeedsToSetupTotpMfa_ClaimReturnsNull_ReturnsFalse()
    {
        // Arrange
        var tokens = Substitute.For<IAuthenticationInformation>();
        tokens.GetAdditionalClaim(MetalGuardianTotpMfaClaimTypes.NeedsToSetupTotpMfaClaimType).Returns((string?)null);

        // Act
        var result = tokens.NeedsToSetupTotpMfa();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void NeedsToSetupTotpMfa_ClaimReturnsInvalidString_ReturnsFalse()
    {
        // Arrange
        var tokens = Substitute.For<IAuthenticationInformation>();
        tokens.GetAdditionalClaim(MetalGuardianTotpMfaClaimTypes.NeedsToSetupTotpMfaClaimType).Returns("invalid");

        // Act
        var result = tokens.NeedsToSetupTotpMfa();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void NeedsToSetupTotpMfa_ClaimReturnsWhitespace_ReturnsFalse()
    {
        // Arrange
        var tokens = Substitute.For<IAuthenticationInformation>();
        tokens.GetAdditionalClaim(MetalGuardianTotpMfaClaimTypes.NeedsToSetupTotpMfaClaimType).Returns("   ");

        // Act
        var result = tokens.NeedsToSetupTotpMfa();

        // Assert
        result.ShouldBeFalse();
    }

    // ── HasTotpMfaEnabled ────────────────────────────────────────────────────────

    [Fact]
    public void HasTotpMfaEnabled_NullTokens_ReturnsFalse()
    {
        // Arrange
        IAuthenticationInformation? tokens = null;

        // Act
        var result = tokens.HasTotpMfaEnabled();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasTotpMfaEnabled_ClaimReturnsTrue_ReturnsTrue()
    {
        // Arrange
        var tokens = Substitute.For<IAuthenticationInformation>();
        tokens.GetAdditionalClaim(MetalGuardianTotpMfaClaimTypes.HasTotpMfaEnabledClaimType).Returns("true");

        // Act
        var result = tokens.HasTotpMfaEnabled();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void HasTotpMfaEnabled_ClaimReturnsFalse_ReturnsFalse()
    {
        // Arrange
        var tokens = Substitute.For<IAuthenticationInformation>();
        tokens.GetAdditionalClaim(MetalGuardianTotpMfaClaimTypes.HasTotpMfaEnabledClaimType).Returns("false");

        // Act
        var result = tokens.HasTotpMfaEnabled();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasTotpMfaEnabled_ClaimReturnsNull_ReturnsFalse()
    {
        // Arrange
        var tokens = Substitute.For<IAuthenticationInformation>();
        tokens.GetAdditionalClaim(MetalGuardianTotpMfaClaimTypes.HasTotpMfaEnabledClaimType).Returns((string?)null);

        // Act
        var result = tokens.HasTotpMfaEnabled();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasTotpMfaEnabled_ClaimReturnsInvalidString_ReturnsFalse()
    {
        // Arrange
        var tokens = Substitute.For<IAuthenticationInformation>();
        tokens.GetAdditionalClaim(MetalGuardianTotpMfaClaimTypes.HasTotpMfaEnabledClaimType).Returns("invalid");

        // Act
        var result = tokens.HasTotpMfaEnabled();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasTotpMfaEnabled_ClaimReturnsWhitespace_ReturnsFalse()
    {
        // Arrange
        var tokens = Substitute.For<IAuthenticationInformation>();
        tokens.GetAdditionalClaim(MetalGuardianTotpMfaClaimTypes.HasTotpMfaEnabledClaimType).Returns("   ");

        // Act
        var result = tokens.HasTotpMfaEnabled();

        // Assert
        result.ShouldBeFalse();
    }
}
