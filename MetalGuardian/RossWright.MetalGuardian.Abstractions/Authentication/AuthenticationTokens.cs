namespace RossWright.MetalGuardian;

/// <summary>
/// Carries both the access token and the refresh token returned by the authentication server.
/// </summary>
public record AuthenticationTokens
{
    /// <summary>The short-lived JWT used to authorize API requests.</summary>
    public string AccessToken { get; init; } = null!;

    /// <summary>The long-lived token used to obtain a new access token without re-entering credentials.</summary>
    public string RefreshToken { get; init; } = null!;
}
