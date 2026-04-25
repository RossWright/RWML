namespace RossWright.MetalGuardian;

public interface IAuthenticationUser
{
    Guid UserId { get; }
    string Name { get; }
    bool IsDisabled { get; }
    string PasswordSalt { get; set; }
    string PasswordHash { get; set; }
}

public static class IAuthenticationUserExtensions
{
    public static bool IsPassword<T>(this T user, string password)
        where T : IAuthenticationUser =>
        user?.PasswordSalt != null && user.PasswordHash != null &&
        SecurityTools.Hash(password, user.PasswordSalt) == user.PasswordHash;

    public static T SetPassword<T>(this T user, string password)
        where T : IAuthenticationUser
    {
        (var salt, var hash) = SecurityTools.Hash(password);
        user.PasswordSalt = salt;
        user.PasswordHash = hash;
        return user;
    }
}
