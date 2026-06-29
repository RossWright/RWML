using RossWright.MetalChain;
using static RossWright.MetalGuardian.SetupTotp;

namespace RossWright.MetalGuardian;

internal class SetupTotpRequestHandler(
    ICurrentUser _user,
    IMetalGuardianTotpMfaService _totpService) 
    : IRequestHandler<Request, Response>
{
    public async Task<Response> Handle(Request request, CancellationToken cancellationToken) =>
        new Response
        { 
            QrCode = await _totpService.GetSetupQrCode(_user.UserId, cancellationToken)
        };
}