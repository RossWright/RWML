using RossWright.MetalGuardian.Internal;

namespace RossWright.MetalGuardian.Authentication;

internal class MetalGuardianAuthenticationService(
    IMetalGuardianServerConfiguration _configuration,
    IAccessTokenFactory _accessTokenFactory,
    IAuthenticationRepository _authenticationRepository,
    IEnumerable<IMultifactorAuthenticationProvider> _multifactorAuthenticationServices,
    IEnumerable<IUserClaimsProvider> _userClaimsProviders,
    IUserDeviceRepository? _userDeviceRepository = null)
    : IMetalGuardianAuthenticationService
{
    public async Task<AuthenticationTokens> Login(string userIdentity, string password, string? deviceFingerprint, CancellationToken cancellationToken)
    {
        var user = await _authenticationRepository.LookupUser(userIdentity, cancellationToken);
        if (user == null || !user.IsPassword(password) || user.IsDisabled)
        {
            throw new NotAuthenticatedException();
        }

        bool? isKnownDevice = null;
        if (_userDeviceRepository != null && deviceFingerprint != null)
        { 
            var device = await _userDeviceRepository.Get(user.UserId, deviceFingerprint, cancellationToken);
            isKnownDevice = device != null && (device.ExpiresOn == null || device.ExpiresOn > DateTime.UtcNow);
        }

        var isProvisional = _multifactorAuthenticationServices
            .Any(_ => _.ShouldLoginAsProvisional(user, isKnownDevice));

        return await InnerLogin(user, isProvisional, isKnownDevice, cancellationToken);
    }

    public Task<AuthenticationTokens> Login(IAuthenticationUser user, CancellationToken cancellationToken) => InnerLogin(user, false, null, cancellationToken);

    private async Task<AuthenticationTokens> InnerLogin(IAuthenticationUser user, bool asProvisional, bool? deviceKnown, CancellationToken cancellationToken)
    {
        string? accessToken = null;
        string refreshToken = string.Empty;
        int? accessTokenExpirationOverride = null;

        var claims = await GatherClaims(user, cancellationToken);
        if (deviceKnown != null)
        {
            claims = claims.Append((MetalGuardianClaimTypes.IsKnownDevice, deviceKnown.ToString()!.ToLower()));
        }

        if (asProvisional)
        {
            claims = claims.Append((MetalGuardianClaimTypes.ProvisionalLogin, "true"));
            accessTokenExpirationOverride = _configuration.ProvisionalAccessTokenExpireMins;
        }
        else
        {
            refreshToken = SecurityTools.RandomString();
            await _authenticationRepository.AddRefreshToken(_ =>
            {
                _.UserId = user.UserId;
                _.Token = refreshToken;
                _.ExpiresOn = DateTime.UtcNow.AddMinutes(_configuration.RefreshTokenExpireMins);
                _.LastSeen = DateTime.UtcNow;
            }, cancellationToken);
        }
        
        accessToken = _accessTokenFactory.Create(user, claims, accessTokenExpirationOverride);
        
        return new AuthenticationTokens()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task Logout(AuthenticationTokens tokens, CancellationToken cancellationToken = default)
    {
        if (_accessTokenFactory.Validate(tokens.AccessToken))
        {
            var oldAccessToken = tokens.DecodeAccessToken();
            await _authenticationRepository.DeleteRefreshToken(
                oldAccessToken.UserId, tokens.RefreshToken, cancellationToken);
        }
    }

    public async Task<AuthenticationTokens> Refresh(AuthenticationTokens tokens, CancellationToken cancellationToken = default)
    {
        if (!_accessTokenFactory.Validate(tokens.AccessToken))
        {
            throw new NotAuthenticatedException();
        }
        var userId = tokens.DecodeAccessToken().UserId;

        if (string.IsNullOrWhiteSpace(tokens.RefreshToken)) { throw new NotAuthenticatedException(); }

        bool notAuthenticatedThrown = false;
        string newToken = SecurityTools.RandomString();
        var user = await _authenticationRepository.UpdateRefreshToken(
            userId, tokens.RefreshToken, dbRefreshToken =>
            {
                if (dbRefreshToken.ExpiresOn < DateTime.UtcNow ||
                    dbRefreshToken.User.IsDisabled)
                {
                    notAuthenticatedThrown = true;
                    throw new NotAuthenticatedException();
                }

                dbRefreshToken.Token = newToken;
                dbRefreshToken.ExpiresOn = DateTime.UtcNow
                    .AddMinutes(_configuration.RefreshTokenExpireMins);
                dbRefreshToken.LastSeen = DateTime.UtcNow;
            }, cancellationToken);
        if (user == null || notAuthenticatedThrown)
        {
            throw new NotAuthenticatedException();
        }

        var claims = await GatherClaims(user, cancellationToken);
        return new AuthenticationTokens()
        {
            AccessToken = _accessTokenFactory.Create(user, claims),
            RefreshToken = newToken
        };
    }

    private async Task<IEnumerable<(string, string)>> GatherClaims(IAuthenticationUser user, CancellationToken cancellationToken)
    {
        IEnumerable<(string, string)> claims = [];
        foreach (var userClaimsProvider in _userClaimsProviders)
        {
            var theseClaims = await userClaimsProvider
                .GetClaims(user, cancellationToken);
            if (theseClaims != null) claims = claims.Concat(theseClaims);
        }

        return claims;
    }
}
