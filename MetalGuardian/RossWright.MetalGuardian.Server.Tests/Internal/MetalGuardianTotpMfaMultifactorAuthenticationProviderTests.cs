using NSubstitute;
using Shouldly;

namespace RossWright.MetalGuardian.Server.Tests.Internal;

public class MetalGuardianTotpMfaMultifactorAuthenticationProviderTests
{
    [Fact]
    public void ShouldLoginAsProvisional_MfaRequiredButNotEnabled_ReturnsTrue()
    {
        // Arrange
        var provider = new MetalGuardianTotpMfaMultifactorAuthenticationProvider();
        var user = Substitute.For<ITotpMfaAuthenticationUser>();
        user.IsMfaTotpRequired.Returns(true);
        user.IsMfaTotpEnabled.Returns(false);

        // Act
        var result = provider.ShouldLoginAsProvisional(user, true);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ShouldLoginAsProvisional_MfaEnabledAndUnknownDevice_ReturnsTrue()
    {
        // Arrange
        var provider = new MetalGuardianTotpMfaMultifactorAuthenticationProvider();
        var user = Substitute.For<ITotpMfaAuthenticationUser>();
        user.IsMfaTotpRequired.Returns(false);
        user.IsMfaTotpEnabled.Returns(true);

        // Act
        var result = provider.ShouldLoginAsProvisional(user, false);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ShouldLoginAsProvisional_MfaEnabledAndDeviceUnknown_ReturnsTrue()
    {
        // Arrange
        var provider = new MetalGuardianTotpMfaMultifactorAuthenticationProvider();
        var user = Substitute.For<ITotpMfaAuthenticationUser>();
        user.IsMfaTotpRequired.Returns(false);
        user.IsMfaTotpEnabled.Returns(true);

        // Act
        var result = provider.ShouldLoginAsProvisional(user, null);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ShouldLoginAsProvisional_MfaEnabledAndKnownDevice_ReturnsFalse()
    {
        // Arrange
        var provider = new MetalGuardianTotpMfaMultifactorAuthenticationProvider();
        var user = Substitute.For<ITotpMfaAuthenticationUser>();
        user.IsMfaTotpRequired.Returns(false);
        user.IsMfaTotpEnabled.Returns(true);

        // Act
        var result = provider.ShouldLoginAsProvisional(user, true);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldLoginAsProvisional_MfaNotRequiredNotEnabledUnknownDevice_ReturnsFalse()
    {
        // Arrange
        var provider = new MetalGuardianTotpMfaMultifactorAuthenticationProvider();
        var user = Substitute.For<ITotpMfaAuthenticationUser>();
        user.IsMfaTotpRequired.Returns(false);
        user.IsMfaTotpEnabled.Returns(false);

        // Act
        var result = provider.ShouldLoginAsProvisional(user, false);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldLoginAsProvisional_MfaNotRequiredNotEnabledKnownDevice_ReturnsFalse()
    {
        // Arrange
        var provider = new MetalGuardianTotpMfaMultifactorAuthenticationProvider();
        var user = Substitute.For<ITotpMfaAuthenticationUser>();
        user.IsMfaTotpRequired.Returns(false);
        user.IsMfaTotpEnabled.Returns(false);

        // Act
        var result = provider.ShouldLoginAsProvisional(user, true);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldLoginAsProvisional_MfaNotRequiredNotEnabledDeviceNull_ReturnsFalse()
    {
        // Arrange
        var provider = new MetalGuardianTotpMfaMultifactorAuthenticationProvider();
        var user = Substitute.For<ITotpMfaAuthenticationUser>();
        user.IsMfaTotpRequired.Returns(false);
        user.IsMfaTotpEnabled.Returns(false);

        // Act
        var result = provider.ShouldLoginAsProvisional(user, null);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldLoginAsProvisional_MfaRequiredAndEnabledUnknownDevice_ReturnsTrue()
    {
        // Arrange
        var provider = new MetalGuardianTotpMfaMultifactorAuthenticationProvider();
        var user = Substitute.For<ITotpMfaAuthenticationUser>();
        user.IsMfaTotpRequired.Returns(true);
        user.IsMfaTotpEnabled.Returns(true);

        // Act
        var result = provider.ShouldLoginAsProvisional(user, false);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ShouldLoginAsProvisional_MfaRequiredAndEnabledDeviceNull_ReturnsTrue()
    {
        // Arrange
        var provider = new MetalGuardianTotpMfaMultifactorAuthenticationProvider();
        var user = Substitute.For<ITotpMfaAuthenticationUser>();
        user.IsMfaTotpRequired.Returns(true);
        user.IsMfaTotpEnabled.Returns(true);

        // Act
        var result = provider.ShouldLoginAsProvisional(user, null);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ShouldLoginAsProvisional_MfaRequiredAndEnabledKnownDevice_ReturnsFalse()
    {
        // Arrange
        var provider = new MetalGuardianTotpMfaMultifactorAuthenticationProvider();
        var user = Substitute.For<ITotpMfaAuthenticationUser>();
        user.IsMfaTotpRequired.Returns(true);
        user.IsMfaTotpEnabled.Returns(true);

        // Act
        var result = provider.ShouldLoginAsProvisional(user, true);

        // Assert
        result.ShouldBeFalse();
    }
}
