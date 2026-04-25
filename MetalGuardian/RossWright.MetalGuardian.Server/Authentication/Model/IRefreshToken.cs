namespace RossWright.MetalGuardian;

public interface IRefreshToken
{
    Guid UserId { get; set; }
    IAuthenticationUser User { get; }
    string Token { get; set; }
    DateTime ExpiresOn { get; set; }
    DateTime LastSeen { get; set; }
}
