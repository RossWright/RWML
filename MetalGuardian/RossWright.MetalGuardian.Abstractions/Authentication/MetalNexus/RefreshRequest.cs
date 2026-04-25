using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalGuardian;

public static class Refresh
{
    [ApiRequest(HttpProtocol.PostViaBody, path: "/Authentication/Refresh", tag: "Authentication"), Anonymous]
    public record Request : AuthenticationTokens, IRequest<AuthenticationTokens> { }
}