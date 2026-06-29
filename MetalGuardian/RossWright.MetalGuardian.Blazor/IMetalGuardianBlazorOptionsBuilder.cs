using Microsoft.Extensions.Configuration;

namespace RossWright.MetalGuardian;

/// <summary>
/// Fluent options builder for configuring the MetalGuardian Blazor WASM client.
/// Extends <see cref="IMetalGuardianClientOptionsBuilder"/> with Blazor-specific options.
/// </summary>
public interface IMetalGuardianBlazorOptionsBuilder : IMetalGuardianClientOptionsBuilder
{
    /// <summary>The <see cref="IConfiguration"/> instance provided by the Blazor host.</summary>
    IConfiguration Configuration { get; }

    /// <summary>The base address of the Blazor WASM host environment.</summary>
    string HostBaseAddress { get; }

    /// <summary>Enables browser-based device fingerprinting using browser signals.</summary>
    void UseDeviceFingerprinting();
}
