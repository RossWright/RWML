using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Completes MFA confirmation. Accepts provisionally-authenticated tokens.</summary>
[ApiRequest(HttpProtocol.PostViaBody)]
[Authenticated(AllowProvisional = true)]
public class ConfirmMfaRequest : IRequest<MfaConfirmDto>
{
    public string Code { get; set; } = null!;
}

public class MfaConfirmDto
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
