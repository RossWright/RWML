namespace RossWright.MetalGuardian;

/// <summary>
/// Represents a refresh token record stored by the host application.
/// Implement this interface on the host application's refresh token entity.
/// </summary>
public interface IRefreshToken
{
    /// <summary>The unique identifier of the user this refresh token belongs to.</summary>
    Guid UserId { get; set; }

    /// <summary>The user associated with this refresh token.</summary>
    IAuthenticationUser User { get; }

    /// <summary>The opaque refresh token string issued to the client.</summary>
    string Token { get; set; }

    /// <summary>The UTC date and time after which this refresh token is no longer valid.</summary>
    DateTime ExpiresOn { get; set; }

    /// <summary>
    /// The UTC date and time this refresh token was last used to obtain a new access token.
    /// </summary>
    DateTime LastSeen { get; set; }
}
