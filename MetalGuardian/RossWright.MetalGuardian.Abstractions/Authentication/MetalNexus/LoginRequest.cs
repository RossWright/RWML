using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalGuardian;

public static class Login
{
    [ApiRequest(HttpProtocol.PostViaBody, path: "/Authentication/Login", tag: "Authentication"), Anonymous]
    public record Request : IRequest<AuthenticationTokens>
    {
        public string UserIdentity { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? DeviceFingerprint { get; set; }
    }
}