using RossWright.MetalInjection;
using System.Net.Http.Json;

namespace RossWright.MetalNexus.Testbed.Console;

/// <summary>Stores the current bearer token for the console session and provides a login method.</summary>
[Singleton(typeof(TokenService))]
public sealed class TokenService
{
    private string? _token;
    private string _username = "unauthenticated";

    public string? Token => _token;
    public string Username => _username;

    /// <summary>Obtains a token from the server and stores it.</summary>
    public async Task LoginAsync(
        HttpClient http,
        string username,
        string password,
        bool provisional = false,
        CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/auth/token", new
        {
            username,
            password,
            provisional
        }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
        _token = result?.Token ?? throw new InvalidOperationException("Server returned no token.");
        _username = username + (provisional ? "(provisional)" : "");
    }

    public void ClearToken()
    {
        _token = null;
        _username = "unauthenticated";
    }

    private sealed record TokenResponse(string Token);
}
