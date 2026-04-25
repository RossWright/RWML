using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace RossWright.Server.Tests;

public class AddSmtpEmailServiceTests
{
    private static SmtpConfig TestConfig => new()
    {
        Host = "smtp.example.com",
        Port = 587,
        FromEmail = "from@example.com",
        FromName = "Sender",
        EnableSsl = true
    };

    // Overload 1: AddSmtpEmailService(WebApplicationBuilder, configSection)
    // Binds from configuration — with no real appsettings, produces an empty SmtpConfig
    [Fact]
    public void AddSmtpEmailService_WithConfigSection_RegistersIEmailService()
    {
        var builder = WebApplication.CreateBuilder();

        builder.AddSmtpEmailService("TestSection");
        var app = builder.Build();

        var service = app.Services.GetService<IEmailService>();
        service.ShouldNotBeNull();
    }

    // Overload 2: AddSmtpEmailService(WebApplicationBuilder, SmtpConfig)
    [Fact]
    public void AddSmtpEmailService_WithExplicitConfig_RegistersIEmailService()
    {
        var builder = WebApplication.CreateBuilder();

        builder.AddSmtpEmailService(TestConfig);
        var app = builder.Build();

        var service = app.Services.GetRequiredService<IEmailService>();
        service.ShouldNotBeNull();
    }

    [Fact]
    public void AddSmtpEmailService_WithExplicitConfig_ReturnsSameBuilder()
    {
        var builder = WebApplication.CreateBuilder();

        var result = builder.AddSmtpEmailService(TestConfig);

        result.ShouldBeSameAs(builder);
    }

    // Overload 3: AddSmtpEmailService(WebApplicationBuilder, Action<SmtpConfig>, configSection)
    [Fact]
    public void AddSmtpEmailService_WithConfigBuilderDelegate_AppliesOverrides()
    {
        var builder = WebApplication.CreateBuilder();
        SmtpConfig? capturedConfig = null;

        builder.AddSmtpEmailService(cfg =>
        {
            capturedConfig = cfg;
            cfg.Host = "override.smtp.com";
        });

        var app = builder.Build();
        var service = app.Services.GetRequiredService<IEmailService>();

        service.ShouldNotBeNull();
        capturedConfig.ShouldNotBeNull();
        capturedConfig!.Host.ShouldBe("override.smtp.com");
    }

    [Fact]
    public void AddSmtpEmailService_IsSingleton()
    {
        var builder = WebApplication.CreateBuilder();
        builder.AddSmtpEmailService(TestConfig);
        var app = builder.Build();

        var s1 = app.Services.GetRequiredService<IEmailService>();
        var s2 = app.Services.GetRequiredService<IEmailService>();

        s1.ShouldBeSameAs(s2);
    }
}
