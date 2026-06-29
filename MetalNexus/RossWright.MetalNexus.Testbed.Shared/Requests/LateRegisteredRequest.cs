using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>
/// A request registered via AddMetalNexusEndpoints after the main server setup call,
/// demonstrating late endpoint registration.
/// </summary>
[ApiRequest]
[Anonymous]
public class LateRegisteredRequest : IRequest<LateRegisteredResponse> { }

public class LateRegisteredResponse
{
    public string Message { get; init; } = string.Empty;
}
