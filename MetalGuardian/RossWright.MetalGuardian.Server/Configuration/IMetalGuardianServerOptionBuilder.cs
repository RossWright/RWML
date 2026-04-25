using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace RossWright.MetalGuardian;

public interface IMetalGuardianServerOptionBuilder 
    : IMetalGuardianOptionsBuilder
{
    void UseJwtConfiguration(IMetalGuardianServerConfiguration configuration);

    void UseJwtConfigurationSection(string sectionName);


    void UseAuthenticationRepository<TAuthenticationRepository>()
        where TAuthenticationRepository : class, IAuthenticationRepository;

    void UseUserDeviceRepository<TUserDeviceRepository>()
        where TUserDeviceRepository : class, IUserDeviceRepository;

    void MapDatabaseAuthentication<TDbContext, TUser, TRefreshToken>(
        Func<string, Expression<Func<TUser, bool>>> userIdentityPredicate)
        where TDbContext : DbContext, IMetalGuardianDbContext<TUser, TRefreshToken>
        where TUser : class, IAuthenticationUser
        where TRefreshToken : class, IRefreshToken, new();

    void MapDatabaseAuthenticationWithDevices<TDbContext, TUser, TRefreshToken, TUserDevice>(
        Func<string, Expression<Func<TUser, bool>>> userIdentityPredicate)
        where TDbContext : DbContext, IMetalGuardianDbContext<TUser, TRefreshToken, TUserDevice>
        where TUser : class, IAuthenticationUser
        where TRefreshToken : class, IRefreshToken, new()
        where TUserDevice : class, IUserDevice, new();

    void UseUserClaimsProvider<TUserClaimsProvider>()
        where TUserClaimsProvider : class, IUserClaimsProvider;

    void AddUserClaimMapping<TUser>(string claimName, Func<TUser, string?> getValue)
        where TUser : class, IAuthenticationUser;

    void AddUserClaimsArrayMapping<TUser>(string claimName, Func<TUser, string[]> getValues)
        where TUser : class, IAuthenticationUser;

    void UseOneTimePassword(Action<OneTimePasswordOptions>? configure = null);
}

public static class IMetalGuardianServerOptionBuilderExtensions
{
    public static void MapDatabaseAuthentication<TDbContext, TUser>(
        this IMetalGuardianServerOptionBuilder builder,
        Func<string, Expression<Func<TUser, bool>>> userIdentityPredicate)
        where TDbContext : DbContext, IMetalGuardianDbContext<TUser, RefreshToken<TUser>>
        where TUser : class, IAuthenticationUser =>
        builder.MapDatabaseAuthentication<TDbContext, TUser, RefreshToken<TUser>>(userIdentityPredicate);

    public static void MapDatabaseAuthenticationWithDevices<TDbContext, TUser, TUserDevice>(
        this IMetalGuardianServerOptionBuilder builder,
        Func<string, Expression<Func<TUser, bool>>> userIdentityPredicate)
        where TDbContext : DbContext, IMetalGuardianDbContext<TUser, RefreshToken<TUser>, TUserDevice>
        where TUser : class, IAuthenticationUser
        where TUserDevice : class, IUserDevice, new() =>
        builder.MapDatabaseAuthentication<TDbContext, TUser, RefreshToken<TUser>>(userIdentityPredicate);

    public static void MapDatabaseAuthenticationWithDevices<TDbContext, TUser>(
        this IMetalGuardianServerOptionBuilder builder,
        Func<string, Expression<Func<TUser, bool>>> userIdentityPredicate)
        where TDbContext : DbContext, IMetalGuardianDbContext<TUser, RefreshToken<TUser>, UserDevice<TUser>>
        where TUser : class, IAuthenticationUser =>
        builder.MapDatabaseAuthentication<TDbContext, TUser, RefreshToken<TUser>>(userIdentityPredicate);
}