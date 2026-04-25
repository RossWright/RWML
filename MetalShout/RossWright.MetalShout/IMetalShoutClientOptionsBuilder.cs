using System.Text.Json;

namespace RossWright.MetalShout;

public interface IMetalShoutClientOptionsBuilder : IUsesLoggerOptionsBuilder
{
    void SetHubName(string hubName);
    void SetJsonSerializerOptions(JsonSerializerOptions jsonOptions);
}

