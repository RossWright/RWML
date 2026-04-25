using Microsoft.Extensions.Configuration;

namespace RossWright.MetalGuardian;

public interface IMetalGuardianBlazorOptionsBuilder : IMetalGuardianClientOptionsBuilder
{
    IConfiguration Configuration { get; }
    string HostBaseAddress { get; }
    void UseDeviceFingerprinting();
}
