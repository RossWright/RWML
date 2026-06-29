using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand;
using RossWright.MetalNexus.Internal;

namespace RossWright.MetalNexus;

/// <summary>
/// Extension methods for registering the MetalNexus HTTP client and configuring
/// <see cref="HttpClient"/> connections on <see cref="IServiceCollection"/> and
/// <see cref="IConsoleApplicationBuilder"/>.
/// </summary>
public static class MetalNexusExtensions
{
    /// <summary>
    /// Registers the MetalNexus client with the DI container, including all MetalChain request
    /// handlers that dispatch API requests over HTTP.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="optionsBuilder">A delegate that configures the client options, such as assembly scanning and connection settings.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddMetalNexusClient(
        this IServiceCollection services,
        Action<IMetalNexusClientOptionsBuilder> optionsBuilder)
    {
        var options = new MetalNexusClientOptionsBuilder();
        optionsBuilder(options);
        options.InitializeClient(services);
        return services;
    }

    /// <summary>
    /// Registers a named <see cref="HttpClient"/> with the DI container.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="connectionName">The name used to identify this connection in <see cref="ApiRequestAttribute.ConnectionName"/> or <see cref="IMetalNexusClientOptionsBuilder.SetDefaultConnection"/>.</param>
    /// <param name="configureClient">A delegate that configures the <see cref="HttpClient"/> instance (e.g. sets <c>BaseAddress</c>).</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> for further configuration such as adding delegating handlers.</returns>
    public static IHttpClientBuilder AddHttpClient(
        this IServiceCollection services,
        string connectionName, 
        Action<HttpClient> configureClient) =>
        HttpClientFactoryServiceCollectionExtensions
            .AddHttpClient(services, connectionName, configureClient);

    /// <summary>
    /// Registers the default <see cref="HttpClient"/> connection with the DI container.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configureClient">A delegate that configures the <see cref="HttpClient"/> instance (e.g. sets <c>BaseAddress</c>).</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> for further configuration such as adding delegating handlers.</returns>
    public static IHttpClientBuilder AddHttpClient(
        this IServiceCollection services, 
        Action<HttpClient> configureClient) =>
        HttpClientFactoryServiceCollectionExtensions
            .AddHttpClient(services, Microsoft.Extensions.Options.Options.DefaultName, configureClient);


    /// <summary>
    /// Registers the MetalNexus client with a <see cref="IConsoleApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The console application builder.</param>
    /// <param name="buildOptions">A delegate that configures the client options.</param>
    /// <returns>The same <see cref="IConsoleApplicationBuilder"/> for chaining.</returns>
    public static IConsoleApplicationBuilder AddMetalNexusClient(
        this IConsoleApplicationBuilder builder,
        Action<IMetalNexusClientOptionsBuilder> buildOptions)
    {
        builder.Services.AddMetalNexusClient(buildOptions);
        return builder;
    }

    /// <summary>
    /// Registers the default <see cref="HttpClient"/> connection on a <see cref="IConsoleApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The console application builder.</param>
    /// <param name="configureClient">A delegate that configures the <see cref="HttpClient"/> instance.</param>
    /// <returns>The same <see cref="IConsoleApplicationBuilder"/> for chaining.</returns>
    public static IConsoleApplicationBuilder AddHttpClient(
        this IConsoleApplicationBuilder builder,
        Action<HttpClient> configureClient)
    {
        HttpClientFactoryServiceCollectionExtensions
            .AddHttpClient(builder.Services, Microsoft.Extensions.Options.Options.DefaultName, configureClient);
        return builder;
    }

    /// <summary>
    /// Registers a named <see cref="HttpClient"/> connection on a <see cref="IConsoleApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The console application builder.</param>
    /// <param name="connectionName">The name used to identify this connection.</param>
    /// <param name="configureClient">A delegate that configures the <see cref="HttpClient"/> instance.</param>
    /// <returns>The same <see cref="IConsoleApplicationBuilder"/> for chaining.</returns>
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
