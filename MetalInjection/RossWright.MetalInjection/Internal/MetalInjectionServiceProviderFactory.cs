using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalInjection;

internal class MetalInjectionServiceProviderFactory
    : IServiceProviderFactory<IServiceCollection>
{
    public MetalInjectionServiceProviderFactory(MetalInjectionOptionsBuilder options) => _options = options;
    private readonly MetalInjectionOptionsBuilder _options;

    public IServiceCollection CreateBuilder(IServiceCollection services) => services;
    public IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection) =>
        new MetalInjectionServiceProvider(serviceCollection, _options);
}
