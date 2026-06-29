using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using RossWright.MetalChain;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using RossWright.MetalNexus.Server;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RossWright.MetalNexus;

/// <summary>
/// Extension methods for registering and activating the MetalNexus server middleware.
/// </summary>
public static class MetalNexusServerExtensions
{
    /// <summary>
    /// Registers the MetalNexus server services with the DI container, including endpoint
    /// schema discovery, middleware options, and ambient request/response context services.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to register services into.</param>
    /// <param name="setOptions">A delegate that configures server options such as assembly scanning and upload limits.</param>
    /// <returns>The same <see cref="WebApplicationBuilder"/> for chaining.</returns>
    public static WebApplicationBuilder AddMetalNexusServer(this WebApplicationBuilder builder,
        Action<IMetalNexusServerOptionsBuilder> setOptions)
    {
        var optionsBuilder = new MetalNexusServerOptionsBuilder();
        setOptions(optionsBuilder);
        optionsBuilder.InitializeServer(builder.Services, builder.Configuration);

        // Transient factory that returns the ambient per-request context set by the middleware.
        // Uses AsyncLocal<T> so each concurrent request gets its own isolated instance,
        // identical to how IHttpContextAccessor works internally.
        builder.Services.AddTransient<IMetalNexusResponseContext>(_ =>
            MetalNexusResponseContext.Current
            ?? throw new InvalidOperationException(
                "IMetalNexusResponseContext resolved outside a MetalNexus request context."));

        builder.Services.AddTransient<IMetalNexusRequestContext>(_ =>
            MetalNexusRequestContext.Current
            ?? throw new InvalidOperationException(
                "IMetalNexusRequestContext resolved outside a MetalNexus request context."));

        return builder;
    }

    /// <summary>
    /// Configures Swashbuckle / OpenAPI to generate accurate documentation for MetalNexus endpoints,
    /// including bearer-token security definitions and MetalNexus-specific schema IDs.
    /// </summary>
    /// <param name="options">The <see cref="SwaggerGenOptions"/> to configure.</param>
    public static void UseMetalNexus(this SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("MetalGuardian", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your Access Token.",
        });
        options.DocumentFilter<MetalNexusApiDocumentFilter>();
        options.SchemaGeneratorOptions = new SchemaGeneratorOptions
        {
            SchemaIdSelector = _ => _.FullName!.Replace('+', '.')
        };
    }

    /// <summary>
    /// Activates the MetalNexus middleware pipeline, routing inbound HTTP requests to the
    /// appropriate MetalChain handler based on the registered endpoint schema.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Call this method on the <see cref="IApplicationBuilder"/> after all other middleware
    /// (e.g. CORS, routing) but before any terminal middleware.  The method automatically
    /// calls <c>UseAuthentication</c> when ASP.NET Core authentication services are registered.
    /// </para>
    /// <para>
    /// This method seals the <see cref="IMetalNexusRegistry"/>, preventing further endpoint
    /// registrations after the application has started.
    /// </para>
    /// </remarks>
    /// <param name="app">The application builder to add the MetalNexus middleware to.</param>
    /// <exception cref="MetalNexusException">
    /// Thrown when <c>AddMetalNexusServer</c> was not called before this method.
    /// </exception>
    public static void UseMetalNexusServer(this IApplicationBuilder app)
    {
        var metalChainMediator = app.ApplicationServices.GetService<IMediator>();
        var metalNexusRegistry = app.ApplicationServices.GetService<IMetalNexusRegistry>();
        var options = app.ApplicationServices.GetService<IMetalNexusOptions>();
        if (metalChainMediator == null || metalNexusRegistry == null || options == null) throw new MetalNexusException(
            "MetalNexus not added to service collections. Call AddMetalNexusServer prior to UseMetalNexusServer.");

        var isService = app.ApplicationServices.GetService<IServiceProviderIsService>();
        var authServiceInstalled = isService?.IsService(typeof(IAuthenticationService)) ?? false;
        if (authServiceInstalled) app.UseAuthentication();

        MetalNexusMiddleware middleware = new(
            options, authServiceInstalled, metalNexusRegistry.Endpoints
            .Where(_ => metalChainMediator.HasHandlerFor(_.RequestType)));
        metalNexusRegistry.Seal();
        app.Use(middleware.Handle);
    }
}