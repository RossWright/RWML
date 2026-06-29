using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace RossWright.MetalGuardian;

internal sealed class MetalGuardianAuthenticationStateProvider : AuthenticationStateProvider
{
    public MetalGuardianAuthenticationStateProvider(
        IMetalGuardianAuthenticationClient client,
        ILogger<MetalGuardianAuthenticationStateProvider> logger,
        string? connectionName = null)
    {
        _client = client;
        _logger = logger;
        _client.AuthenticationChanged += _client_AuthenticationChanged;
        _connectionName = connectionName;
    }
    private readonly IMetalGuardianAuthenticationClient _client;
    private readonly ILogger<MetalGuardianAuthenticationStateProvider> _logger;
    private readonly string? _connectionName;

    private Task _client_AuthenticationChanged(string connectionName,
        IAuthenticationInformation? accessToken, CancellationToken cancellationToken = default)
    {
        NotifyAuthenticationStateChanged(Task.FromResult(MakeAuthenticationState(accessToken)));
        return Task.CompletedTask;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            return MakeAuthenticationState(await _client.Authenticate(_connectionName));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to determine authentication state; treating as unauthenticated.");
            return MakeAuthenticationState(null);
        }
    }

    private static AuthenticationState MakeAuthenticationState(IAuthenticationInformation? authInfo)
    {
        var authAsClaimsIdentity = authInfo?.AsClaimsIdentity();
        return new AuthenticationState(new ClaimsPrincipal(authAsClaimsIdentity ?? new ClaimsIdentity()));
    }
}
