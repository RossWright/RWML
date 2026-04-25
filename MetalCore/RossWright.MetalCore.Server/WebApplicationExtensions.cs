using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RossWright;

/// <summary>
/// Fluent extension methods for <see cref="WebApplicationBuilder"/> and <see cref="WebApplication"/> that enable
/// a single-expression startup chain from <c>CreateBuilder</c> to <c>Run</c>.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Registers services on the builder's <see cref="IServiceCollection"/> and returns the builder for chaining.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="addServices">A delegate that receives the service collection for registration.</param>
    /// <returns>The same <paramref name="builder"/> for fluent chaining.</returns>
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder,
        Action<IServiceCollection> addServices)
    {
        addServices(builder.Services);
        return builder;
    }

    /// <summary>
    /// Registers services with access to <see cref="IConfiguration"/> and returns the builder for chaining.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="addServices">A delegate that receives the service collection and the application configuration.</param>
    /// <returns>The same <paramref name="builder"/> for fluent chaining.</returns>
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder,
        Action<IServiceCollection, IConfiguration> addServices)
    {
        addServices(builder.Services, builder.Configuration);
        return builder;
    }

    /// <summary>
    /// Configures the middleware pipeline and returns the application for chaining.
    /// </summary>
    /// <param name="app">The built web application.</param>
    /// <param name="useServices">A delegate that configures the application pipeline.</param>
    /// <returns>The same <paramref name="app"/> for fluent chaining.</returns>
    public static WebApplication UseApp(this WebApplication app,
        Action<WebApplication> useServices)
    {
        useServices(app);
        return app;
    }

    /// <summary>
    /// Configures the middleware pipeline with access to <see cref="IConfiguration"/> and returns the application for chaining.
    /// </summary>
    /// <param name="app">The built web application.</param>
    /// <param name="useServices">A delegate that configures the pipeline and receives the application configuration.</param>
    /// <returns>The same <paramref name="app"/> for fluent chaining.</returns>
    public static WebApplication UseApp(this WebApplication app,
        Action<WebApplication, IConfiguration> useServices)
    {
        useServices(app, app.Configuration);
        return app;
    }
}
