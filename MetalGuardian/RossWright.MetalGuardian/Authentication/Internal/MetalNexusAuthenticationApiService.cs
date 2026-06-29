using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalGuardian.Authentication;

internal class MetalNexusAuthenticationApiService(
    IMediator _mediator,
    IDeviceFingerprintService? _deviceFingerprintSvc = null) 
    : IAuthenticationApiService
{
    public async Task<AuthenticationTokens?> Login(string userIdentity, string password,
        string connectionName, CancellationToken cancellationToken = default)
    {
        try
        {
            var deviceFingerprint = _deviceFingerprintSvc == null ? null
                : await _deviceFingerprintSvc.GetFingerprint();
            return await _mediator.SendVia<AuthenticationTokens?>(
                    connectionName,
                    new Login.Request
                    {
                        UserIdentity = userIdentity,
                        Password = password,
                        DeviceFingerprint = deviceFingerprint,
                    }, cancellationToken);
        }
        catch (NotAuthenticatedException)
        {
            return null;
        }
    }

    public Task Logout(AuthenticationTokens tokens, string connectionName,
        CancellationToken cancellationToken = default) =>
        _mediator.SendVia(
            connectionName, 
            new Logout.Request
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
            }, cancellationToken);

    public async Task<AuthenticationTokens?> Refresh(AuthenticationTokens tokens,
        string connectionName, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _mediator.SendVia(
                connectionName,
                new Refresh.Request
                {
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                }, cancellationToken);
        }
        catch (NotAuthenticatedException)
        {
            return null;
        }
    }
}