using RossWright.MetalNexus.Schemna;

namespace RossWright.MetalNexus;

public interface IEndpointSchemaOptions
{
    bool RequiresAuthenticationByDefault { get; set; }
    string ApiPathPrefix { get; set; }
    bool ApiPathToLower { get; set; }
    string[] RequestSuffixesToTrim { get; set; }
    IPathStrategy? PathStrategy { get; set; }
    int MaximumRequestParameters { get; set; }
    HttpProtocol DefaultHttpProtocol { get; }
}