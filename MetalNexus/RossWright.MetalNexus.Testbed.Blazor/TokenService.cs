using RossWright.MetalInjection;

namespace RossWright.MetalNexus.Testbed.Blazor;

/// <summary>Stores the current bearer token for the active user session.</summary>
[Singleton(typeof(TokenService))]
public sealed class TokenService
{
    private string? _token;
    private string _username = "unauthenticated";

    public string? Token => _token;
    public string Username => _username;

    public void SetToken(string username, string token)
    {
        _username = username;
        _token = token;
    }

    public void ClearToken()
    {
        _username = "unauthenticated";
        _token = null;
    }
}
