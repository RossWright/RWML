namespace RossWright.MetalGuardian;

/// <summary>
/// Configuration options for registering MetalGuardian in a standard DI host
/// (ASP.NET Core, console, etc.). Extends <see cref="IMetalGuardianOptionsBuilder"/>
/// with client-specific settings.
/// </summary>
public interface IMetalGuardianClientOptionsBuilder 
    : IMetalGuardianOptionsBuilder
{
    /// <summary>
    /// Replaces the default MetalNexus authentication API service with a custom implementation.
    /// Use this when targeting a non-MetalNexus authentication backend.
    /// </summary>
    void UseAuthenticationApiService<TAuthenticationApiService>()
        where TAuthenticationApiService : class, IAuthenticationApiService;

    /// <summary>
    /// Registers an <see cref="System.Net.Http.HttpClient"/> that automatically attaches the
    /// current user's access token to outgoing requests. The client is identified by
    /// <paramref name="connectionName"/>; set <paramref name="isDefault"/> to <c>true</c> to
    /// make it the unnamed default client.
    /// </summary>
    void AddAuthenticatedHttpClient(string baseAddress, string? connectionName = null, bool isDefault = false);

    /// <summary>
    /// Registers a custom <see cref="IDeviceFingerprintService"/> implementation.
    /// Device fingerprinting is used to remember trusted devices and skip MFA on subsequent logins.
    /// For Blazor WASM, use the Blazor-specific overload instead.
    /// </summary>
    void UseDeviceFingerprinting<TDeviceFingerprintService>()
        where TDeviceFingerprintService : IDeviceFingerprintService;
}
