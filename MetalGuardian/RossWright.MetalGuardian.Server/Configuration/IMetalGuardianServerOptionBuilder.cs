using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace RossWright.MetalGuardian;

/// <summary>
/// Builder interface for configuring MetalGuardian server services within
/// <see cref="MetalGuardianServerExtensions.AddMetalGuardianServer"/>.
/// </summary>
public interface IMetalGuardianServerOptionBuilder 
    : IMetalGuardianOptionsBuilder
{
    /// <summary>
    /// Supplies JWT configuration directly via an <see cref="IMetalGuardianServerConfiguration"/>
    /// instance.
    /// </summary>
    void UseJwtConfiguration(IMetalGuardianServerConfiguration configuration);

    /// <summary>
    /// Binds JWT configuration from the specified <c>appsettings.json</c> section.
    /// The section must be deserializable as <see cref="MetalGuardianServerConfiguration"/>.
    /// </summary>
    void UseJwtConfigurationSection(string sectionName);

    /// <summary>
    /// Registers a custom <see cref="IAuthenticationRepository"/> implementation for
    /// user and refresh-token data access.
    /// </summary>
    void UseAuthenticationRepository<TAuthenticationRepository>()
        where TAuthenticationRepository : class, IAuthenticationRepository;

    /// <summary>
    /// Registers a custom <see cref="IUserDeviceRepository"/> implementation to enable
    /// device-trust fingerprinting. When registered, recognized devices may bypass MFA
    /// challenges.
    /// </summary>
    void UseUserDeviceRepository<TUserDeviceRepository>()
        where TUserDeviceRepository : class, IUserDeviceRepository;

    /// <summary>
    /// Wires MetalGuardian's built-in EF Core repository against the specified
    /// <c>DbContext</c>. <paramref name="userIdentityPredicate"/> maps the login identity
    /// string (email, username, etc.) to a user lookup expression.
    /// </summary>
    void MapDatabaseAuthentication<TDbContext, TUser, TRefreshToken>(
        Func<string, Expression<Func<TUser, bool>>> userIdentityPredicate)
        where TDbContext : DbContext, IMetalGuardianDbContext<TUser, TRefreshToken>
        where TUser : class, IAuthenticationUser
        where TRefreshToken : class, IRefreshToken, new();

    /// <summary>
    /// Wires MetalGuardian's built-in EF Core repository with device-trust support
    /// against the specified <c>DbContext</c>.
    /// </summary>
    void MapDatabaseAuthenticationWithDevices<TDbContext, TUser, TRefreshToken, TUserDevice>(
        Func<string, Expression<Func<TUser, bool>>> userIdentityPredicate)
        where TDbContext : DbContext, IMetalGuardianDbContext<TUser, TRefreshToken, TUserDevice>
        where TUser : class, IAuthenticationUser
        where TRefreshToken : class, IRefreshToken, new()
        where TUserDevice : class, IUserDevice, new();

    /// <summary>
    /// Registers a custom <see cref="IUserClaimsProvider"/> that contributes additional
    /// claims to issued JWTs. Multiple providers may be registered; all are called during
    /// token generation.
    /// </summary>
    void UseUserClaimsProvider<TUserClaimsProvider>()
        where TUserClaimsProvider : class, IUserClaimsProvider;

    /// <summary>
    /// Maps a single claim from a property on the user entity. The claim is injected
    /// into the JWT using <paramref name="claimName"/>; <paramref name="getValue"/> may
    /// return <c>null</c> to omit the claim.
    /// </summary>
    void AddUserClaimMapping<TUser>(string claimName, Func<TUser, string?> getValue)
        where TUser : class, IAuthenticationUser;

    /// <summary>
    /// Maps multiple claim values from an array property on the user entity. Each element
    /// of the array is added as a separate claim entry with the given <paramref name="claimName"/>.
    /// </summary>
    void AddUserClaimsArrayMapping<TUser>(string claimName, Func<TUser, string[]> getValues)
        where TUser : class, IAuthenticationUser;

    /// <summary>
    /// Enables the OTP service (<see cref="IOtpService"/>). Optionally configure OTP
    /// settings via <paramref name="configure"/>. Requires <c>AddDistributedMemoryCache()</c>
    /// (or a distributed cache) to be registered in the service collection.
    /// </summary>
    void UseOneTimePassword(Action<OneTimePasswordOptions>? configure = null);
}

/// <summary>
/// Convenience overloads for <see cref="IMetalGuardianServerOptionBuilder"/> that supply
/// the default <see cref="RefreshToken{TUser}"/> and <see cref="UserDevice{TUser}"/> types.
/// </summary>
public static class IMetalGuardianServerOptionBuilderExtensions
{
    /// <summary>
    /// Wires MetalGuardian's built-in EF Core repository using the default
    /// <see cref="RefreshToken{TUser}"/> type.
    /// </summary>
    public static void MapDatabaseAuthentication<TDbContext, TUser>(
        this IMetalGuardianServerOptionBuilder builder,
        Func<string, Expression<Func<TUser, bool>>> userIdentityPredicate)
        where TDbContext : DbContext, IMetalGuardianDbContext<TUser, RefreshToken<TUser>>
        where TUser : class, IAuthenticationUser =>
        builder.MapDatabaseAuthentication<TDbContext, TUser, RefreshToken<TUser>>(userIdentityPredicate);

    /// <summary>
    /// Wires MetalGuardian's built-in EF Core repository with device-trust support using
    /// the default <see cref="RefreshToken{TUser}"/> type and a custom
    /// <typeparamref name="TUserDevice"/> type.
    /// </summary>
    public static void MapDatabaseAuthenticationWithDevices<TDbContext, TUser, TUserDevice>(
        this IMetalGuardianServerOptionBuilder builder,
        Func<string, Expression<Func<TUser, bool>>> userIdentityPredicate)
        where TDbContext : DbContext, IMetalGuardianDbContext<TUser, RefreshToken<TUser>, TUserDevice>
        where TUser : class, IAuthenticationUser
        where TUserDevice : class, IUserDevice, new() =>
        builder.MapDatabaseAuthentication<TDbContext, TUser, RefreshToken<TUser>>(userIdentityPredicate);

    /// <summary>
    /// Wires MetalGuardian's built-in EF Core repository with device-trust support using
    /// the default <see cref="RefreshToken{TUser}"/> and <see cref="UserDevice{TUser}"/> types.
    /// </summary>
    public static void MapDatabaseAuthenticationWithDevices<TDbContext, TUser>(
        this IMetalGuardianServerOptionBuilder builder,
        Func<string, Expression<Func<TUser, bool>>> userIdentityPredicate)
        where TDbContext : DbContext, IMetalGuardianDbContext<TUser, RefreshToken<TUser>, UserDevice<TUser>>
        where TUser : class, IAuthenticationUser =>
        builder.MapDatabaseAuthentication<TDbContext, TUser, RefreshToken<TUser>>(userIdentityPredicate);
}