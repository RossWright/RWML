using RossWright.MetalChain;
using static RossWright.MetalGuardian.Login;

namespace RossWright.MetalGuardian.MetalNexus;

internal class LoginRequestHandler : IRequestHandler<Request, AuthenticationTokens>
{
    public LoginRequestHandler(IMetalGuardianAuthenticationService authentSvc) => _authentSvc = authentSvc;
    private readonly IMetalGuardianAuthenticationService _authentSvc;

    public Task<AuthenticationTokens> Handle(Request request, CancellationToken cancellationToken) =>
        _authentSvc.Login(request.UserIdentity, request.Password, request.DeviceFingerprint, cancellationToken);
}
