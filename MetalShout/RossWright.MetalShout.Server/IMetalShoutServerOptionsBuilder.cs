using System.Text.Json;

namespace RossWright.MetalShout;

public interface IMetalShoutServerOptionsBuilder : IOptionsBuilder
{
    void SetJsonSerializerOptions(JsonSerializerOptions jsonOptions);
}
