using RossWright.MetalChain;
using static RossWright.MetalGuardian.VerifyTotpMfa;

namespace RossWright.MetalGuardian;

internal class VerifyTotpMfaRequestHandler(
    ICurrentUser _user,
    IMetalGuardianTotpMfaService _totpService) 
    : IRequestHandler<Request, AuthenticationTokens?>
{
    public Task<AuthenticationTokens?> Handle(Request request, CancellationToken cancellationToken) =>
        _totpService.VerifyCode(_user.UserId, request.Code, request.DeviceFingerprint, cancellationToken);
}