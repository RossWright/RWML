namespace RossWright.MetalGuardian;

public class UserDevice<TUser> : IUserDevice
    where TUser : class, IAuthenticationUser
{
    public Guid RefreshTokenId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public TUser User { get; set; } = null!;
    IAuthenticationUser IUserDevice.User => User;
    public string Fingerprint { get; set; } = null!;
    public DateTime? ExpiresOn { get; set; }
    public DateTime LastSeen { get; set; }
}