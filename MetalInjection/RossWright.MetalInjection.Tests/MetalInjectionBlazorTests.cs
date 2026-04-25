using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Reflection;
using Xunit;

namespace RossWright.MetalInjection.Tests;

// ── Blazor project coverage ────────────────────────────────────────────────────────────────────
//
// WebAssemblyHostBuilder.CreateDefault() requires the WASM JS runtime and cannot be invoked in
// a standard test process. These tests instead exercise the identical code paths that
// MetalInjectionBlazorExtensions.AddMetalInjection(WebAssemblyHostBuilder) executes:
//
//   1. MetalInjectionOptionsBuilder is created.
//   2. Blazor's [Inject] is registered as the alternate inject attribute via
//      SetAlternateInjectAttribute<Microsoft.AspNetCore.Components.InjectAttribute>(_ => _.Key).
//   3. The caller's setOptions delegate is applied.
//   4. InitializeServices populates the service collection from the scanned assembly.
//   5. CreateServiceProviderFactory() produces the provider factory, which wraps
//      MetalInjectionServiceProvider — the same provider used in all other tests.
//
// The only step not exercised is ConfigureContainer on the WASM host builder, which is a
// one-line pass-through to the standard IServiceProviderFactory contract already covered by
// ServiceProviderFactoryTests.

// ── Helper types ─────────────────────────────────────────────────────────────────────────────

public interface IBlazorT14Svc { }

[ScopedService(typeof(IBlazorT14Svc))]
public class BlazorT14SvcImpl : IBlazorT14Svc { }

// A component-like class using Blazor's [Inject] attribute for T15
public class BlazorT15Component
{
    [Inject]
    public IBlazorT14Svc? Service { get; set; }
}

// A class that uses both MetalInjection's [Inject] and Blazor's [Inject] for T15
public class BlazorT15DualInjectComponent
{
    [Inject]
    public IBlazorT14Svc? BlazorInjectedService { get; set; }

    [RossWright.MetalInjection.Inject]
    public IBlazorT14Svc? MetalInjectedService { get; set; }
}

// ── Helpers shared across tests ───────────────────────────────────────────────────────────────

file static class BlazorTestProviderFactory
{
    /// <summary>
    /// Builds a MetalInjection provider configured the same way as
    /// MetalInjectionBlazorExtensions.AddMetalInjection, scanning only the supplied assembly.
    /// </summary>
    public static IServiceProvider BuildBlazorProvider(Assembly scannedAssembly,
        Action<IMetalInjectionOptionsBuilder>? extraOptions = null) =>
        new ServiceCollection().BuildMetalInjectionServiceProvider(opts =>
        {
            opts.SetAlternateInjectAttribute<InjectAttribute>(_ => _.Key);
            opts.ScanAssemblies(scannedAssembly);
            extraOptions?.Invoke(opts);
        });
}

// ── AddMetalInjection(WebAssemblyHostBuilder) equivalence tests ────────────────────────────────

public class MetalInjectionBlazorExtensionsTests
{
    // T14a — The scanned service type resolves from the provider after setup.
    [Fact]
    public void ScannedScopedService_ResolvesFromScope_AfterBlazorSetup()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(BlazorT14SvcImpl)]);

        var provider = BlazorTestProviderFactory.BuildBlazorProvider(mockAssembly);

        using var scope = provider.CreateScope();
        var result = scope.ServiceProvider.GetService<IBlazorT14Svc>();

        result.ShouldNotBeNull();
        result.ShouldBeOfType<BlazorT14SvcImpl>();
    }

    // T14b — Blazor's [Inject] attribute is registered as the alternate inject attribute.
    // Verified by resolving a property on a type decorated with Blazor's [Inject] via InjectProperties.
    [Fact]
    public void BlazorInjectAttribute_IsRegisteredAsAlternateInjectAttribute()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(BlazorT14SvcImpl)]);

        var provider = BlazorTestProviderFactory.BuildBlazorProvider(mockAssembly);

        // If [Inject] (Blazor's attribute) is wired, InjectProperties will populate Service.
        using var scope = provider.CreateScope();
        var component = new BlazorT15Component();
        scope.ServiceProvider.InjectProperties(component);

        component.Service.ShouldNotBeNull();
        component.Service.ShouldBeOfType<BlazorT14SvcImpl>();
    }

    // T14c — No exception is thrown during provider build with a scanned assembly.
    [Fact]
    public void BuildWithScannedAssembly_DoesNotThrow()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(BlazorT14SvcImpl)]);

        var ex = Record.Exception(() => BlazorTestProviderFactory.BuildBlazorProvider(mockAssembly));

        ex.ShouldBeNull();
    }
}

// ── Blazor [Inject] property injection through the wired provider ──────────────────────────────

public class MetalInjectionBlazorPropertyInjectionTests
{
    // T15a — Blazor's [Inject]-decorated properties are populated via InjectProperties.
    [Fact]
    public void BlazorInjectProperty_IsPopulated_ByInjectProperties()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(BlazorT14SvcImpl)]);

        var provider = BlazorTestProviderFactory.BuildBlazorProvider(mockAssembly);

        using var scope = provider.CreateScope();
        var component = new BlazorT15Component();
        scope.ServiceProvider.InjectProperties(component);

        component.Service.ShouldNotBeNull();
        component.Service.ShouldBeOfType<BlazorT14SvcImpl>();
    }

    // T15b — Both Blazor's [Inject] and MetalInjection's own [Inject] are honoured simultaneously.
    [Fact]
    public void BothInjectAttributes_AreHonoured_OnSameType()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(BlazorT14SvcImpl)]);

        var provider = BlazorTestProviderFactory.BuildBlazorProvider(mockAssembly);

        using var scope = provider.CreateScope();
        var component = new BlazorT15DualInjectComponent();
        scope.ServiceProvider.InjectProperties(component);

        component.BlazorInjectedService.ShouldNotBeNull();
        component.MetalInjectedService.ShouldNotBeNull();
        component.BlazorInjectedService.ShouldBeOfType<BlazorT14SvcImpl>();
        component.MetalInjectedService.ShouldBeOfType<BlazorT14SvcImpl>();
        // Both properties resolve to the same scoped instance
        component.MetalInjectedService.ShouldBeSameAs(component.BlazorInjectedService);
    }

    // T15c — A property decorated with Blazor's [Inject] on a type resolved from the container
    // is populated during construction (not just via manual InjectProperties).
    [Fact]
    public void BlazorInjectProperty_IsPopulated_WhenTypeIsResolvedFromContainer()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(BlazorT14SvcImpl)]);

        var services = new ServiceCollection();
        services.AddScoped<BlazorT15Component>();
        var provider = BlazorTestProviderFactory.BuildBlazorProvider(mockAssembly, opts =>
        {
            // BlazorT15Component is already in services; scanning the mock assembly adds BlazorT14SvcImpl
        });

        // Re-build with BlazorT15Component in the collection
        var fullServices = new ServiceCollection();
        fullServices.AddScoped<BlazorT15Component>();
        var fullProvider = fullServices.BuildMetalInjectionServiceProvider(opts =>
        {
            opts.SetAlternateInjectAttribute<InjectAttribute>(_ => _.Key);
            opts.ScanAssemblies(mockAssembly);
        });

        using var scope = fullProvider.CreateScope();
        var component = scope.ServiceProvider.GetRequiredService<BlazorT15Component>();

        component.Service.ShouldNotBeNull();
        component.Service.ShouldBeOfType<BlazorT14SvcImpl>();
    }
}
