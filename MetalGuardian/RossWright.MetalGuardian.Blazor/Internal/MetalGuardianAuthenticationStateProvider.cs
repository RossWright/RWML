using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace RossWright.MetalGuardian;

internal sealed class MetalGuardianAuthenticationStateProvider : AuthenticationStateProvider
{
    public MetalGuardianAuthenticationStateProvider(IMetalGuardianAuthenticationClient client, string? connectionName = null)
    {
        _client = client;
        _client.AuthenticationChanged += _client_AuthenticationChanged;
        _connectionName = connectionName;
    }
    private readonly IMetalGuardianAuthenticationClient _client;
    private readonly string? _connectionName;

    private Task _client_AuthenticationChanged(string connectionName,
        IAuthenticationInformation? accessToken, CancellationToken cancellationToken = default)
    {
        NotifyAuthenticationStateChanged(Task.FromResult(MakeAuthenticationState(accessToken)));
        return Task.CompletedTask;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync() =>
        MakeAuthenticationState(await _client.Authenticate(_connectionName));

    private static AuthenticationState MakeAuthenticationState(IAuthenticationInformation? authInfo)
    {
        var authAsClaimsIdentity = authInfo?.AsClaimsIdentity();

        var rolesClaims = authAsClaimsIdentity?.FindAll(ClaimTypes.Role).ToArray();

        if (rolesClaims != null && rolesClaims.Count() > 0)
        {
            for (int i = 0; i < rolesClaims.Count(); i++)
            {
                var claim = rolesClaims[i];
                if (claim.Value.StartsWith("["))
                {
                    authAsClaimsIdentity?.RemoveClaim(claim);
                    var roles = claim.Value.Trim('[', ']').Split(',');
                    foreach (var role in roles)
                    {
                        authAsClaimsIdentity?.AddClaim(new Claim(ClaimTypes.Role, role.Trim('"')));
                    }
                }
            }
        }
        return new AuthenticationState(new ClaimsPrincipal(authAsClaimsIdentity ?? new ClaimsIdentity()));
    }
}
