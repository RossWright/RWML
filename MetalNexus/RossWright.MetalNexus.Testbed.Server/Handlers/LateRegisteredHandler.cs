using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

/// <summary>
/// Handles a request that was registered after the main AddMetalNexusServer call
/// via services.AddMetalNexusEndpoints(typeof(LateRegisteredRequest)).
///
/// This demonstrates that MetalNexus resolves registration order automatically —
/// you can scatter endpoint registrations across Program.cs, extension methods,
/// or module initializers and they all resolve correctly at startup.
/// </summary>
internal class LateRegisteredHandler : IRequestHandler<LateRegisteredRequest, LateRegisteredResponse>
{
    public Task<LateRegisteredResponse> Handle(LateRegisteredRequest request, CancellationToken cancellationToken)
        => Task.FromResult(new LateRegisteredResponse
        {
            Message = "LateRegisteredRequest was registered via AddMetalNexusEndpoints() " +
                      "AFTER the main AddMetalNexusServer() call — and it still works!"
        });
}
