using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalNexus;

namespace RossWright.MetalGuardian;

internal class MetalGuardianTotpMfaOptionsBuilder<TUser>
    : IMetalGuardianServerTotpMfaOptionsBuilder
    where TUser : class, ITotpMfaAuthenticationUser
{
    public void SetIssuer(string issuer) => 
        _issuer = issuer;
    private string _issuer = null!;

    public void SetDeviceRememberDays(int? days) =>
        _deviceRememeberDays = days;
    private int? _deviceRememeberDays;

    public void UseMetalNexusTotpMfaEndpoints() =>
        _usingMetalNexusTotpMfaEndpoints = true;
    private bool _usingMetalNexusTotpMfaEndpoints;

    public void Initialize(
        IMetalGuardianServerOptionBuilder guardianBuilder, 
        IServiceCollection services)
    {
        if (string.IsNullOrWhiteSpace(_issuer))
            throw new MetalGuardianException("TOTP MFA Issuer must be set");

        services.AddScoped<IMetalGuardianTotpMfaService>(_ =>
            new MetalGuardianTotpMfaService(_issuer, _deviceRememeberDays,
                _.GetRequiredService<IAuthenticationRepository>(),
                _.GetRequiredService<IMetalGuardianAuthenticationService>(),
                _.GetService<IUserDeviceRepository>()));
        services.AddScoped<IMultifactorAuthenticationProvider, 
                           MetalGuardianTotpMfaMultifactorAuthenticationProvider>();
        services.AddScoped<IUserClaimsProvider,
                           MetalGuardianTotpMfaUserClaimsProvider>();

        if (_usingMetalNexusTotpMfaEndpoints)
        {
            services.AddMetalNexusEndpoints(
                typeof(SetupTotpRequestHandler),
                typeof(VerifyTotpMfaRequestHandler),
                typeof(ResetTotpMfaRequestHandler));
        }

    }
}
