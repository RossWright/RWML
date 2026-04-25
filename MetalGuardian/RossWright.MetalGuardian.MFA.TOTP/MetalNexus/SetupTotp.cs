using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalGuardian;

public static class SetupTotp
{
    [ApiRequest(HttpProtocol.Get, path: "/Authentication/SetupTotp", tag: "Authentication")]
    [Authenticated(AllowProvisional = true)]
    public class Request : IRequest<Response> { }

    public class Response
    {
        public string QrCode { get; set; } = null!;
    }
}
