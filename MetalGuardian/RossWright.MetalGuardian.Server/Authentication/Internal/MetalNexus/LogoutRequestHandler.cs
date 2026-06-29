using RossWright.MetalChain;
using static RossWright.MetalGuardian.Logout;

namespace RossWright.MetalGuardian.MetalNexus;

internal class LogoutRequestHandler : IRequestHandler<Request>
{
    public LogoutRequestHandler(IMetalGuardianAuthenticationService authentSvc) => _authentSvc = authentSvc;
    private readonly IMetalGuardianAuthenticationService _authentSvc;
        
    public Task Handle(Request request, CancellationToken cancellationToken) =>
        _authentSvc.Logout(request, cancellationToken);
}