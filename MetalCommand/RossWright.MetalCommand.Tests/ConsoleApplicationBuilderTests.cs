using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RossWright.MetalCommand.Internal;
using RossWright.MetalCommand.Internal.Commands;
using RossWright.MetalCommand.Tests.Infrastructure;

namespace RossWright.MetalCommand.Tests;

[Collection("FileSystem")]
public class ConsoleApplicationBuilderTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _originalDir;

    public ConsoleApplicationBuilderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_tempDir);
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDir);
    }

    [Fact]
    public async Task CustomizeBuiltInCommands_ReplacedInvocation_IsReachable()
    {
        var customOptions = new BuiltInCommandOptions
        {
            SaveContextInvocations = ["sc"]
        };

        var (app, console) = CommandFixture.Build(configure: (commands, services) =>
        {
            commands.Add<SaveConCommand>();
            services.AddSingleton<ICommandOptionsRegistry>(new BuiltInCommandOptionsRegistry(customOptions));
        });

        app.Context["env"] = "prod";
        await app.Execute("sc");

        File.Exists(Path.Combine(_tempDir, "default.mcc.json")).ShouldBeTrue();
        console.ErrorLines.ShouldBeEmpty();
    }

    [Fact]
    public async Task CustomizeBuiltInCommands_OriginalInvocation_IsNoLongerReachable()
    {
        var customOptions = new BuiltInCommandOptions
        {
            SaveContextInvocations = ["sc"]
        };

        var (app, console) = CommandFixture.Build(configure: (commands, services) =>
        {
            commands.Add<SaveConCommand>();
            services.AddSingleton<ICommandOptionsRegistry>(new BuiltInCommandOptionsRegistry(customOptions));
        });

        await app.Execute("savectx");

        // The original default invocation must not resolve
        console.ErrorLines.ShouldContain(l => l.Contains("savectx") || l.Contains("No command"));
    }

    [Fact]
    public void Logging_Property_ReturnsLoggingBuilderThatCanRegisterProvider()
    {
        var builder = ConsoleApplication.CreateBuilder();

        builder.Logging.AddConsole();
        var app = builder.Build();

        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        loggerFactory.ShouldNotBeNull();
    }
}
