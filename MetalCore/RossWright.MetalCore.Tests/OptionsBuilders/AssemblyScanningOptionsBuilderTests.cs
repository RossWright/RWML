using System.Reflection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace RossWright;

public class AssemblyScanningOptionsBuilderTests
{
    [Fact]
    public void ScanAssembly_AddsAssemblyToList()
    {
        var builder = new AssemblyScanningOptionsBuilder("test");
        var asm = Assembly.GetExecutingAssembly();
        builder.ScanAssembly(asm);
        builder.Assemblies.ShouldContain(asm);
    }

    [Fact]
    public void ScanAssemblies_AddsAllAssemblies()
    {
        var builder = new AssemblyScanningOptionsBuilder("test");
        var asm1 = Assembly.GetExecutingAssembly();
        var asm2 = typeof(string).Assembly;
        builder.ScanAssemblies(new[] { asm1, asm2 });
        builder.Assemblies.ShouldContain(asm1);
        builder.Assemblies.ShouldContain(asm2);
    }

    [Fact]
    public void ScanAssemblyContaining_Generic_AddsAssemblyOfType()
    {
        var builder = new AssemblyScanningOptionsBuilder("test");
        builder.ScanAssemblyContaining<string>();
        builder.Assemblies.ShouldContain(typeof(string).Assembly);
    }

    [Fact]
    public void ScanAssemblyContaining_TypeParams_AddsAssembly()
    {
        var builder = new AssemblyScanningOptionsBuilder("test");
        builder.ScanAssemblyContaining(typeof(string));
        builder.Assemblies.ShouldContain(typeof(string).Assembly);
    }

    [Fact]
    public void ScanThisAssembly_AddsCallerAssembly()
    {
        var builder = new AssemblyScanningOptionsBuilder("test");
        builder.ScanThisAssembly();
        builder.Assemblies.ShouldNotBeEmpty();
    }

    [Fact]
    public void ScanAssembliesStartingWith_FiltersLoadedAssembliesByPrefix()
    {
        // Ensure System.* assemblies are loaded
        _ = typeof(System.Text.StringBuilder).Assembly;
        var builder = new AssemblyScanningOptionsBuilder("test");
        builder.ScanAssembliesStartingWith("System");
        builder.Assemblies.ShouldNotBeEmpty();
        builder.Assemblies.ShouldAllBe(asm => asm.FullName!.StartsWith("System"));
    }

    [Fact]
    public void ScanAllAssemblies_DiscoversKnownType()
    {
        var builder = new AssemblyScanningOptionsBuilder("test");
        builder.ScanAllAssemblies();
        builder.DiscoveredConcreteTypes.ShouldContain(t => t == typeof(AssemblyScanningOptionsBuilder));
    }

    [Fact]
    public void ScanAllAssembliesViaFileSystem_DiscoversKnownType()
    {
        var builder = new AssemblyScanningOptionsBuilder("test");
        builder.ScanAllAssembliesViaFileSystem();
        builder.DiscoveredConcreteTypes.ShouldContain(t => t == typeof(AssemblyScanningOptionsBuilder));
    }

    [Fact]
    public void ScanAllAssembliesViaReference_DiscoversKnownType()
    {
        var builder = new AssemblyScanningOptionsBuilder("test");
        builder.ScanAllAssembliesViaReference();
        builder.DiscoveredConcreteTypes.ShouldContain(t => t == typeof(AssemblyScanningOptionsBuilder));
    }

    [Fact]
    public void ScanAssembliesInFolderStartingWith_DiscoversKnownType()
    {
        var builder = new AssemblyScanningOptionsBuilder("test");
        builder.ScanAssembliesInFolderStartingWith("RossWright");
        builder.DiscoveredConcreteTypes.ShouldContain(t => t == typeof(AssemblyScanningOptionsBuilder));
    }

    [Fact]
    public void ScanReferencedAssembliesStartingWith_DiscoversKnownType()
    {
        var builder = new AssemblyScanningOptionsBuilder("test");
        builder.ScanReferencedAssembliesStartingWith("RossWright");
        builder.DiscoveredConcreteTypes.ShouldContain(t => t == typeof(AssemblyScanningOptionsBuilder));
    }

    [Fact]
    public void DiscoveredConcreteTypes_IsBuiltOnFirstAccessAndCached()
    {
        var builder = new AssemblyScanningOptionsBuilder("test");
        builder.ScanAssembly(Assembly.GetExecutingAssembly());

        var first = builder.DiscoveredConcreteTypes;
        var second = builder.DiscoveredConcreteTypes;

        first.ShouldBeSameAs(second);
    }

    [Fact]
    public void DiscoveredConcreteTypes_IsInvalidatedWhenNewAssemblyAdded()
    {
        var builder = new AssemblyScanningOptionsBuilder("test");
        builder.ScanAssembly(Assembly.GetExecutingAssembly());

        var before = builder.DiscoveredConcreteTypes;

        // Adding a new assembly should reset the cache
        builder.ScanAssembly(typeof(string).Assembly);
        var after = builder.DiscoveredConcreteTypes;

        after.ShouldNotBeSameAs(before);
    }

    [Fact]
    public void DiscoveredConcreteTypes_HandlesReflectionTypeLoadException()
    {
        var builder = new AssemblyScanningOptionsBuilder("test");
        var mockAssembly = Substitute.For<Assembly>();
        var loaderException = new ReflectionTypeLoadException(
            new Type?[] { typeof(AssemblyScanningOptionsBuilder), null, typeof(string) },
            new Exception?[] { null, new Exception("Load failed"), null });
        mockAssembly.GetTypes().Returns(_ => throw loaderException);

        builder.ScanAssembly(mockAssembly);
        var types = builder.DiscoveredConcreteTypes;

        types.ShouldContain(typeof(AssemblyScanningOptionsBuilder));
        types.ShouldContain(typeof(string));
    }

    [Fact]
    public void DiscoveredConcreteTypes_HandlesGeneralException()
    {
        var builder = new AssemblyScanningOptionsBuilder("test");
        var mockAssembly = Substitute.For<Assembly>();
        var capturedMessages = new List<(LogLevel Level, string Message)>();
        var mockLog = new CapturingLogger(capturedMessages);
        var mockLoggerFactory = Substitute.For<ILoggerFactory>();
        mockLoggerFactory.CreateLogger(Arg.Any<string>()).Returns(mockLog);
        builder.UseBootstrapLogger(mockLoggerFactory);

        mockAssembly.GetTypes().Returns(_ => throw new InvalidOperationException("Assembly cannot be loaded"));
        mockAssembly.FullName.Returns("TestAssembly, Version=1.0.0.0");

        builder.ScanAssembly(mockAssembly);
        var types = builder.DiscoveredConcreteTypes;

        types.ShouldBeEmpty();
        capturedMessages.ShouldContain(e =>
            e.Level == LogLevel.Warning &&
            e.Message.Contains("Could not load types from assembly") &&
            e.Message.Contains("TestAssembly"));
    }

    [Fact]
    public void DiscoveredConcreteTypes_InternalSetter_SetsValue()
    {
        var builder = new AssemblyScanningOptionsBuilder("test");
        var customTypes = new[] { typeof(string), typeof(int) };

        builder.DiscoveredConcreteTypes = customTypes;

        builder.DiscoveredConcreteTypes.ShouldBeSameAs(customTypes);
    }
}

file sealed class CapturingLogger(List<(LogLevel Level, string Message)> entries) : ILogger
{
    public bool IsEnabled(LogLevel logLevel) => true;
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => entries.Add((logLevel, formatter(state, exception)));
}
