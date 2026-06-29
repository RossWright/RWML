using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Integration.Tests.Infrastructure;

/// <summary>
/// Minimal ASP.NET Core host used as the <c>TEntryPoint</c> for
/// <see cref="MetalNexusTestFactory"/>.  All endpoint types and handlers are
/// discovered from this assembly.
/// </summary>
public class TestApp
{
    internal static WebApplication CreateApplication(WebApplicationBuilder builder,
        Action<IMetalNexusServerOptionsBuilder>? configureServer = null,
        TestAuthOptions? authOptions = null,
        bool nullifyBodySizeFeature = false)
    {
        // Pre-register IMediator with no assembly scan so that AddMetalNexusServer's
        // Initialize() skips its own mass scan and lets AddEndpoints register each
        // handler exactly once (avoiding the "already registered" duplicate error).
        builder.Services.AddMetalChain(_ => { });

        builder.AddMetalNexusServer(server =>
        {
            server.ScanAssemblies(typeof(TestApp).Assembly);
            configureServer?.Invoke(server);
        });

        if (authOptions != null)
        {
            builder.Services.AddSingleton(authOptions);
            builder.Services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        }

        var app = builder.Build();

        if (nullifyBodySizeFeature)
            app.Use((ctx, next) =>
            {
                ctx.Features.Set<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature?>(null);
                return next(ctx);
            });

        app.UseMetalNexusServer();
        return app;
    }
}
