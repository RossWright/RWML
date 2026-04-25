using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand;
using RossWright.MetalNexus.Internal;

namespace RossWright.MetalNexus;

public static class MetalNexusExtensions
{
    public static IServiceCollection AddMetalNexusClient(
        this IServiceCollection services,
        Action<IMetalNexusClientOptionsBuilder> optionsBuilder)
    {
        var options = new MetalNexusClientOptionsBuilder();
        optionsBuilder(options);
        options.InitializeClient(services);
        return services;
    }

    public static IHttpClientBuilder AddHttpClient(
        this IServiceCollection services,
        string connectionName, 
        Action<HttpClient> configureClient) =>
        HttpClientFactoryServiceCollectionExtensions
            .AddHttpClient(services, connectionName, configureClient);

    public static IHttpClientBuilder AddHttpClient(
        this IServiceCollection services, 
        Action<HttpClient> configureClient) =>
        HttpClientFactoryServiceCollectionExtensions
            .AddHttpClient(services, Microsoft.Extensions.Options.Options.DefaultName, configureClient);


    public static IConsoleApplicationBuilder AddMetalNexusClient(
        this IConsoleApplicationBuilder builder,
        Action<IMetalNexusClientOptionsBuilder> buildOptions)
    {
        builder.Services.AddMetalNexusClient(buildOptions);
        return builder;
    }

    public static IConsoleApplicationBuilder AddHttpClient(
        this IConsoleApplicationBuilder builder,
        Action<HttpClient> configureClient)
    {
        HttpClientFactoryServiceCollectionExtensions
            .AddHttpClient(builder.Services, Microsoft.Extensions.Options.Options.DefaultName, configureClient);
        return builder;
    }

    public static IConsoleApplicationBuilder AddHttpClient(
        this IConsoleApplicationBuilder builder,
        string connectionName, 
        Action<HttpClient> configureClient)
    {
        HttpClientFactoryServiceCollectionExtensions
            .AddHttpClient(builder.Services, connectionName, configureClient);
        return builder;
    }
}
