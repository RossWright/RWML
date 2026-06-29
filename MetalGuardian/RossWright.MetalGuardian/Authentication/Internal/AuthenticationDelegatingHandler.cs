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
    // AsyncLocal isolates the re-entrancy flag per async call chain, preventing
    // concurrent requests on a shared handler instance from suppressing each other's auth.
    private readonly AsyncLocal<bool> _isReentry = new();

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization == null)
        {
            IAuthenticationInformation? accessToken = null;
            if (!_isReentry.Value)
            {
                _isReentry.Value = true;
                accessToken = await _accessTokenProvider.Authenticate(_connectionName, cancellationToken: cancellationToken);
                _isReentry.Value = false;
            }
            if (accessToken != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
            }
        }
        return await base.SendAsync(request, cancellationToken);
    }
}