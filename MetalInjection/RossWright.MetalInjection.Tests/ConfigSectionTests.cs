using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Reflection;
using Xunit;

namespace RossWright.MetalInjection.Tests;

public class ConfigSectionTests
{
    private static IServiceProvider BuildProvider(
        Dictionary<string, string?> configValues,
        Action<IMetalInjectionOptionsBuilder>? options = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
        return new ServiceCollection()
            .BuildMetalInjectionServiceProvider(options, configuration);
    }

    // ── Tests ────────────────────────────────────────────────────────────────────────────────

    [Fact] public void ConfigSection_BindsAndRegistersByConcrete()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase4Settings)]);

        var provider = BuildProvider(
            new() { ["Phase4:Basic:Value"] = "hello" },
            _ => _.ScanAssemblies(mockAssembly));

        var result = provider.GetService<Phase4Settings>();
        result.ShouldNotBeNull();
        result.Value.ShouldBe("hello");
    }

    [Fact] public void ConfigSection_Generic_RegistersByInterface()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase4InterfaceSettings)]);

        var provider = BuildProvider(
            new() { ["Phase4:Interface:Value"] = "iface" },
            _ => _.ScanAssemblies(mockAssembly));

        provider.GetService<IPhase4Settings>().ShouldNotBeNull();
        provider.GetService<Phase4InterfaceSettings>().ShouldBeNull();
    }

    [Fact] public void ConfigSection_Generic_TypeMismatch_ThrowsAtStartup()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase4BadSettings)]);

        Should.Throw<MetalInjectionException>(() =>
            BuildProvider(new(), _ => _.ScanAssemblies(mockAssembly)));
    }

    [Fact] public void ConfigSection_ValidateOrDie_ExceptionPreventsStartup()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase4ValidatingSettings)]);

        // Value is absent from config so it stays null; ValidateOrDie throws
        Should.Throw<InvalidOperationException>(() =>
            BuildProvider(new(), _ => _.ScanAssemblies(mockAssembly)));
    }

    [Fact] public void ConfigSection_MultipleAttributes_SameInstance()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase4MultiSettings)]);

        var provider = BuildProvider(new(), _ => _.ScanAssemblies(mockAssembly));

        var flags = provider.GetService<IPhase4Flags>();
        var limits = provider.GetService<IPhase4Limits>();
        flags.ShouldNotBeNull();
        limits.ShouldNotBeNull();
        limits.ShouldBeSameAs(flags);
    }

    [Fact] public void ConfigSection_WithoutIConfiguration_IsIgnored()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase4Settings)]);

        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(
                _ => _.ScanAssemblies(mockAssembly),
                configuration: null);

        provider.GetService<Phase4Settings>().ShouldBeNull();
    }

    // ── G-28: Section key absent in config — empty instance is still registered ───────────────

    [Fact] public void ConfigSection_SectionKeyAbsent_EmptyInstanceStillRegistered()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase4Settings)]);

        // Config is empty — "Phase4:Basic" section does not exist; Bind leaves all props at defaults
        var provider = BuildProvider(new(), _ => _.ScanAssemblies(mockAssembly));

        var result = provider.GetService<Phase4Settings>();
        result.ShouldNotBeNull();
        result!.Value.ShouldBeNull();
    }
}

// ── Phase 4 test types ───────────────────────────────────────────────────────────────────────

[ConfigSection("Phase4:Basic")]
public class Phase4Settings
{
    public string? Value { get; set; }
}

public interface IPhase4Settings
{
    string? Value { get; }
}

[ConfigSection<IPhase4Settings>("Phase4:Interface")]
public class Phase4InterfaceSettings : IPhase4Settings
{
    public string? Value { get; set; }
}

// Intentionally does not implement IPhase4Settings — triggers type mismatch error
[ConfigSection<IPhase4Settings>("Phase4:Bad")]
public class Phase4BadSettings { }

[ConfigSection("Phase4:Validate")]
public class Phase4ValidatingSettings : IValidatingConfigSection
{
    public string? Value { get; set; }

    public void ValidateOrDie()
    {
        if (string.IsNullOrWhiteSpace(Value))
            throw new InvalidOperationException("Phase4:Validate Value is required.");
    }
}

public interface IPhase4Flags { }
public interface IPhase4Limits { }

[ConfigSection<IPhase4Flags>("Phase4:Flags")]
[ConfigSection<IPhase4Limits>("Phase4:Limits")]
public class Phase4MultiSettings : IPhase4Flags, IPhase4Limits { }
