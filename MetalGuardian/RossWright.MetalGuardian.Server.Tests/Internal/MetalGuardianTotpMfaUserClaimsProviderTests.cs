using NSubstitute;
using Shouldly;

namespace RossWright.MetalGuardian.Server.Tests.Internal;

public class MetalGuardianTotpMfaUserClaimsProviderTests
{
    [Fact]
    public async Task GetClaims_MfaEnabledAndRequired_ReturnsCorrectClaims()
    {
        // Arrange
        var provider = new MetalGuardianTotpMfaUserClaimsProvider();
        var user = Substitute.For<ITotpMfaAuthenticationUser>();
        user.IsMfaTotpEnabled.Returns(true);
        user.IsMfaTotpRequired.Returns(true);

        // Act
        var result = await provider.GetClaims(user, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        var claims = result.ToList();
        claims.Count.ShouldBe(2);
        claims[0].Item1.ShouldBe(MetalGuardianTotpMfaClaimTypes.NeedsToSetupTotpMfaClaimType);
        claims[0].Item2.ShouldBe("False");
        claims[1].Item1.ShouldBe(MetalGuardianTotpMfaClaimTypes.HasTotpMfaEnabledClaimType);
        claims[1].Item2.ShouldBe("True");
    }

    [Fact]
    public async Task GetClaims_MfaNotEnabledButRequired_ReturnsNeedsSetupTrue()
    {
        // Arrange
        var provider = new MetalGuardianTotpMfaUserClaimsProvider();
        var user = Substitute.For<ITotpMfaAuthenticationUser>();
        user.IsMfaTotpEnabled.Returns(false);
        user.IsMfaTotpRequired.Returns(true);

        // Act
        var result = await provider.GetClaims(user, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        var claims = result.ToList();
        claims.Count.ShouldBe(2);
        claims[0].Item1.ShouldBe(MetalGuardianTotpMfaClaimTypes.NeedsToSetupTotpMfaClaimType);
        claims[0].Item2.ShouldBe("True");
        claims[1].Item1.ShouldBe(MetalGuardianTotpMfaClaimTypes.HasTotpMfaEnabledClaimType);
        claims[1].Item2.ShouldBe("False");
    }

    [Fact]
    public async Task GetClaims_MfaEnabledButNotRequired_ReturnsCorrectClaims()
    {
        // Arrange
        var provider = new MetalGuardianTotpMfaUserClaimsProvider();
        var user = Substitute.For<ITotpMfaAuthenticationUser>();
        user.IsMfaTotpEnabled.Returns(true);
        user.IsMfaTotpRequired.Returns(false);

        // Act
        var result = await provider.GetClaims(user, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        var claims = result.ToList();
        claims.Count.ShouldBe(2);
        claims[0].Item1.ShouldBe(MetalGuardianTotpMfaClaimTypes.NeedsToSetupTotpMfaClaimType);
        claims[0].Item2.ShouldBe("False");
        claims[1].Item1.ShouldBe(MetalGuardianTotpMfaClaimTypes.HasTotpMfaEnabledClaimType);
        claims[1].Item2.ShouldBe("True");
    }

    [Fact]
    public async Task GetClaims_MfaNotEnabledAndNotRequired_ReturnsCorrectClaims()
    {
        // Arrange
        var provider = new MetalGuardianTotpMfaUserClaimsProvider();
        var user = Substitute.For<ITotpMfaAuthenticationUser>();
        user.IsMfaTotpEnabled.Returns(false);
        user.IsMfaTotpRequired.Returns(false);

        // Act
        var result = await provider.GetClaims(user, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        var claims = result.ToList();
        claims.Count.ShouldBe(2);
        claims[0].Item1.ShouldBe(MetalGuardianTotpMfaClaimTypes.NeedsToSetupTotpMfaClaimType);
        claims[0].Item2.ShouldBe("False");
        claims[1].Item1.ShouldBe(MetalGuardianTotpMfaClaimTypes.HasTotpMfaEnabledClaimType);
        claims[1].Item2.ShouldBe("False");
    }
}
