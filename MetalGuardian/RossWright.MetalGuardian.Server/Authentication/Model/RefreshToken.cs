namespace RossWright.MetalGuardian;

/// <summary>
/// Entity model for persisted refresh tokens.
/// </summary>
/// <typeparam name="TUser">The application user entity type.</typeparam>
public class RefreshToken<TUser> : IRefreshToken
    where TUser : class, IAuthenticationUser
{
    /// <summary>The refresh token record identifier.</summary>
    public Guid RefreshTokenId { get; set; } = Guid.NewGuid();

    /// <summary>The user identifier that owns this token.</summary>
    public Guid UserId { get; set; }

    /// <summary>The user that owns this token.</summary>
    public TUser User { get; set; } = null!;

    IAuthenticationUser IRefreshToken.User => User;

    /// <summary>The refresh token value.</summary>
    public string Token { get; set; } = null!;

    /// <summary>The most recent time this refresh token was used.</summary>
    public DateTime LastSeen { get; set; }

    /// <summary>The time this refresh token expires.</summary>
    public DateTime ExpiresOn { get; set; }
}
