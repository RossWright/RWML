using System.Text.Json;

namespace RossWright.MetalNexus;

public interface IMetalNexusClientOptionsBuilder : IMetalNexusOptionsBuilder
{
    void SetDefaultConnection(string connectionName); // client-only setting
    void SetRequestBodyJsonSerializerOptions(JsonSerializerOptions options); // client-only setting
}