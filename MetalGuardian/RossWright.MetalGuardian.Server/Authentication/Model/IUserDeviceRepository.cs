namespace RossWright.MetalGuardian;

/// <summary>
/// Repository interface for managing trusted user-device records.
/// Registering an implementation via
/// <see cref="IMetalGuardianServerOptionBuilder.UseUserDeviceRepository{TUserDeviceRepository}"/>
/// or <c>MapDatabaseAuthenticationWithDevices</c> enables device-trust tracking, which
/// allows <see cref="IMultifactorAuthenticationProvider"/> implementations to skip MFA
/// challenges for recognized devices. This interface is optional — if not registered,
/// device fingerprinting is disabled.
/// </summary>
public interface IUserDeviceRepository
{
    /// <summary>
    /// Adds a new device record, populating its properties via <paramref name="setProperties"/>.
    /// </summary>
    Task Add(Action<IUserDevice> setProperties, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the device record for the specified user and fingerprint, or <c>null</c>
    /// if not found.
    /// </summary>
    Task<IUserDevice?> Get(Guid userId, string deviceFingerprint, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the device record for the specified user and fingerprint by applying
    /// <paramref name="setProperties"/>.
    /// </summary>
    Task Update(Guid userId, string deviceFingerprint, Action<IUserDevice> setProperties, CancellationToken cancellationToken);
}