using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalGuardian;

public static class ResetTotpMfa
{
    [ApiRequest(path: "/Authentication/ResetTotp", tag: "Authentication")] 
    [Authenticated]
    public class Request : IRequest
    {
        public Guid UserId { get; init; }
    }
}