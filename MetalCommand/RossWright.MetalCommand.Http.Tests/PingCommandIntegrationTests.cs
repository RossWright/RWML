using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Http.Commands;
using RossWright.MetalCommand.Http.Tests.Infrastructure;
using System.Net;

namespace RossWright.MetalCommand.Http.Tests;

/// <summary>
/// Integration tests for <see cref="PingCommand"/> that exercise the full
/// <see cref="EnvironmentArgMiddleware"/> pipeline, verifying that the benign
/// environment policy never prompts even for protected environments, and that
/// the default environment from <see cref="IHttpConnectionResolver"/> is used
/// when no environment argument is supplied.
/// </summary>
public class PingCommandIntegrationTests
{
    // ── 21.1 — Benign policy / protected environment — never prompts ───────

    /// <summary>
    /// PingCommand uses [EnvironmentArg(EnvironmentPolicy.Benign)].
    /// EnvironmentArgMiddleware must never write a confirmation prompt when the
    /// policy is Benign, even when the target environment is marked as protected.
    /// </summary>
    [Fact]
    public async Task PingCommand_WithBenignPolicy_ProtectedEnvironment_NeverPrompts()
    {
        // Arrange — stub resolver returns a working client so we don't hit a real server.
        var stubResolver = Substitute.For<IHttpConnectionResolver>();
        stubResolver.GetClientName(Arg.Any<string?>(), Arg.Any<string?>()).Returns("MetalCommand:production");
        stubResolver.GetClient(Arg.Any<string?>(), Arg.Any<string?>())
            .Returns(new HttpClient(new StaticOkHandler()) { BaseAddress = new Uri("http://prod.example.com/") });

        var console = new TestConsole();
        var app = BuildAppWithStubResolver(stubResolver, console, protectedEnvironment: "production");

        // Act — execute ping targeting a protected environment
        await app.Execute("ping", "production");

        // Assert — benign policy: no confirmation prompt should be written
        console.Lines.ShouldNotContain(
            l => l.Contains("yes", StringComparison.OrdinalIgnoreCase) &&
                 l.Contains("confirm", StringComparison.OrdinalIgnoreCase));
        console.ErrorLines.ShouldNotContain(l => l.Contains("protected"));
    }

    // ── 21.2 — Default environment used when Environment arg is omitted ────

    /// <summary>
    /// When no environment argument is provided, EnvironmentArgMiddleware resolves the
    /// default environment from the registered IEnvironmentSource and sets it on the
    /// command property. PingCommand must then call GetClientName with that default value —
    /// verifying the resolver is used and not bypassed.
    /// </summary>
    [Fact]
    public async Task PingCommand_DefaultEnvironment_UsedWhenEnvironmentArgOmitted()
    {
        // Arrange — stub resolver records arguments and returns a working client.
        var stubResolver = Substitute.For<IHttpConnectionResolver>();
        stubResolver.GetClientName(Arg.Any<string?>(), Arg.Any<string?>()).Returns("MetalCommand:local");
        stubResolver.GetClient(Arg.Any<string?>(), Arg.Any<string?>())
            .Returns(new HttpClient(new StaticOkHandler()) { BaseAddress = new Uri("http://localhost:5100/") });

        var console = new TestConsole();
        var app = BuildAppWithStubResolver(stubResolver, console, protectedEnvironment: null);

        // Act — invoke ping with no environment argument
        await app.Execute("ping");

        // Assert — EnvironmentArgMiddleware resolves the default ("local") from IEnvironmentSource
        // and sets it on the command; the resolver is then called with that resolved default.
        stubResolver.Received().GetClientName("local", null);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a <see cref="ConsoleApplication"/> with <see cref="PingCommand"/> registered,
    /// <see cref="EnvironmentArgMiddleware"/> in the pipeline, and a stub
    /// <see cref="IHttpConnectionResolver"/> pre-registered in DI so <c>TryAdd</c> in
    /// <see cref="AddHttpConnectionsExtensions"/> will not replace it.
    /// </summary>
    private static ConsoleApplication BuildAppWithStubResolver(
        IHttpConnectionResolver stubResolver,
        TestConsole console,
        string? protectedEnvironment)
    {
        var builder = ConsoleApplication.CreateBuilder();
        builder.Services.AddSingleton<IConsole>(console);

        // Register the stub before AddHttpConnections so TryAdd does not override it.
        builder.Services.AddScoped<IHttpConnectionResolver>(_ => stubResolver);

        // Also expose an IEnvironmentSource so EnvironmentArgMiddleware can check protection.
        if (protectedEnvironment is not null)
        {
            var source = new StubEnvironmentSource(protectedEnvironment);
            builder.Services.AddSingleton<IEnvironmentSource>(source);
        }

        builder.AddHttpConnections(cfg =>
        {
            cfg.AddDefault("local", "http://localhost:5100/");
            if (protectedEnvironment is not null)
                cfg.AddProtected(protectedEnvironment, $"http://{protectedEnvironment}.example.com/");
        });

        builder.Commands.Add<PingCommand>();

        return builder.Build();
    }

    /// <summary>Minimal environment source with one protected environment.</summary>
    private sealed class StubEnvironmentSource(string protectedEnv) : IEnvironmentSource
    {
        public string DefaultEnvironment => "local";
        public EnvironmentEntry[] Environments =>
        [
            new() { Name = "local",        IsProtected = false },
            new() { Name = protectedEnv,   IsProtected = true  },
        ];
    }

    private sealed class StaticOkHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
