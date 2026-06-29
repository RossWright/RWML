namespace RossWright.MetalGuardian;

/// <summary>
/// Provides the base address of the authentication server for one or more named connections.
/// Register an implementation to configure the server URL(s) that MetalGuardian connects to.
/// </summary>
public interface IBaseAddressRepository
{
    /// <summary>The name of the default connection used when no explicit connection name is specified.</summary>
    string DefaultConnectionName { get; }

    /// <summary>Returns the base address for the specified connection, or the default connection if <paramref name="connectionName"/> is <c>null</c>.</summary>
    public string GetBaseAddress(string? connectionName = null);
}
