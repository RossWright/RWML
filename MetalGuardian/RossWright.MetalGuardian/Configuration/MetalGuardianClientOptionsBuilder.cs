using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RossWright.MetalGuardian.Authentication;
using RossWright.MetalNexus;
using System.ComponentModel;

namespace RossWright.MetalGuardian;

/// <summary>
/// Base implementation for MetalGuardian client option builders.
/// This type is public for package-to-package infrastructure use; application code should use the public builder interfaces.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class MetalGuardianClientOptionsBuilder 
    : MetalGuardianOptionsBuilder,
    IMetalGuardianClientOptionsBuilder
{
    /// <inheritdoc />
    public void AddAuthenticatedHttpClient(
        string baseAddress, 
        string? connectionName = null, 
        bool isDefault = false) =>
        baseAddressRepository.Add(
            connectionName ?? Microsoft.Extensions.Options.Options.DefaultName,
            baseAddress, 
            isDefault);
    private static BaseAddressRepository baseAddressRepository = new();

    /// <inheritdoc />
    public void UseAuthenticationApiService<TAuthenticationApiService>()
        where TAuthenticationApiService : class, IAuthenticationApiService
    {
        if (_authenticationApiService != null) 
            throw new MetalNexusException("You may only use one Authentication API Service");
        _authenticationApiService = typeof(TAuthenticationApiService);
    }
    private Type? _authenticationApiService;

    /// <inheritdoc />
    public override void UseMetalNexusAuthenticationEndpoints() =>
        UseAuthenticationApiService<MetalNexusAuthenticationApiService>();

    /// <inheritdoc />
    public void UseDeviceFingerprinting<TDeviceFingerprintService>()
        where TDeviceFingerprintService : IDeviceFingerprintService =>
        _deviceFingerprintServiceType = typeof(TDeviceFingerprintService);

    /// <summary>
    /// Registers the built-in <see cref="MachineDeviceFingerprintService"/> for console
    /// and desktop clients. The fingerprint is a SHA-256 hash of stable machine-level
    /// signals (machine name, OS, architecture, processor count).
    /// </summary>
    public void UseMachineDeviceFingerprinting() =>
        UseDeviceFingerprinting<MachineDeviceFingerprintService>();

    private Type? _deviceFingerprintServiceType;

    /// <summary>
    /// Registers all client services represented by the configured options.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    public virtual void InitializeClient(IServiceCollection services)
    {
        foreach (var connection in baseAddressRepository.BaseUrlsByConnectionName)
        {
            var uri = connection.Value != null ? new Uri(connection.Value) : null;
            services.AddHttpClient(connection.Key, options => options.BaseAddress = uri)
                .AddHttpMessageHandler(services => new AuthenticationDelegatingHandler(
                    services.GetRequiredService<IMetalGuardianAuthenticationClient>(), 
                    connection.Key));
        }
        services.TryAddSingleton<IBaseAddressRepository>(baseAddressRepository);
        services.TryAddSingleton<IAccessTokenRepository, AccessTokenRepository>();

        if (_deviceFingerprintServiceType != null)
        {
            services.TryAddTransient(typeof(IDeviceFingerprintService), _deviceFingerprintServiceType);
        }

        if (_authenticationApiService != null)
        {
            services.TryAddScoped(typeof(IAuthenticationApiService), _authenticationApiService);
            if (_authenticationApiService == typeof(MetalNexusAuthenticationApiService))
            {
                services.AddMetalNexusEndpoints(
                    typeof(Login.Request), 
                    typeof(Logout.Request), 
                    typeof(Login.Request));
            }
        }
        services.TryAddScoped<IMetalGuardianAuthenticationClient, MetalGuardianAuthenticationClient>();
        services.TryAddScoped<IMetalGuardianUrlHelper, MetalGuardianUrlHelper>();
        AddServices(services);
    }
}
