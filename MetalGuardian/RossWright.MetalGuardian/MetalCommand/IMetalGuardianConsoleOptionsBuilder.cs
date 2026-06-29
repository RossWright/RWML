using Microsoft.Extensions.Configuration;

namespace RossWright.MetalGuardian;

/// <summary>
/// Configuration options for registering MetalGuardian in a MetalCommand console application.
/// Extends <see cref="IMetalGuardianClientOptionsBuilder"/> with access to the host's
/// <see cref="IConfiguration"/>.
/// </summary>
public interface IMetalGuardianConsoleOptionsBuilder : IMetalGuardianClientOptionsBuilder
{
    /// <summary>The host application's <see cref="IConfiguration"/> instance.</summary>
    IConfiguration Configuration { get; }
}
