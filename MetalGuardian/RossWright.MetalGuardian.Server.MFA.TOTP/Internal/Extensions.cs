namespace RossWright.MetalGuardian;

/// <summary>
/// Registration helpers for TOTP multifactor authentication.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Enables TOTP MFA for MetalGuardian server authentication.
    /// </summary>
    /// <typeparam name="TUser">The user entity type that stores TOTP state.</typeparam>
    /// <param name="guardianBuilder">The MetalGuardian server options builder.</param>
    /// <param name="totpBuilder">Configures TOTP options.</param>
    public static void UseTotpMfa<TUser>(
        this IMetalGuardianServerOptionBuilder guardianBuilder,
        Action<IMetalGuardianServerTotpMfaOptionsBuilder> totpBuilder)
        where TUser : class, ITotpMfaAuthenticationUser
    {
        MetalGuardianTotpMfaOptionsBuilder<TUser> builder = new();
        totpBuilder(builder);
        ((IOptionsBuilder)guardianBuilder).AddServices(_ =>
            builder.Initialize(guardianBuilder, _));
    }
}
