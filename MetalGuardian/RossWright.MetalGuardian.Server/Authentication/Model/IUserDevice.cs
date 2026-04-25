namespace RossWright.MetalGuardian;

public interface IUserDevice
{
    Guid UserId { get; set; }
    IAuthenticationUser User { get; }
    string Fingerprint { get; set; }
    DateTime? ExpiresOn { get; set; }
    DateTime LastSeen { get; set; }
}