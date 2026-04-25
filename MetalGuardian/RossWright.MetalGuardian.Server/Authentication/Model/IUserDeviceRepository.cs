namespace RossWright.MetalGuardian;

public interface IUserDeviceRepository
{
    Task Add(Action<IUserDevice> setProperties, CancellationToken cancellationToken);
    Task<IUserDevice?> Get(Guid userId, string deviceFingerprint, CancellationToken cancellationToken);
    Task Update(Guid userId, string deviceFingerprint, Action<IUserDevice> setProperties, CancellationToken cancellationToken);
}