using System.Net.Http.Headers;

namespace RossWright.MetalGuardian.Authentication;

internal class AuthenticationDelegatingHandler : DelegatingHandler
{
    public AuthenticationDelegatingHandler(
        IMetalGuardianAuthenticationClient accessTokenProvider, 
        string connectionName)
    {
        _accessTokenProvider = accessTokenProvider;
        _connectionName = connectionName;
    }

    private readonly IMetalGuardianAuthenticationClient _accessTokenProvider;
    private readonly string _connectionName;
    private bool _isReentry = false;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization == null)
        {
            IAuthenticationInformation? accessToken = null;
            if (!_isReentry)
            {
                _isReentry = true;
                accessToken = await _accessTokenProvider.Authenticate(_connectionName, cancellationToken: cancellationToken);
                _isReentry = false;
            }
            if (accessToken != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
            }
        }
        return await base.SendAsync(request, cancellationToken);
    }
}