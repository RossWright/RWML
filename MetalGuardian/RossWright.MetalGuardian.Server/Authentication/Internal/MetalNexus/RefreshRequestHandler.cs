using RossWright.MetalChain;
using static RossWright.MetalGuardian.Refresh;

namespace RossWright.MetalGuardian.MetalNexus;

internal class RefreshRequestHandler : IRequestHandler<Request, AuthenticationTokens>
{
    public RefreshRequestHandler(IMetalGuardianAuthenticationService authentSvc) => _authentSvc = authentSvc;
    private readonly IMetalGuardianAuthenticationService _authentSvc;

    public Task<AuthenticationTokens> Handle(Request request, CancellationToken cancellationToken) =>
        _authentSvc.Refresh(request, cancellationToken);
}
