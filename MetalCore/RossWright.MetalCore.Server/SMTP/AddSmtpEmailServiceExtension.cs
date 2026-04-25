using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RossWright.Messaging.Smtp;

/// <summary>
/// Extension methods for registering the SMTP <see cref="IEmailService"/> on a <see cref="WebApplicationBuilder"/>.
/// </summary>
public static class AddSmtpEmailServiceExtension
{
    /// <summary>
    /// Registers <see cref="IEmailService"/> as a singleton SMTP service, binding configuration from
    /// <paramref name="configSection"/> in <c>appsettings.json</c>.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="configSection">The configuration section name to bind. Defaults to <c>"MetalCore.Smtp"</c>.</param>
    /// <returns>The same <paramref name="builder"/> for fluent chaining.</returns>
    public static WebApplicationBuilder AddSmtpEmailService(this WebApplicationBuilder builder,
        string configSection = "MetalCore.Smtp")
    {
        var config = new SmtpConfig();
        builder.Configuration.Bind(configSection, config);
        builder.Services.AddSingleton<IEmailService>(_ => new SmtpEmailService(config));
        return builder;
    }

    /// <summary>
    /// Registers <see cref="IEmailService"/> as a singleton SMTP service from a pre-built <see cref="SmtpConfig"/>.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="config">The SMTP configuration to use.</param>
    /// <returns>The same <paramref name="builder"/> for fluent chaining.</returns>
    public static WebApplicationBuilder AddSmtpEmailService(this WebApplicationBuilder builder, 
        SmtpConfig config)
    {
        builder.Services.AddSingleton<IEmailService>(_ => new SmtpEmailService(config));
        return builder;
    }

    /// <summary>
    /// Registers <see cref="IEmailService"/> as a singleton SMTP service, binding configuration from
    /// <paramref name="configSection"/> and then applying a post-bind delegate for overrides.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="configBuilder">A delegate applied after the configuration section is bound, allowing property overrides.</param>
    /// <param name="configSection">The configuration section name to bind. Defaults to <c>"MetalCore.Smtp"</c>.</param>
    /// <returns>The same <paramref name="builder"/> for fluent chaining.</returns>
    public static WebApplicationBuilder AddSmtpEmailService(this WebApplicationBuilder builder,
        Action<SmtpConfig> configBuilder, string configSection = "MetalCore.Smtp")
    {
        var config = new SmtpConfig();
        builder.Configuration.Bind(configSection, config);
        configBuilder(config);
        builder.Services.AddSingleton<IEmailService>(_ => new SmtpEmailService(config));
        return builder;
    }
}
