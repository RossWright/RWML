using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalNexus.Internal;

namespace RossWright.MetalNexus;

internal class MetalNexusServerOptionsBuilder :
    MetalNexusOptionsBuilderBase,
    IMetalNexusServerOptionsBuilder
{
    public void SetMultipartBodyLengthLimit(long? limitInBytes) =>
        _multipartBodyLengthLimit = limitInBytes ?? long.MaxValue;
    private long _multipartBodyLengthLimit = long.MaxValue;

    public void InitializeServer(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FormOptions>(opt =>
            opt.MultipartBodyLengthLimit = _multipartBodyLengthLimit);
        Initialize(services, isServer: true);
    }
}
