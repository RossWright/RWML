using System.Text.Json;

namespace RossWright.MetalNexus;

/// <summary>
/// Configuration builder for the MetalNexus HTTP client, extending the shared
/// <see cref="IMetalNexusOptionsBuilder"/> with client-only settings.
/// </summary>
public interface IMetalNexusClientOptionsBuilder : IMetalNexusOptionsBuilder
{
    /// <summary>
    /// Sets the named <see cref="HttpClient"/> connection used for requests that do not specify
    /// an explicit <see cref="ApiRequestAttribute.ConnectionName"/>.
    /// </summary>
    /// <param name="connectionName">The name of the default <see cref="HttpClient"/> connection.</param>
    void SetDefaultConnection(string connectionName);

    /// <summary>
    /// Overrides the <see cref="JsonSerializerOptions"/> used when serializing request bodies
    /// sent to the server.  The default options use camelCase property naming.
    /// </summary>
    /// <param name="options">The custom serializer options to apply to all outgoing request bodies.</param>
    void SetRequestBodyJsonSerializerOptions(JsonSerializerOptions options);
}