using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using RossWright.MetalChain;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schemna;
using RossWright.MetalNexus.Server;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RossWright.MetalNexus;

public static class MetalNexusServerExtensions
{
    public static WebApplicationBuilder AddMetalNexusServer(this WebApplicationBuilder builder,
        Action<IMetalNexusServerOptionsBuilder> setOptions)
    {
        var optionsBuilder = new MetalNexusServerOptionsBuilder();
        setOptions(optionsBuilder);
        optionsBuilder.InitializeServer(builder.Services, builder.Configuration);
        return builder;
    }

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

    public static void UseMetalNexusServer(this IApplicationBuilder app)
    {
        var metalChainMediator = app.ApplicationServices.GetService<IMediator>();
        var metalNexusRegistry = app.ApplicationServices.GetService<IMetalNexusRegistry>();
        var options = app.ApplicationServices.GetService<IMetalNexusOptions>();
        if (metalChainMediator == null || metalNexusRegistry == null || options == null) throw new MetalNexusException(
            "MetalNexus not added to service collections. Call AddMetalNexusServer prior to UseMetalNexusServer.");

        using (var serviceScope = app.ApplicationServices.CreateScope())
        {
            if (serviceScope.ServiceProvider.GetService<IMediator>() == null)
                throw new MetalNexusException("MetalChain not found");

            var authServiceInstalled = serviceScope.ServiceProvider
                .GetService<IAuthenticationService>() != null;
            if (authServiceInstalled) app.UseAuthentication();

            MetalNexusMiddleware middleware = new(
                options, authServiceInstalled, metalNexusRegistry.Endpoints
                .Where(_ => metalChainMediator.HasHandlerFor(_.RequestType)));
            app.Use(middleware.Handle);
        }
    }
}