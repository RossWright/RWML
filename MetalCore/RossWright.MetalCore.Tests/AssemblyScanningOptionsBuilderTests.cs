using System.Reflection;

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
}
