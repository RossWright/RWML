using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Reflection;
using Xunit;

namespace RossWright.MetalInjection.Tests;

// ── Advanced / gap-coverage tests ──────────────────────────────────────────────────────────────

// ── Open Generic Registration ─────────────────────────────────────────────────────────────────

public interface IAdvOpenGenericSvc<T> { }

[Singleton(typeof(IAdvOpenGenericSvc<>))]
public class AdvOpenGenericSvc<T> : IAdvOpenGenericSvc<T> { }

public class OpenGenericServiceRegistrationTests
{
    [Fact]
    public void OpenGenericSingleton_ResolvesClosedGenericType()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(AdvOpenGenericSvc<>)]);
        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        var result = provider.GetService<IAdvOpenGenericSvc<string>>();

        Assert.NotNull(result);
        Assert.IsType<AdvOpenGenericSvc<string>>(result);
    }
}

// ── Keyed Constructor Injection ───────────────────────────────────────────────────────────────

public interface IAdvKeyedCtorSvc { }
public class AdvKeyedCtorSvcImpl : IAdvKeyedCtorSvc { }

public class AdvKeyedConsumer
{
    public IAdvKeyedCtorSvc Svc { get; }
    public AdvKeyedConsumer([FromKeyedServices("adv-ctor-key")] IAdvKeyedCtorSvc svc) => Svc = svc;
}

public class KeyedConstructorInjectionTests
{
    [Fact]
    public void KeyedService_InjectedViaConstructorAttribute()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IAdvKeyedCtorSvc, AdvKeyedCtorSvcImpl>("adv-ctor-key");
        var provider = services.BuildMetalInjectionServiceProvider();

        var consumer = ActivatorUtilities.CreateInstance<AdvKeyedConsumer>(provider);

        Assert.NotNull(consumer.Svc);
        Assert.IsType<AdvKeyedCtorSvcImpl>(consumer.Svc);
    }
}

// ── SetAlternateInjectAttribute — generic overload, no service key ────────────────────────────

[AttributeUsage(AttributeTargets.Property)]
public class AdvAlternateInjectAttribute : Attribute { }

public class AdvAlternateInjectTarget
{
    [AdvAlternateInjectAttribute]
    public PropertyInjectionTestService? AltService { get; set; }
}

public class SetAlternateInjectAttributeGenericTests
{
    [Fact]
    public void SetAlternateInjectAttributeGeneric_UnkeyedVariant_ResolvesProperty()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<PropertyInjectionTestService>();
        var provider = serviceCollection.BuildMetalInjectionServiceProvider(opts =>
            opts.SetAlternateInjectAttribute<AdvAlternateInjectAttribute>(_ => null));

        var obj = ActivatorUtilities.CreateInstance<AdvAlternateInjectTarget>(provider);

        Assert.NotNull(obj.AltService);
    }
}

// ── Ignore(Type) — non-generic overload ──────────────────────────────────────────────────────

public interface IAdvIgnoreSvc { }

[Singleton(typeof(IAdvIgnoreSvc))]
public class AdvIgnoreSvcImpl : IAdvIgnoreSvc { }

public class IgnoreByTypeTests
{
    [Fact]
    public void Ignore_ByType_ExcludesServiceFromRegistration()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(AdvIgnoreSvcImpl)]);
        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ =>
            {
                _.ScanAssemblies(mockAssembly);
                _.Ignore(typeof(AdvIgnoreSvcImpl));
            });

        Assert.Null(provider.GetService<IAdvIgnoreSvc>());
    }
}

// ── AllowMultipleServicesOf(Type) — non-generic overload ─────────────────────────────────────

public interface IAdvMultiSvc { }

[Singleton(typeof(IAdvMultiSvc))]
public class AdvMultiSvcImpl1 : IAdvMultiSvc { }

[Singleton(typeof(IAdvMultiSvc))]
public class AdvMultiSvcImpl2 : IAdvMultiSvc { }

public class AllowMultipleServicesOfByTypeTests
{
    [Fact]
    public void AllowMultipleServicesOf_ByType_AllowsDuplicateRegistrations()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(AdvMultiSvcImpl1), typeof(AdvMultiSvcImpl2)]);
        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ =>
            {
                _.ScanAssemblies(mockAssembly);
                _.AllowMultipleServicesOf(typeof(IAdvMultiSvc));
            });

        var all = provider.GetServices<IAdvMultiSvc>().ToList();
        Assert.Equal(2, all.Count);
    }
}

// ── Scoped service accessed from root provider via CreateInstance ──────────────────────────────

public interface IAdvScopedDep { }
public class AdvScopedDepImpl : IAdvScopedDep { }

public class AdvScopedDepConsumer
{
    public AdvScopedDepConsumer(IAdvScopedDep dep) { }
}

public class ScopedServiceOutsideScopeTests
{
    [Fact]
    public void ScopedService_AccessedViaCreateInstance_FromRoot_ThrowsMetalInjectionException()
    {
        var services = new ServiceCollection();
        services.AddScoped<IAdvScopedDep, AdvScopedDepImpl>();
        var provider = services.BuildMetalInjectionServiceProvider();

        Assert.Throws<MetalInjectionException>(() => provider.CreateInstance<AdvScopedDepConsumer>());
    }
}

// ── Disposable service disposal ───────────────────────────────────────────────────────────────

public class AdvDisposableService : IDisposable
{
    public bool Disposed { get; private set; }
    public void Dispose() => Disposed = true;
}

public class DisposableServiceTests
{
    [Fact]
    public void DisposableTransient_IsDisposed_WhenProviderIsDisposed()
    {
        var services = new ServiceCollection();
        services.AddTransient<AdvDisposableService>();
        var provider = services.BuildMetalInjectionServiceProvider();

        var instance = provider.GetRequiredService<AdvDisposableService>();
        Assert.False(instance.Disposed);

        ((IDisposable)provider).Dispose();

        Assert.True(instance.Disposed);
    }

    [Fact]
    public void DisposableScoped_IsDisposed_WhenScopeIsDisposed()
    {
        var services = new ServiceCollection();
        services.AddScoped<AdvDisposableService>();
        var provider = services.BuildMetalInjectionServiceProvider();

        AdvDisposableService instance;
        using (var scope = provider.CreateScope())
        {
            instance = scope.ServiceProvider.GetRequiredService<AdvDisposableService>();
            Assert.False(instance.Disposed);
        }

        Assert.True(instance.Disposed);
    }

    [Fact]
    public void DisposableSingleton_IsDisposed_WhenRootProviderIsDisposed()
    {
        var services = new ServiceCollection();
        services.AddSingleton<AdvDisposableService>();
        var provider = services.BuildMetalInjectionServiceProvider();

        var instance = provider.GetRequiredService<AdvDisposableService>();
        Assert.False(instance.Disposed);

        ((IDisposable)provider).Dispose();

        Assert.True(instance.Disposed);
    }

    // ── Transient IDisposable in a scope is disposed when that scope is disposed ─────────────

    [Fact]
    public void DisposableTransient_InScope_IsDisposed_WhenScopeIsDisposed()
    {
        var services = new ServiceCollection();
        services.AddTransient<AdvDisposableService>();
        var provider = services.BuildMetalInjectionServiceProvider();

        AdvDisposableService instance;
        using (var scope = provider.CreateScope())
        {
            instance = scope.ServiceProvider.GetRequiredService<AdvDisposableService>();
            Assert.False(instance.Disposed);
        }

        Assert.True(instance.Disposed);
    }
}

// ── IAsyncDisposable tracking ───────────────────────────────────────────────────────────────────

public class AdvAsyncDisposableService : IAsyncDisposable
{
    public bool Disposed { get; private set; }
    public ValueTask DisposeAsync() { Disposed = true; return ValueTask.CompletedTask; }
}

public class AsyncDisposableTrackingTests
{
    [Fact]
    public void AsyncDisposableTransient_IsDisposed_WhenProviderIsDisposed()
    {
        var services = new ServiceCollection();
        services.AddTransient<AdvAsyncDisposableService>();
        var provider = services.BuildMetalInjectionServiceProvider();

        var instance = provider.GetRequiredService<AdvAsyncDisposableService>();
        Assert.False(instance.Disposed);

        ((IDisposable)provider).Dispose();

        Assert.True(instance.Disposed);
    }

    [Fact]
    public void AsyncDisposableTransient_IsDisposed_WhenScopeIsDisposed()
    {
        var services = new ServiceCollection();
        services.AddTransient<AdvAsyncDisposableService>();
        var provider = services.BuildMetalInjectionServiceProvider();

        AdvAsyncDisposableService instance;
        using (var scope = provider.CreateScope())
        {
            instance = scope.ServiceProvider.GetRequiredService<AdvAsyncDisposableService>();
            Assert.False(instance.Disposed);
        }

        Assert.True(instance.Disposed);
    }
}

// ── MetalInjectionServiceProviderFactory direct tests ──────────────────────────────────────────

public interface IFactoryT1Svc { }
public class FactoryT1SvcImpl : IFactoryT1Svc { }

[Singleton<IFactoryT1Svc>]
public class FactoryT1ScannedSvc : IFactoryT1Svc { }

public class ServiceProviderFactoryTests
{
    [Fact]
    public void CreateBuilder_ReturnsSameServiceCollection()
    {
        var options = MetalInjectionOptionsBuilder.Create(InternalKey.Value);
        var factory = options.CreateServiceProviderFactory();
        var services = new ServiceCollection();

        var builder = factory.CreateBuilder(services);

        Assert.Same(services, builder);
    }

    [Fact]
    public void CreateServiceProvider_ResolvesManuallyRegisteredService()
    {
        var options = MetalInjectionOptionsBuilder.Create(InternalKey.Value);
        var factory = options.CreateServiceProviderFactory();
        var services = new ServiceCollection();
        services.AddSingleton<IFactoryT1Svc, FactoryT1SvcImpl>();

        var builder = factory.CreateBuilder(services);
        var provider = factory.CreateServiceProvider(builder);

        var result = provider.GetService<IFactoryT1Svc>();

        Assert.NotNull(result);
        Assert.IsType<FactoryT1SvcImpl>(result);
    }

    [Fact]
    public void CreateServiceProvider_IsEquivalentToBuildMetalInjectionServiceProvider_ForManualRegistrations()
    {
        // Via factory
        var options = MetalInjectionOptionsBuilder.Create(InternalKey.Value);
        var factory = options.CreateServiceProviderFactory();
        var services1 = new ServiceCollection();
        services1.AddSingleton<IFactoryT1Svc, FactoryT1SvcImpl>();
        var factoryProvider = factory.CreateServiceProvider(factory.CreateBuilder(services1));

        // Via BuildMetalInjectionServiceProvider
        var services2 = new ServiceCollection();
        services2.AddSingleton<IFactoryT1Svc, FactoryT1SvcImpl>();
        var directProvider = services2.BuildMetalInjectionServiceProvider();

        Assert.IsType<FactoryT1SvcImpl>(factoryProvider.GetService<IFactoryT1Svc>());
        Assert.IsType<FactoryT1SvcImpl>(directProvider.GetService<IFactoryT1Svc>());
    }
}