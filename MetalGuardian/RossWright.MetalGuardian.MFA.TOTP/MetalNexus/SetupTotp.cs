using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalGuardian;

/// <summary>
/// MetalNexus request/response pair for initiating TOTP MFA setup (<c>GET /Authentication/SetupTotp</c>).
/// Requires a provisional or full authentication token.
/// </summary>
public static class SetupTotp
{
    /// <summary>Requests the server to generate a TOTP secret and return a QR code data URI for the authenticator app.</summary>
    [ApiRequest(HttpProtocol.Get, path: "/Authentication/SetupTotp", tag: "Authentication")]
    [Authenticated(AllowProvisional = true)]
    public class Request : IRequest<Response> { }

    /// <summary>Contains the QR code data URI to display to the user for scanning with an authenticator app.</summary>
    public class Response
    {
        /// <summary>A data URI containing the QR code image for the TOTP setup.</summary>
        public string QrCode { get; set; } = null!;
    }
}
