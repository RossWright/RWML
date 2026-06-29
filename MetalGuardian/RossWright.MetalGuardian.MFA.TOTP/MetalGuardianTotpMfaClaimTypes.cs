namespace RossWright.MetalGuardian;

/// <summary>
/// MetalGuardian-specific claim type URI constants injected into JWTs by the TOTP MFA claims provider.
/// Use the extension methods on <see cref="MetalGuardianTotpMfaExtensions"/> to read these values
/// from an <see cref="IAuthenticationInformation"/> instance.
/// </summary>
public class MetalGuardianTotpMfaClaimTypes
{
    /// <summary>
    /// Claim type URI indicating that the user has not yet set up TOTP MFA and should be directed to the setup flow.
    /// </summary>
    public const string NeedsToSetupTotpMfaClaimType =
        "http://schemas.rosswright.com/ws/2026/01/identity/claims/NeedsToSetupTotpMfa";

    /// <summary>
    /// Claim type URI indicating that the user has TOTP MFA enabled on their account.
    /// </summary>
    public const string HasTotpMfaEnabledClaimType =
        "http://schemas.rosswright.com/ws/2026/01/identity/claims/HasTotpMfaEnabled";
}