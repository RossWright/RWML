using RossWright.MetalGuardian.Authorization;

namespace RossWright.MetalGuardian.Authentication;

internal class MetalGuardianAuthenticationClient : IMetalGuardianAuthenticationClient
{
    public MetalGuardianAuthenticationClient(
        IAccessTokenRepository accessTokenRepository,
        IAuthenticationApiService authenticationApiService,
        IBaseAddressRepository baseAddressRepository,
        IAuthenticationTokenStorage? authenticationTokenRepository = null,
        IAuthenticationAuthorizationConnection? authorizationService = null)
    {
        _accessTokenRepository = accessTokenRepository;
        _authenticationApiService = authenticationApiService;
        _baseAddressRepository = baseAddressRepository;
        _authenticationTokenRepository = authenticationTokenRepository;
        _authorizationService = authorizationService;
    }
    private readonly IAccessTokenRepository _accessTokenRepository;
    private readonly IAuthenticationApiService _authenticationApiService;
    private readonly IBaseAddressRepository _baseAddressRepository;
    private readonly IAuthenticationTokenStorage? _authenticationTokenRepository;
    private readonly IAuthenticationAuthorizationConnection? _authorizationService;

    public Task<IAuthenticationInformation?> Login(string userIdentity, string password,
        string? connectionName = null, CancellationToken cancellationToken = default) =>
        DoLogin(() => _authenticationApiService.Login(
            userIdentity, password, connectionName ?? Microsoft.Extensions.Options.Options.DefaultName), 
            connectionName, cancellationToken);

    public Task<IAuthenticationInformation?> Login(AuthenticationTokens tokens, 
        string? connectionName = null, CancellationToken cancellationToken = default) =>
        DoLogin(() => Task.FromResult<AuthenticationTokens?>(tokens), 
            connectionName, cancellationToken);

    private async Task<IAuthenticationInformation?> DoLogin(Func<Task<AuthenticationTokens?>> getTokens, 
        string? connectionName = null, CancellationToken cancellationToken = default)
    {
        connectionName ??= _baseAddressRepository.DefaultConnectionName;
        var tokens = await getTokens();
        if (tokens == null) return null;
        _accessTokenRepository.Set(connectionName, tokens);
        return await SaveTokens(connectionName, tokens, cancellationToken);
    }

    public async Task<IAuthenticationInformation?> Authenticate(
        string? connectionName = null, bool forceRefesh = false,
        CancellationToken cancellationToken = default)
    {
        connectionName ??= _baseAddressRepository.DefaultConnectionName;

        AuthenticationTokens? tokens = null;
        if (!_accessTokenRepository.TryGet(connectionName, out tokens) &&
            _authenticationTokenRepository != null)
        {
            tokens = await _authenticationTokenRepository.LoadTokens(
                connectionName, cancellationToken);
            if (tokens != null)
            {
                _accessTokenRepository.Set(connectionName, tokens);
            }
        }

        if (tokens == null) return null;
        var accessToken = new AccessToken(tokens.AccessToken);
        if (!forceRefesh && accessToken.ExpiresOn > DateTimeOffset.UtcNow) return accessToken;

        try
        {
            tokens = await _authenticationApiService.Refresh(tokens, connectionName);
        }
        catch (NotAuthenticatedException)
        {
            tokens = null;
        }
        if (tokens != null)
        {
            _accessTokenRepository.Set(connectionName, tokens);
        }
        else
        {
            _accessTokenRepository.Remove(connectionName);
        }
        return await SaveTokens(connectionName, tokens, cancellationToken);
    }

    public async Task Logout(string? connectionName = null,
        CancellationToken cancellationToken = default)
    {
        if (_authorizationService != null &&
            _authorizationService.ConnectionName == connectionName)
        {
            _authorizationService.ClearAuthorizations();
        }

        connectionName ??= _baseAddressRepository.DefaultConnectionName;        

        if (_authenticationTokenRepository != null)
        {
            await _authenticationTokenRepository.ClearTokens(
                connectionName, cancellationToken);
        }

        if (_accessTokenRepository.TryGet(connectionName, out var tokens))
        {
            await _authenticationApiService.Logout(
                tokens, connectionName, cancellationToken);
            _accessTokenRepository.Remove(connectionName);
            if (AuthenticationChanged != null)
            {
                await AuthenticationChanged(connectionName, null, cancellationToken);
            }
        }
    }

    public bool IsAuthenticated(string? connectionName = null) =>
        _accessTokenRepository.Contains(connectionName ?? _baseAddressRepository.DefaultConnectionName);

    public IAuthenticationInformation? GetUser(string? connectionName = null) =>
        _accessTokenRepository.TryGet(connectionName ?? _baseAddressRepository.DefaultConnectionName, out var tokens) 
            ? new AccessToken(tokens.AccessToken) : null;

    public event AuthenticationChangedEventHandler? AuthenticationChanged;

    private async Task<IAuthenticationInformation?> SaveTokens(
        string connectionName,
        AuthenticationTokens? tokens,
        CancellationToken cancellationToken)
    {
        if (_authorizationService != null &&
            _authorizationService.ConnectionName == connectionName)
        {
            await _authorizationService.LoadAuthorizations();
        }
        if (_authenticationTokenRepository != null)
        {
            if (tokens == null)
            {
                await _authenticationTokenRepository.ClearTokens(
                    connectionName, cancellationToken);
            }
            else
            {
                await _authenticationTokenRepository.SaveTokens(
                    connectionName, tokens, cancellationToken);
            }
        }
        var accessToken = tokens != null ? new AccessToken(tokens.AccessToken) : null;
        if (AuthenticationChanged != null)
        {
            await AuthenticationChanged(connectionName, accessToken, cancellationToken);
        }
        return accessToken;
    }
}
