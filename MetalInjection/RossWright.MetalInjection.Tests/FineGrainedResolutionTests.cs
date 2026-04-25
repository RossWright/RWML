using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Reflection;
using Xunit;

namespace RossWright.MetalInjection.Tests;

// ── Scenario 1: [AllowMultipleRegistrations] attribute ──────────────────────────────────────

public class AllowMultipleRegistrationsAttributeTests
{
    public interface IChannel { }

    [AllowMultipleRegistrations]
    public interface IMultiChannel { }

    [Singleton<IChannel>] public class ChannelA : IChannel { }
    [Singleton<IChannel>] public class ChannelB : IChannel { }

    [Singleton<IMultiChannel>] public class MultiChannelA : IMultiChannel { }
    [Singleton<IMultiChannel>] public class MultiChannelB : IMultiChannel { }

    [Fact] public void DuplicateWithoutAttribute_ThrowsAtStartup()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(ChannelA), typeof(ChannelB)]);

        Should.Throw<MetalInjectionException>(() =>
            new ServiceCollection()
                .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly)));
    }

    [Fact] public void DuplicateWithAttribute_AllowsRegistration()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(MultiChannelA), typeof(MultiChannelB)]);

        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        var all = provider.GetServices<IMultiChannel>().ToList();
        all.Count.ShouldBe(2);
    }

    [Fact] public void DuplicateWithAttribute_GetServicesReturnsAll()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(MultiChannelA), typeof(MultiChannelB)]);

        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        var all = provider.GetServices<IMultiChannel>().ToList();
        all.ShouldContain(s => s is MultiChannelA);
        all.ShouldContain(s => s is MultiChannelB);
    }

    [Fact] public void DuplicateWithAttribute_GetSingleServiceThrows()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(MultiChannelA), typeof(MultiChannelB)]);

        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        Should.Throw<MetalInjectionException>(() => provider.GetService<IMultiChannel>());
    }

    [Fact] public void AllowMultipleRegistrationsAttribute_AndAllowMultipleServicesOf_AreEquivalent()
    {
        Assembly mockAssembly1 = Substitute.For<Assembly>();
        mockAssembly1.GetTypes().Returns([typeof(MultiChannelA), typeof(MultiChannelB)]);

        var providerViaAttr = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly1));

        Assembly mockAssembly2 = Substitute.For<Assembly>();
        mockAssembly2.GetTypes().Returns([typeof(ChannelA), typeof(ChannelB)]);

        var providerViaOption = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ =>
            {
                _.ScanAssemblies(mockAssembly2);
                _.AllowMultipleServicesOf<IChannel>();
            });

        providerViaAttr.GetServices<IMultiChannel>().Count().ShouldBe(2);
        providerViaOption.GetServices<IChannel>().Count().ShouldBe(2);
    }
}

// ── Scenario 2: [AllowRootResolution] attribute ──────────────────────────────────────────────

public class AllowRootResolutionAttributeTests
{
    public interface IProcessor { }
    public interface IGuardedProcessor { }

    [ScopedService<IProcessor>] public class ScopedProcessor : IProcessor { }

    [AllowRootResolution]
    [ScopedService<IGuardedProcessor>] public class AllowedRootScopedProcessor : IGuardedProcessor { }

    [Fact] public void ScopedService_ResolvedFromRoot_ThrowsByDefault()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(ScopedProcessor)]);

        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        Should.Throw<MetalInjectionException>(() => provider.GetService<IProcessor>());
    }

    [Fact] public void ScopedServiceWithAllowRootResolution_ResolvedFromRoot_Succeeds()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(AllowedRootScopedProcessor)]);

        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        var result = provider.GetService<IGuardedProcessor>();
        result.ShouldNotBeNull();
        result.ShouldBeOfType<AllowedRootScopedProcessor>();
    }

    [Fact] public void ScopedServiceWithAllowRootResolution_ScopeResolution_AlsoWorks()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(AllowedRootScopedProcessor)]);

        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        using var scope = provider.CreateScope();
        var result = scope.ServiceProvider.GetService<IGuardedProcessor>();
        result.ShouldNotBeNull();
    }

    // -- G-14: [AllowRootResolution] from root returns same instance on repeat (singleton cache) -

    [Fact] public void ScopedServiceWithAllowRootResolution_RepeatedRootResolution_ReturnsSameInstance()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(AllowedRootScopedProcessor)]);

        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        var first = provider.GetService<IGuardedProcessor>();
        var second = provider.GetService<IGuardedProcessor>();

        first.ShouldNotBeNull();
        second.ShouldBeSameAs(first);
    }

    // -- G-16: [AllowRootResolution] on a manually-registered service resolves from root -------

    public interface IManualProcessor { }

    [AllowRootResolution]
    public class ManuallyRegisteredAllowedProcessor : IManualProcessor { }

    [Fact] public void ScopedServiceWithAllowRootResolution_ManualRegistration_ResolvedFromRoot_Succeeds()
    {
        var sc = new ServiceCollection();
        sc.AddScoped<IManualProcessor, ManuallyRegisteredAllowedProcessor>();
        var provider = sc.BuildMetalInjectionServiceProvider();

        var result = provider.GetService<IManualProcessor>();

        result.ShouldNotBeNull();
        result.ShouldBeOfType<ManuallyRegisteredAllowedProcessor>();
    }

    // -- G-17: Keyed scoped + [AllowRootResolution] resolves from root -----------------------

    public interface IKeyedRootProcessor { }

    [AllowRootResolution]
    public class KeyedAllowedRootProcessor : IKeyedRootProcessor { }

    [Fact] public void KeyedScopedServiceWithAllowRootResolution_ResolvedFromRoot_Succeeds()
    {
        var sc = new ServiceCollection();
        sc.AddKeyedScoped<IKeyedRootProcessor, KeyedAllowedRootProcessor>("kkey");
        var provider = sc.BuildMetalInjectionServiceProvider();

        var result = provider.GetKeyedService<IKeyedRootProcessor>("kkey");

        result.ShouldNotBeNull();
        result.ShouldBeOfType<KeyedAllowedRootProcessor>();
    }
}

// ── Scenario 3: AllowRootScopedResolution() global option ───────────────────────────────────

public class AllowRootScopedResolutionOptionTests
{
    public interface IBackgroundJob { }

    [ScopedService<IBackgroundJob>] public class BackgroundJob : IBackgroundJob { }

    [Fact] public void AllowRootScopedResolution_GlobalOption_AllowsAllScopedFromRoot()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(BackgroundJob)]);

        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ =>
            {
                _.ScanAssemblies(mockAssembly);
                _.AllowRootScopedResolution();
            });

        var result = provider.GetService<IBackgroundJob>();
        result.ShouldNotBeNull();
        result.ShouldBeOfType<BackgroundJob>();
    }

    [Fact] public void WithoutAllowRootScopedResolution_ScopedFromRoot_StillThrows()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(BackgroundJob)]);

        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        Should.Throw<MetalInjectionException>(() => provider.GetService<IBackgroundJob>());
    }

    // -- AllowRootScopedResolution() global option returns same instance on repeat -------------

    [Fact] public void AllowRootScopedResolution_GlobalOption_RepeatedRootResolution_ReturnsSameInstance()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(BackgroundJob)]);

        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ =>
            {
                _.ScanAssemblies(mockAssembly);
                _.AllowRootScopedResolution();
            });

        var first = provider.GetService<IBackgroundJob>();
        var second = provider.GetService<IBackgroundJob>();

        first.ShouldNotBeNull();
        second.ShouldBeSameAs(first);
    }
}

// ── Scenario 4: [Inject(Optional = true/false)] on properties ───────────────────────────────

public class InjectAttributeOptionalFlagTests
{
    public interface IOptionalDep { }
    public interface IRequiredDep { }

    public class ServiceWithOptionalFlagTrue
    {
        [Inject(Optional = true)] public IRequiredDep? ForcedOptional { get; set; }
    }

    public class ServiceWithOptionalFlagFalse
    {
        [Inject(Optional = false)] public IOptionalDep? ForcedRequired { get; set; }
    }

    public class ServiceWithNullableOptional
    {
        [Inject] public IOptionalDep? OptionalViaNullability { get; set; }
    }

    [Fact] public void InjectOptionalTrue_MissingService_LeavesPropertyAtDefault()
    {
        var services = new ServiceCollection();
        var provider = services.BuildMetalInjectionServiceProvider(_ => { });

        var target = new ServiceWithOptionalFlagTrue();
        provider.InjectProperties(target);

        target.ForcedOptional.ShouldBeNull();
    }

    [Fact] public void InjectOptionalFalse_MissingService_Throws()
    {
        var services = new ServiceCollection();
        var provider = services.BuildMetalInjectionServiceProvider(_ => { });

        var target = new ServiceWithOptionalFlagFalse();
        Should.Throw<InvalidOperationException>(() => provider.InjectProperties(target));
    }

    [Fact] public void InjectNullableProperty_MissingService_LeavesPropertyAtDefault()
    {
        var services = new ServiceCollection();
        var provider = services.BuildMetalInjectionServiceProvider(_ => { });

        var target = new ServiceWithNullableOptional();
        provider.InjectProperties(target);

        target.OptionalViaNullability.ShouldBeNull();
    }

    [Fact] public void InjectOptionalTrue_PresentService_SetsProperty()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IRequiredDep>(Substitute.For<IRequiredDep>());
        var provider = services.BuildMetalInjectionServiceProvider(_ => { });

        var target = new ServiceWithOptionalFlagTrue();
        provider.InjectProperties(target);

        target.ForcedOptional.ShouldNotBeNull();
    }

    // -- [Inject(Optional = false)] + present service ? sets property ---------------------------

    [Fact] public void InjectOptionalFalse_PresentService_SetsProperty()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOptionalDep>(Substitute.For<IOptionalDep>());
        var provider = services.BuildMetalInjectionServiceProvider(_ => { });

        var target = new ServiceWithOptionalFlagFalse();
        provider.InjectProperties(target);

        target.ForcedRequired.ShouldNotBeNull();
    }
}

// ── Scenario 5: [Inject] on constructor parameters ─────────────────────────────────────────

public class InjectOnConstructorParameterTests
{
    public interface IFoo { }
    public interface IBar { }
    public interface IKeyedFoo { }

    public class FooImpl : IFoo { }
    public class BarImpl : IBar { }
    public class KeyedFooImpl : IKeyedFoo { }

    [Singleton<IFoo>] public class FooService : IFoo { }

    public class ServiceWithOptionalCtorParam
    {
        public IBar? Bar { get; }

        public ServiceWithOptionalCtorParam([Inject(Optional = true)] IBar bar = null!)
        {
            Bar = bar;
        }
    }

    public class ServiceWithRequiredCtorParam
    {
        public IBar Bar { get; }

        public ServiceWithRequiredCtorParam([Inject(Optional = false)] IBar bar)
        {
            Bar = bar;
        }
    }

    public class ServiceWithKeyedCtorParam
    {
        public IKeyedFoo KeyedFoo { get; }

        public ServiceWithKeyedCtorParam([Inject("premium")] IKeyedFoo keyedFoo)
        {
            KeyedFoo = keyedFoo;
        }
    }

    [Fact] public void InjectOptionalOnCtorParam_MissingService_ConstructsWithNull()
    {
        var services = new ServiceCollection();
        services.AddTransient<ServiceWithOptionalCtorParam>();
        var provider = services.BuildMetalInjectionServiceProvider(_ => { });

        var result = provider.GetService<ServiceWithOptionalCtorParam>();
        result.ShouldNotBeNull();
        result.Bar.ShouldBeNull();
    }

    [Fact] public void InjectRequiredOnCtorParam_MissingService_FailsConstruction()
    {
        var services = new ServiceCollection();
        services.AddTransient<ServiceWithRequiredCtorParam>();
        var provider = services.BuildMetalInjectionServiceProvider(_ => { });

        Should.Throw<MetalInjectionException>(() => provider.GetService<ServiceWithRequiredCtorParam>());
    }

    [Fact] public void InjectKeyOnCtorParam_ResolvesKeyedService()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IKeyedFoo>("premium", new KeyedFooImpl());
        services.AddTransient<ServiceWithKeyedCtorParam>();
        var provider = services.BuildMetalInjectionServiceProvider(_ => { });

        var result = provider.GetService<ServiceWithKeyedCtorParam>();
        result.ShouldNotBeNull();
        result.KeyedFoo.ShouldBeOfType<KeyedFooImpl>();
    }
}

// ── Scenario 6: Default-value constructor parameters are optional ───────────────────────────

public class DefaultValueCtorParameterTests
{
    public interface IOptionalService { }

    public class ServiceWithDefaultParam
    {
        public IOptionalService? Dep { get; }

        public ServiceWithDefaultParam(IOptionalService dep = null!)
        {
            Dep = dep;
        }
    }

    public class ServiceWithNullableParam
    {
        public IOptionalService? Dep { get; }

        public ServiceWithNullableParam(IOptionalService? dep)
        {
            Dep = dep;
        }
    }

    [Fact] public void CtorParamWithDefaultValue_MissingService_ConstructsWithNull()
    {
        var services = new ServiceCollection();
        services.AddTransient<ServiceWithDefaultParam>();
        var provider = services.BuildMetalInjectionServiceProvider(_ => { });

        var result = provider.GetService<ServiceWithDefaultParam>();
        result.ShouldNotBeNull();
        result.Dep.ShouldBeNull();
    }

    [Fact] public void CtorParamNullable_MissingService_ConstructsWithNull()
    {
        var services = new ServiceCollection();
        services.AddTransient<ServiceWithNullableParam>();
        var provider = services.BuildMetalInjectionServiceProvider(_ => { });

        var result = provider.GetService<ServiceWithNullableParam>();
        result.ShouldNotBeNull();
        result.Dep.ShouldBeNull();
    }

    [Fact] public void CtorParamWithDefaultValue_PresentService_UsesService()
    {
        var services = new ServiceCollection();
        var dep = Substitute.For<IOptionalService>();
        services.AddSingleton(dep);
        services.AddTransient<ServiceWithDefaultParam>();
        var provider = services.BuildMetalInjectionServiceProvider(_ => { });

        var result = provider.GetService<ServiceWithDefaultParam>();
        result.ShouldNotBeNull();
        result.Dep.ShouldBeSameAs(dep);
    }
}

// -- Nested IServiceScope disposal order ---------------------------------------------------------

public class NestedScopeDisposalOrderTests
{
    public interface INestedScopeSvc { }
    public class DisposableNestedScopeSvc : INestedScopeSvc, IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }

    [Fact]
    public void ParentScopeDisposal_DoesNotPrematurelyInvalidateChildScope()
    {
        var services = new ServiceCollection();
        services.AddScoped<DisposableNestedScopeSvc>();
        var provider = services.BuildMetalInjectionServiceProvider();

        // Create parent scope, then a child scope from the parent's provider
        var parentScope = provider.CreateScope();
        var childScope = parentScope.ServiceProvider.CreateScope();

        var childInstance = childScope.ServiceProvider.GetRequiredService<DisposableNestedScopeSvc>();

        // Child scope is still alive and can resolve services before parent is disposed
        childInstance.ShouldNotBeNull();
        childInstance.Disposed.ShouldBeFalse();

        // Dispose parent � child scope owns its own lifetime; child instance should NOT be disposed yet
        parentScope.Dispose();

        childInstance.Disposed.ShouldBeFalse();

        // Explicitly disposing the child scope disposes its services
        childScope.Dispose();

        childInstance.Disposed.ShouldBeTrue();
    }

    [Fact]
    public void ChildScopeAndParentScope_HaveIndependentServiceInstances()
    {
        var services = new ServiceCollection();
        services.AddScoped<DisposableNestedScopeSvc>();
        var provider = services.BuildMetalInjectionServiceProvider();

        using var parentScope = provider.CreateScope();
        using var childScope = parentScope.ServiceProvider.CreateScope();

        var parentInstance = parentScope.ServiceProvider.GetRequiredService<DisposableNestedScopeSvc>();
        var childInstance = childScope.ServiceProvider.GetRequiredService<DisposableNestedScopeSvc>();

        childInstance.ShouldNotBeSameAs(parentInstance);
    }
}
