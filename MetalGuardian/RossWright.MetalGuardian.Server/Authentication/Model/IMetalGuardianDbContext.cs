using Microsoft.EntityFrameworkCore;

namespace RossWright.MetalGuardian;

/// <summary>
/// Marker interface that the host application's <c>DbContext</c> must implement to use
/// MetalGuardian's built-in database authentication. Exposes the required
/// <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/> properties for users and
/// refresh tokens.
/// </summary>
public interface IMetalGuardianDbContext<TUser, TRefreshToken>
    where TUser : class, IAuthenticationUser
    where TRefreshToken : class, IRefreshToken,  new()
{
    /// <summary>The users table.</summary>
    DbSet<TUser> Users { get; }

    /// <summary>The refresh tokens table.</summary>
    DbSet<TRefreshToken> RefreshTokens { get; }
}

/// <summary>
/// Extends <see cref="IMetalGuardianDbContext{TUser,TRefreshToken}"/> with a
/// <c>UserDevices</c> set. Implement this overload when device-trust fingerprinting
/// is enabled via <c>MapDatabaseAuthenticationWithDevices</c>.
/// </summary>
public interface IMetalGuardianDbContext<TUser, TRefreshToken, TUserDevice>
    : IMetalGuardianDbContext<TUser, TRefreshToken>
    where TUser : class, IAuthenticationUser
    where TRefreshToken : class, IRefreshToken, new()
    where TUserDevice : class, IUserDevice, new()
{
    /// <summary>The user devices table.</summary>
    DbSet<TUserDevice> UserDevices { get; }
}
