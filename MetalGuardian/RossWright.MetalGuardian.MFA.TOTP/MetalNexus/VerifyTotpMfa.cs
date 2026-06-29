using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalGuardian;

/// <summary>
/// MetalNexus request type for verifying a TOTP code during MFA (<c>POST /Authentication/VerifyTotp</c>).
/// On success the server returns a full (non-provisional) set of authentication tokens.
/// </summary>
public static class VerifyTotpMfa
{
    /// <summary>Submits the TOTP code to the server. Returns new non-provisional tokens on success, or <c>null</c> if the code is invalid.</summary>
    [ApiRequest(HttpProtocol.PostViaBody, path: "/Authentication/VerifyTotp", tag: "Authentication")]
    [Authenticated(AllowProvisional = true)]
    public class Request : IRequest<AuthenticationTokens?>
    {
        /// <summary>The six-digit (or configured length) TOTP code from the authenticator app.</summary>
        public string Code { get; set; } = null!;

        /// <summary>Optional device fingerprint; if provided and valid, the device will be trusted and future MFA prompts skipped.</summary>
        public string? DeviceFingerprint { get; set; }
    }
}
