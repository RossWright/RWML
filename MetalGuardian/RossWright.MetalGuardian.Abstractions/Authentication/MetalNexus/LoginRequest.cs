using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalGuardian;

/// <summary>MetalNexus request type for the login endpoint (<c>POST /Authentication/Login</c>).</summary>
public static class Login
{
    /// <summary>Submits credentials to the MetalNexus login endpoint and returns authentication tokens on success.</summary>
    [ApiRequest(HttpProtocol.PostViaBody, path: "/Authentication/Login", tag: "Authentication"), Anonymous]
    public record Request : IRequest<AuthenticationTokens>
    {
        /// <summary>The username or other user identity string.</summary>
        public string UserIdentity { get; set; } = null!;

        /// <summary>The user's password.</summary>
        public string Password { get; set; } = null!;

        /// <summary>Optional device fingerprint string used for device-trust evaluation.</summary>
        public string? DeviceFingerprint { get; set; }
    }
}