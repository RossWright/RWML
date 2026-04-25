namespace RossWright.MetalGuardian;

public static class Extensions
{
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
