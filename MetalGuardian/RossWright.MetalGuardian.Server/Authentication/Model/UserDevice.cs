namespace RossWright.MetalGuardian;

/// <summary>
/// Entity model for a trusted user device fingerprint.
/// </summary>
/// <typeparam name="TUser">The application user entity type.</typeparam>
public class UserDevice<TUser> : IUserDevice
    where TUser : class, IAuthenticationUser
{
    /// <summary>The device record identifier.</summary>
    public Guid RefreshTokenId { get; set; } = Guid.NewGuid();

    /// <summary>The user identifier that owns this device.</summary>
    public Guid UserId { get; set; }

    /// <summary>The user that owns this device.</summary>
    public TUser User { get; set; } = null!;

    IAuthenticationUser IUserDevice.User => User;

    /// <summary>The stable device fingerprint.</summary>
    public string Fingerprint { get; set; } = null!;

    /// <summary>The time this device trust expires, or <c>null</c> when it does not expire.</summary>
    public DateTime? ExpiresOn { get; set; }

    /// <summary>The most recent time this device was seen.</summary>
    public DateTime LastSeen { get; set; }
}
