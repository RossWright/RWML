using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalGuardian;

public static class Logout
{
    [ApiRequest(HttpProtocol.PostViaBody, "/Authentication/Logout", "Authentication")]
    [Authenticated(AllowProvisional = true)]
    public record Request : AuthenticationTokens, IRequest { }
}