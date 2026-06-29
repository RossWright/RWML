using System.ComponentModel;

namespace RossWright.MetalGuardian.Internal;

/// <summary>
/// MetalGuardian-specific claim type URI constants injected into JWTs by the server.
/// These are used internally; prefer reading values through <see cref="IAuthenticationInformation"/> members.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class MetalGuardianClaimTypes
{
    /// <summary>Claim type URI indicating the token is provisional (MFA pending).</summary>
    public const string ProvisionalLogin = "http://schemas.rosswright.com/ws/2026/01/metalguardian/claims/provisionallogin";

    /// <summary>Claim type URI indicating whether the current device is recognized as a trusted device.</summary>
    public const string IsKnownDevice = "http://schemas.rosswright.com/ws/2026/01/metalguardian/claims/isknowndevice";
}
