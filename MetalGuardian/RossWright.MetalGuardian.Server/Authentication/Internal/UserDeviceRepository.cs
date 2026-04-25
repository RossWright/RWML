using Microsoft.EntityFrameworkCore;

namespace RossWright.MetalGuardian.Authentication;

internal class UserDeviceRepository<TDbContext, TUser, TRefreshToken, TUserDevice>(
    TDbContext _dbCtx)
    : IUserDeviceRepository
    where TDbContext : DbContext, IMetalGuardianDbContext<TUser, TRefreshToken, TUserDevice>
    where TUser : class, IAuthenticationUser
    where TRefreshToken : class, IRefreshToken, new()
    where TUserDevice : class, IUserDevice, new()
{
    public async Task Add(Action<IUserDevice> setProperties, CancellationToken cancellationToken)
    {
        var dbUserDevice = new TUserDevice();
        setProperties(dbUserDevice);
        _dbCtx.UserDevices.Add(dbUserDevice);
        await _dbCtx.SaveChangesAsync(cancellationToken);
    }

    public async Task<IUserDevice?> Get(Guid userId, string deviceFingerprint, CancellationToken cancellationToken) =>
        await _dbCtx.UserDevices.AsNoTracking().FirstOrDefaultAsync(_ => _.UserId == userId && _.Fingerprint == deviceFingerprint, cancellationToken);

    public async Task Update(Guid userId, string deviceFingerprint, Action<IUserDevice> setProperties, CancellationToken cancellationToken)
    {
        var device = await _dbCtx.UserDevices.FirstOrDefaultAsync(_ => _.UserId == userId && _.Fingerprint == deviceFingerprint, cancellationToken);
        if (device != null)
        {
            setProperties(device);
            await _dbCtx.SaveChangesAsync();
        }
    }
}
