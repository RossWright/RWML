using Microsoft.Extensions.Configuration;

namespace RossWright.MetalGuardian;

public interface IMetalGuardianConsoleOptionsBuilder : IMetalGuardianClientOptionsBuilder
{
    IConfiguration Configuration { get; }
}
