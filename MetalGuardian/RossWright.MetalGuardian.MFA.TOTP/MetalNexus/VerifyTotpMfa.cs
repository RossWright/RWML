using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalGuardian;

public static class VerifyTotpMfa
{
    [ApiRequest(HttpProtocol.PostViaBody, path: "/Authentication/VerifyTotp", tag: "Authentication")]
    [Authenticated(AllowProvisional = true)]
    public class Request : IRequest<AuthenticationTokens?>
    {
        public string Code { get; set; } = null!;
        public string? DeviceFingerprint { get; set; }
    }
}
