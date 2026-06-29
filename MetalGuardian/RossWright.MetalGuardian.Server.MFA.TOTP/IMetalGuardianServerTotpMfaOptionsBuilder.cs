namespace RossWright.MetalGuardian;

/// <summary>
/// Fluent options builder for configuring the server-side TOTP MFA add-on.
/// </summary>
public interface IMetalGuardianServerTotpMfaOptionsBuilder
{
    /// <summary>Set the name that appears in the TOTP App</summary>
    void SetIssuer(string issuer);

    /// <summary>
    /// Sets the number of days a trusted device is remembered before MFA is required again.
    /// Pass <c>null</c> to disable device-remember (MFA required on every login).
    /// </summary>
    void SetDeviceRememberDays(int? days);

    /// <summary>
    /// Registers the MetalNexus TOTP MFA endpoints (SetupTotp, VerifyTotpMfa, ResetTotpMfa)
    /// with the server pipeline.
    /// </summary>
    void UseMetalNexusTotpMfaEndpoints();
}
