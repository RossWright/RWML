using RossWright.MetalChain;
using static RossWright.MetalGuardian.ResetTotpMfa;

namespace RossWright.MetalGuardian;

internal class ResetTotpMfaRequestHandler(
    IMetalGuardianTotpMfaService _totpService) 
    : IRequestHandler<Request>
{
    public Task Handle(Request request, CancellationToken cancellationToken) =>
        _totpService.ResetUser(request.UserId, cancellationToken);
}