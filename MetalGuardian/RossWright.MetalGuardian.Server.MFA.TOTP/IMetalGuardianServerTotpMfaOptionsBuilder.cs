namespace RossWright.MetalGuardian;

public interface IMetalGuardianServerTotpMfaOptionsBuilder
{
    /// <summary>Set the name that appears in the TOTP App</summary>
    void SetIssuer(string issuer);
    void SetDeviceRememberDays(int? days);
    void UseMetalNexusTotpMfaEndpoints();
}
