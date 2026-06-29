using Microsoft.Extensions.Configuration;
using RossWright.MetalCommand;

namespace RossWright.MetalGuardian;

internal class MetalGuardianConsoleOptionsBuilder :
    MetalGuardianClientOptionsBuilder,
    IMetalGuardianConsoleOptionsBuilder
{
    public MetalGuardianConsoleOptionsBuilder(IConsoleApplicationBuilder builder) =>
        Configuration = builder.Configuration;
    public IConfiguration Configuration { get; }
}