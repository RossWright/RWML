using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace RossWright.Server.Tests;

public class SmtpEmailServiceTests
{
    private static SmtpConfig ValidConfig => new()
    {
        Host = "127.0.0.1",
        Port = 9999,
        FromEmail = "from@example.com",
        FromName = "Test Sender",
        EnableSsl = false
    };

    private static IEmailService CreateService(SmtpConfig? config = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.AddSmtpEmailService(config ?? ValidConfig);
        var app = builder.Build();
        return app.Services.GetRequiredService<IEmailService>();
    }

    private static IAddressedEmail BuildEmail(
        string? htmlBody = "<b>Hello</b>") =>
        new AddressedEmail(
            [new EmailRecipient("Recipient", "to@example.com")],
            "Test Subject", "Hello", htmlBody);

    [Fact]
    public async Task Send_SmtpConnectionFailure_ThrowsMetalCoreException()
    {
        // SmtpClient connecting to 127.0.0.1:9999 will fail with a socket error,
        // which the service wraps in MetalCoreException
        var service = CreateService();

        await Should.ThrowAsync<MetalCoreException>(
            () => service.Send(BuildEmail()));
    }

    [Fact]
    public async Task Send_NullEmail_CompletesWithoutThrowing()
    {
        var service = CreateService();

        await Should.NotThrowAsync(() => service.Send(null!));
    }

    [Fact]
    public async Task Send_SmtpConnectionFailure_InnerExceptionIsPreserved()
    {
        var service = CreateService();

        var ex = await Should.ThrowAsync<MetalCoreException>(
            () => service.Send(BuildEmail()));

        ex.InnerException.ShouldNotBeNull();
    }

    [Fact]
    public async Task Send_WithAuthCredentials_StillWrapsSmtpFailureInMetalCoreException()
    {
        var config = ValidConfig;
        config.Username = "user";
        config.Password = "pass";
        var service = CreateService(config);

        await Should.ThrowAsync<MetalCoreException>(
            () => service.Send(BuildEmail()));
    }

    [Fact]
    public async Task Send_TextOnlyEmail_ThrowsMetalCoreExceptionOnConnectionFailure()
    {
        // HtmlBody = null means text-only path in the service
        var service = CreateService();

        await Should.ThrowAsync<MetalCoreException>(
            () => service.Send(BuildEmail(htmlBody: null)));
    }

    [Fact]
    public void AddSmtpEmailService_ResolvesIEmailService()
    {
        var service = CreateService();

        service.ShouldNotBeNull();
    }
}
