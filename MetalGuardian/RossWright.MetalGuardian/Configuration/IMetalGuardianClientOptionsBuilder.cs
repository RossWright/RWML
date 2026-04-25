namespace RossWright.MetalGuardian;

public interface IMetalGuardianClientOptionsBuilder 
    : IMetalGuardianOptionsBuilder
{
    void UseAuthenticationApiService<TAuthenticationApiService>()
        where TAuthenticationApiService : class, IAuthenticationApiService;

    void AddAuthenticatedHttpClient(string baseAddress, string? connectionName = null, bool isDefault = false);

    void UseDeviceFingerprinting<TDeviceFingerprintService>()
        where TDeviceFingerprintService : IDeviceFingerprintService;
}
