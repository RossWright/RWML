using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalGuardian;

/// <summary>MetalNexus request type for the token refresh endpoint (<c>POST /Authentication/Refresh</c>).</summary>
public static class Refresh
{
    /// <summary>Exchanges a refresh token for a new set of authentication tokens.</summary>
    [ApiRequest(HttpProtocol.PostViaBody, path: "/Authentication/Refresh", tag: "Authentication"), Anonymous]
    public record Request : AuthenticationTokens, IRequest<AuthenticationTokens> { }
}