namespace RossWright.MetalGuardian;

/// <summary>
/// Represents a trusted device record stored by the host application.
/// When a device is trusted, MetalGuardian may skip MFA challenges for that user.
/// Implement this interface on the host application's user-device entity.
/// </summary>
public interface IUserDevice
{
    /// <summary>The unique identifier of the user this device record belongs to.</summary>
    Guid UserId { get; set; }

    /// <summary>The user associated with this device record.</summary>
    IAuthenticationUser User { get; }

    /// <summary>The stable device fingerprint string that identifies this device.</summary>
    string Fingerprint { get; set; }

    /// <summary>
    /// The UTC date and time after which this device trust record expires.
    /// A <c>null</c> value means the device trust never expires.
    /// </summary>
    DateTime? ExpiresOn { get; set; }

    /// <summary>
    /// The UTC date and time this device was last seen authenticating.
    /// </summary>
    DateTime LastSeen { get; set; }
}