using Microsoft.EntityFrameworkCore;

namespace RossWright.MetalGuardian;

public interface IMetalGuardianDbContext<TUser, TRefreshToken>
    where TUser : class, IAuthenticationUser
    where TRefreshToken : class, IRefreshToken,  new()
{
    DbSet<TUser> Users { get; }
    DbSet<TRefreshToken> RefreshTokens { get; }
}

public interface IMetalGuardianDbContext<TUser, TRefreshToken, TUserDevice>
    : IMetalGuardianDbContext<TUser, TRefreshToken>
    where TUser : class, IAuthenticationUser
    where TRefreshToken : class, IRefreshToken, new()
    where TUserDevice : class, IUserDevice, new()
{
    DbSet<TUserDevice> UserDevices { get; }
}
