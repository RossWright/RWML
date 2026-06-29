using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalGuardian;

/// <summary>MetalNexus request type for the logout endpoint (<c>POST /Authentication/Logout</c>).</summary>
public static class Logout
{
    /// <summary>Invalidates the current tokens on the server and clears the local authentication state.</summary>
    [ApiRequest(HttpProtocol.PostViaBody, "/Authentication/Logout", "Authentication")]
    [Authenticated(AllowProvisional = true)]
    public record Request : AuthenticationTokens, IRequest { }
}