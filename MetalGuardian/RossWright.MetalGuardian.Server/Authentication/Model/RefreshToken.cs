namespace RossWright.MetalGuardian;

public class RefreshToken<TUser> : IRefreshToken
    where TUser : class, IAuthenticationUser
{
    public Guid RefreshTokenId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public TUser User { get; set; } = null!;
    IAuthenticationUser IRefreshToken.User => User;

    public string Token { get; set; } = null!;
    public DateTime LastSeen { get; set; }
    public DateTime ExpiresOn { get; set; }
}
