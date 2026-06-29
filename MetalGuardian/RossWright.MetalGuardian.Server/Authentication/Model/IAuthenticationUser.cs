namespace RossWright.MetalGuardian;

/// <summary>
/// Represents a user record stored by the host application that MetalGuardian uses
/// for credential verification and token generation. Implement this interface on the
/// host application's user entity.
/// </summary>
public interface IAuthenticationUser
{
    /// <summary>The unique identifier for this user.</summary>
    Guid UserId { get; }

    /// <summary>The display name (username) for this user, embedded in issued tokens.</summary>
    string Name { get; }

    /// <summary>
    /// When <c>true</c>, login attempts for this user are rejected regardless of credentials.
    /// </summary>
    bool IsDisabled { get; }

    /// <summary>The random salt value used when hashing <see cref="PasswordHash"/>.</summary>
    string PasswordSalt { get; set; }

    /// <summary>The salted hash of the user's password.</summary>
    string PasswordHash { get; set; }
}

/// <summary>
/// Extension methods for <see cref="IAuthenticationUser"/> providing password verification
/// and hashing helpers.
/// </summary>
public static class IAuthenticationUserExtensions
{
    /// <summary>
    /// Returns <c>true</c> if <paramref name="password"/> matches the stored hash for
    /// <paramref name="user"/>; <c>false</c> otherwise (including when salt or hash is null).
    /// </summary>
    public static bool IsPassword<T>(this T user, string password)
        where T : IAuthenticationUser =>
        user?.PasswordSalt != null && user.PasswordHash != null &&
        SecurityTools.Hash(password, user.PasswordSalt) == user.PasswordHash;

    /// <summary>
    /// Hashes <paramref name="password"/> with a new random salt and stores the results
    /// in <see cref="IAuthenticationUser.PasswordSalt"/> and
    /// <see cref="IAuthenticationUser.PasswordHash"/>. Returns <paramref name="user"/>
    /// for fluent chaining.
    /// </summary>
    public static T SetPassword<T>(this T user, string password)
        where T : IAuthenticationUser
    {
        (var salt, var hash) = SecurityTools.Hash(password);
        user.PasswordSalt = salt;
        user.PasswordHash = hash;
        return user;
    }
}
