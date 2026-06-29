namespace RossWright.MetalGuardian.Authentication;

internal class BlazorAuthenticationTokenRepository : IAuthenticationTokenStorage
{
    public BlazorAuthenticationTokenRepository(IBrowserLocalStorage localStorageService) =>
        _localStorage = localStorageService;
    private readonly IBrowserLocalStorage _localStorage;

    private static string AccessTokenKey(string? connectionName) => 
        string.IsNullOrWhiteSpace(connectionName) 
        ? "accessToken" : $"{connectionName}-accessToken";
    private static string RefreshTokenKey(string? connectionName) => 
        string.IsNullOrWhiteSpace(connectionName) 
        ? "refreshToken" : $"{connectionName}-refreshToken";

    private readonly Dictionary<string, AuthenticationTokens?> _cache = new();

    public async Task<AuthenticationTokens?> LoadTokens(string connectionName,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(connectionName, out var tokens)) return tokens;
        var accessToken = await _localStorage.Get(AccessTokenKey(connectionName));
        var refreshToken = await _localStorage.Get(RefreshTokenKey(connectionName));
        tokens = (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken)) ? null
            : new AuthenticationTokens
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        _cache.Add(connectionName, tokens);
        return tokens;
    }

    public async Task SaveTokens(string connectionName, AuthenticationTokens tokens,
        CancellationToken cancellationToken = default)
    {
        _cache[connectionName] = tokens;
        await _localStorage.Set(AccessTokenKey(connectionName), tokens.AccessToken);
        await _localStorage.Set(RefreshTokenKey(connectionName), tokens.RefreshToken);
    }

    public async Task ClearTokens(string connectionName, CancellationToken cancellationToken = default)
    {
        _cache.Remove(connectionName);
        await _localStorage.Remove(AccessTokenKey(connectionName));
        await _localStorage.Remove(RefreshTokenKey(connectionName));
    }
}