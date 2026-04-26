using RossWright.MetalCommand.Http.Commands;
using RossWright.MetalCommand.Http.Tests.Infrastructure;
using System.Net;
using System.Reflection;

namespace RossWright.MetalCommand.Http.Tests;

public class PingCommandTests
{
    private static HttpClient BuildHttpClient(HttpStatusCode statusCode, string? baseAddress = "http://localhost:5100/")
    {
        var handler = new StaticResponseHandler(statusCode);
        var client = new HttpClient(handler);
        if (baseAddress is not null)
            client.BaseAddress = new Uri(baseAddress);
        return client;
    }

    private static IHttpConnectionResolver BuildResolver(HttpClient client, string clientName = "MetalCommand:local")
    {
        var resolver = Substitute.For<IHttpConnectionResolver>();
        resolver.GetClientName(Arg.Any<string?>(), Arg.Any<string?>()).Returns(clientName);
        resolver.GetClient(Arg.Any<string?>(), Arg.Any<string?>()).Returns(client);
        return resolver;
    }

    [Fact]
    public async Task ExecuteAsync_SuccessResponse_WritesStatusAndLatency()
    {
        var client = BuildHttpClient(HttpStatusCode.OK);
        var resolver = BuildResolver(client);
        var command = new PingCommand(resolver) { Environment = "local" };
        var console = new TestConsole();

        var result = await command.ExecuteAsync(console, CancellationToken.None);

        result.Success.ShouldBeTrue();
        console.Lines.ShouldContain(l => l.Contains("200"));
        console.Lines.ShouldContain(l => l.Contains("Latency"));
        console.ErrorLines.ShouldBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_Non2xxResponse_WritesFailureStatus()
    {
        var client = BuildHttpClient(HttpStatusCode.InternalServerError);
        var resolver = BuildResolver(client);
        var command = new PingCommand(resolver) { Environment = "local" };
        var console = new TestConsole();

        var result = await command.ExecuteAsync(console, CancellationToken.None);

        result.Success.ShouldBeFalse();
        console.Lines.ShouldContain(l => l.Contains("500"));
    }

    [Fact]
    public async Task ExecuteAsync_Timeout_CancelsAndReportsError()
    {
        var handler = new DelayedResponseHandler(TimeSpan.FromSeconds(30));
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5100/") };
        var resolver = BuildResolver(client);
        var command = new PingCommand(resolver) { Environment = "local", TimeoutSeconds = 0 };
        var console = new TestConsole();

        var result = await command.ExecuteAsync(console, CancellationToken.None);

        result.Success.ShouldBeFalse();
        console.ErrorLines.ShouldContain(l => l.Contains("Timed out") || l.Contains("Timeout"));
    }

    [Fact]
    public async Task ExecuteAsync_UnknownEnvironment_WritesErrorAndReturnsFail()
    {
        var resolver = Substitute.For<IHttpConnectionResolver>();
        resolver.When(r => r.GetClientName(Arg.Any<string?>(), Arg.Any<string?>()))
            .Do(_ => throw new InvalidOperationException("Unknown environment 'bad'"));
        var command = new PingCommand(resolver) { Environment = "bad" };
        var console = new TestConsole();

        var result = await command.ExecuteAsync(console, CancellationToken.None);

        result.Success.ShouldBeFalse();
        console.ErrorLines.ShouldContain(l => l.Contains("Unknown environment"));
    }

    [Fact]
    public void PingCommand_HasCommandAttribute()
    {
        var attr = typeof(PingCommand).GetCustomAttribute<CommandAttribute>(inherit: false);

        attr.ShouldNotBeNull();
        attr.Invocations.ShouldContain("ping");
    }

    private sealed class StaticResponseHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(statusCode));
    }

    private sealed class DelayedResponseHandler(TimeSpan delay) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(delay, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
