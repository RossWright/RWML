using System.Diagnostics.CodeAnalysis;

namespace RossWright.MetalGuardian.Authentication;

internal interface IAccessTokenRepository
{
    bool TryGet(string connectionName, [NotNullWhen(true)] out AuthenticationTokens? tokens);
    void Set(string connectionName, AuthenticationTokens tokens);
    void Remove(string connectionName);
    bool Contains(string v);
}

internal class AccessTokenRepository : IAccessTokenRepository
{
    private Dictionary<string, AuthenticationTokens> _accessTokens = new();
    public bool Contains(string connectionName) => _accessTokens.ContainsKey(connectionName);
    public void Remove(string connectionName) => _accessTokens.Remove(connectionName);
    public void Set(string connectionName, AuthenticationTokens tokens) => _accessTokens[connectionName] = tokens;
    public bool TryGet(string connectionName, [NotNullWhen(true)] out AuthenticationTokens? tokens) => _accessTokens.TryGetValue(connectionName, out tokens);
}
