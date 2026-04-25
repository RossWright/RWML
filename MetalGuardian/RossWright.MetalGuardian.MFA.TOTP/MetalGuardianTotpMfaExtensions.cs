using RossWright.MetalNexus;

namespace RossWright.MetalGuardian;

public static class MetalGuardianTotpMfaExtensions
{
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

    public static bool NeedsToSetupTotpMfa(this IAuthenticationInformation? tokens) => ParseOrNull.Bool(tokens?
            .GetAdditionalClaim(MetalGuardianTotpMfaClaimTypes.NeedsToSetupTotpMfaClaimType)) ?? false;

    public static bool HasTotpMfaEnabled(this IAuthenticationInformation? tokens) => ParseOrNull.Bool(tokens?
            .GetAdditionalClaim(MetalGuardianTotpMfaClaimTypes.HasTotpMfaEnabledClaimType)) ?? false;
}