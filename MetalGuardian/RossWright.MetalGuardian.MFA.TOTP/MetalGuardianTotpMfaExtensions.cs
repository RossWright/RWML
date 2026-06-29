using RossWright.MetalNexus;

namespace RossWright.MetalGuardian;

/// <summary>
/// Client-side extension methods for the MetalGuardian TOTP MFA add-on.
/// </summary>
public static class MetalGuardianTotpMfaExtensions
{
    /// <summary>
    /// Registers the MetalNexus TOTP MFA endpoints (SetupTotp, VerifyTotpMfa, ResetTotpMfa)
    /// with the MetalGuardian client options builder.
    /// </summary>
    public static void UseMetalNexusTotpMfaEndpoints(
        this IMetalGuardianClientOptionsBuilder guardianBuilder)
    {
        ((IOptionsBuilder)guardianBuilder).AddServices(_ =>
        {
            _.AddMetalNexusEndpoints(
                typeof(SetupTotp.Request),
                typeof(VerifyTotpMfa.Request),
                typeof(ResetTotpMfa.Request));
        });
    }

    /// <summary>
    /// Returns <c>true</c> if the token indicates the user still needs to set up TOTP MFA.
    /// </summary>
    public static bool NeedsToSetupTotpMfa(this IAuthenticationInformation? tokens) => ParseOrNull.Bool(tokens?
            .GetAdditionalClaim(MetalGuardianTotpMfaClaimTypes.NeedsToSetupTotpMfaClaimType)) ?? false;

    /// <summary>
    /// Returns <c>true</c> if the token indicates the user has TOTP MFA enabled.
    /// </summary>
    public static bool HasTotpMfaEnabled(this IAuthenticationInformation? tokens) => ParseOrNull.Bool(tokens?
            .GetAdditionalClaim(MetalGuardianTotpMfaClaimTypes.HasTotpMfaEnabledClaimType)) ?? false;
}