using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Reflection;
using System.Reflection.Emit;
using Xunit;

namespace RossWright.MetalInjection.Tests;

public class DiscoverServicesTests
{
    [Fact] public void MultipleSingletonRegistrationsForOneImplementation()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(DoubleSingletonService)]);
        var serviceCollection = new ServiceCollection();
        var service = serviceCollection
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));
        var obj1 = service.GetService<ISingletonService1>();
        var obj2 = service.GetService<ISingletonService2>();
        obj2.ShouldBeSameAs(obj1);
    }

    [Fact] public void MultipleSingletonRegistrationsForOneImplementation_SameInstanceAcrossScopes()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(DoubleSingletonService)]);
        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));
        var root1 = serviceProvider.GetService<ISingletonService1>();
        using var scope = serviceProvider.CreateScope();
        var scoped1 = scope.ServiceProvider.GetService<ISingletonService1>();
        var scoped2 = scope.ServiceProvider.GetService<ISingletonService2>();
        scoped1.ShouldBeSameAs(root1);
        scoped2.ShouldBeSameAs(root1);
    }

    [Fact] public void MultipleScopedRegistrationsForOneImplementation_SameScope_ReturnsSameInstance()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(DoubleScopedService)]);
        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));
        using var scope = serviceProvider.CreateScope();
        var obj1 = scope.ServiceProvider.GetService<IScopedStackedService1>();
        var obj2 = scope.ServiceProvider.GetService<IScopedStackedService2>();
        obj2.ShouldBeSameAs(obj1);
    }

    [Fact] public void MultipleScopedRegistrationsForOneImplementation_DifferentScopes_ReturnDifferentInstances()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(DoubleScopedService)]);
        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));
        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();
        var obj1 = scope1.ServiceProvider.GetService<IScopedStackedService1>();
        var obj2 = scope2.ServiceProvider.GetService<IScopedStackedService1>();
        obj2.ShouldNotBeSameAs(obj1);
    }

    [Fact] public void MultipleTransientRegistrationsForOneImplementation_ReturnDifferentInstances()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(DoubleTransientService)]);
        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));
        var obj1 = serviceProvider.GetService<ITransientStackedService1>();
        var obj2 = serviceProvider.GetService<ITransientStackedService2>();
        var obj3 = serviceProvider.GetService<ITransientStackedService1>();
        obj2.ShouldNotBeSameAs(obj1);
        obj3.ShouldNotBeSameAs(obj1);
    }

    // â”€â”€ Registration Syntax â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    [Fact] public void InterfaceSyntax_ISingleton_Registers()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase1SingletonViaInterface)]);
        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        var result1 = serviceProvider.GetService<IPhase1Service>();
        result1.ShouldBeOfType<Phase1SingletonViaInterface>();

        var result2 = serviceProvider.GetService<IPhase1Service>();
        result2.ShouldBeSameAs(result1);

        using var scope = serviceProvider.CreateScope();
        var result3 = scope.ServiceProvider.GetService<IPhase1Service>();
        result3.ShouldBeSameAs(result1);
    }

    [Fact] public void InterfaceSyntax_IScopedService_Registers()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase1ScopedViaInterface)]);
        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<IPhase1Service>());

        using var scope = serviceProvider.CreateScope();
        var result1 = scope.ServiceProvider.GetService<IPhase1Service>();
        result1.ShouldBeOfType<Phase1ScopedViaInterface>();

        var result2 = scope.ServiceProvider.GetService<IPhase1Service>();
        result2.ShouldBeSameAs(result1);
    }

    [Fact] public void InterfaceSyntax_ITransientService_Registers()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase1TransientViaInterface)]);
        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        var result1 = serviceProvider.GetService<IPhase1Service>();
        result1.ShouldBeOfType<Phase1TransientViaInterface>();

        var result2 = serviceProvider.GetService<IPhase1Service>();
        result2.ShouldBeOfType<Phase1TransientViaInterface>();
        result2.ShouldNotBeSameAs(result1);
    }

    [Fact] public void AttributeSyntax_ScopedServiceAttribute_Registers()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase1ScopedViaAttribute)]);
        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<IPhase1Service>());

        using var scope = serviceProvider.CreateScope();
        var result1 = scope.ServiceProvider.GetService<IPhase1Service>();
        result1.ShouldBeOfType<Phase1ScopedViaAttribute>();

        var result2 = scope.ServiceProvider.GetService<IPhase1Service>();
        result2.ShouldBeSameAs(result1);
    }

    [Fact] public void AttributeSyntax_TransientServiceAttribute_Registers()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase1TransientViaAttribute)]);
        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        var result1 = serviceProvider.GetService<IPhase1Service>();
        result1.ShouldBeOfType<Phase1TransientViaAttribute>();

        var result2 = serviceProvider.GetService<IPhase1Service>();
        result2.ShouldBeOfType<Phase1TransientViaAttribute>();
        result2.ShouldNotBeSameAs(result1);
    }

    [Fact] public void AttributeSyntax_TypeMismatch_ThrowsAtStartup()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase1TypeMismatch)]);

        Should.Throw<MetalInjectionException>(() =>
            new ServiceCollection()
                .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly)));
    }

    [Fact] public void SelfRegistration_RegisteredAsConcrete()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase1SelfService)]);
        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        var result1 = serviceProvider.GetService<Phase1SelfService>();
        result1.ShouldBeOfType<Phase1SelfService>();

        var result2 = serviceProvider.GetService<Phase1SelfService>();
        result2.ShouldBeSameAs(result1);
    }

    // â”€â”€ AllowMultipleServices + Ignore â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    [Fact] public void AllowMultipleServicesOf_AllowsDuplicateRegistrationsForSpecificType()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase2MultiImpl1), typeof(Phase2MultiImpl2)]);
        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ =>
            {
                _.ScanAssemblies(mockAssembly);
                _.AllowMultipleServicesOf<IPhase2MultiService>();
            });

        var all = serviceProvider.GetServices<IPhase2MultiService>().ToList();
        all.Count.ShouldBe(2);

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<IPhase2MultiService>());

    }

    [Fact] public void AllowMultipleServicesOfAnyType_AllowsDuplicatesForAllTypes()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([
            typeof(Phase2MultiServiceAImpl1), typeof(Phase2MultiServiceAImpl2),
            typeof(Phase2MultiServiceBImpl1), typeof(Phase2MultiServiceBImpl2)]);
        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ =>
            {
                _.ScanAssemblies(mockAssembly);
                _.AllowMultipleServicesOfAnyType();
            });

        serviceProvider.GetServices<IPhase2MultiServiceA>().Count().ShouldBe(2);
        serviceProvider.GetServices<IPhase2MultiServiceB>().Count().ShouldBe(2);
    }

    [Fact] public void Ignore_ExcludesTypeWithServiceAttribute()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase2IgnoreServiceImpl)]);
        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ =>
            {
                _.ScanAssemblies(mockAssembly);
                _.Ignore<Phase2IgnoreServiceImpl>();
            });

        serviceProvider.GetService<IPhase2IgnoreService>().ShouldBeNull();
    }

    [Fact] public void Ignore_ExcludesConcreteButDoesNotAffectOtherRegistrations()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase2IgnoreServiceImpl), typeof(Phase2OtherServiceImpl)]);
        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ =>
            {
                _.ScanAssemblies(mockAssembly);
                _.Ignore<Phase2IgnoreServiceImpl>();
            });

        serviceProvider.GetService<IPhase2IgnoreService>().ShouldBeNull();
        serviceProvider.GetService<IPhase2OtherService>().ShouldNotBeNull();
    }

    [Fact] public void Ignore_ExcludesTypeWithConfigSectionAttribute()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(Phase2ConfigIgnoreSettings)]);
        var configuration = Substitute.For<IConfiguration>();
        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(
                _ =>
                {
                    _.ScanAssemblies(mockAssembly);
                    _.Ignore<Phase2ConfigIgnoreSettings>();
                },
                configuration);

        serviceProvider.GetService<Phase2ConfigIgnoreSettings>().ShouldBeNull();
    }

    // â”€â”€ SetEntryAssembly + StrictResolution (startup) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    [Fact] public void SetEntryAssembly_WinsConflictOverNonEntryAssembly()
    {
        Assembly mockEntryAssembly = Substitute.For<Assembly>();
        mockEntryAssembly.GetTypes().Returns([typeof(Phase3EntryImpl)]);

        Assembly mockNonEntryAssembly = Substitute.For<Assembly>();
        mockNonEntryAssembly.GetTypes().Returns([Phase3TypeFactory.NonEntryType]);

        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ =>
            {
                _.ScanAssemblies(mockEntryAssembly, mockNonEntryAssembly);
                _.SetEntryAssembly(typeof(Phase3EntryImpl).Assembly);
            });

        serviceProvider.GetService<IPhase3ConflictService>().ShouldBeOfType<Phase3EntryImpl>();
    }

    [Fact] public void DuplicateRegistration_WithoutEntryAssembly_ThrowsAtStartup()
    {
        Assembly mockAssembly1 = Substitute.For<Assembly>();
        mockAssembly1.GetTypes().Returns([typeof(Phase3EntryImpl)]);

        Assembly mockAssembly2 = Substitute.For<Assembly>();
        mockAssembly2.GetTypes().Returns([Phase3TypeFactory.NonEntryType]);

        Should.Throw<MetalInjectionException>(() =>
            new ServiceCollection()
                .BuildMetalInjectionServiceProvider(_ =>
                {
                    _.ScanAssemblies(mockAssembly1, mockAssembly2);

                }));
    }

    [Fact] public void SetEntryAssembly_DominanceResolvesConflict()
    {
        Assembly mockEntryAssembly = Substitute.For<Assembly>();
        mockEntryAssembly.GetTypes().Returns([typeof(Phase3EntryImpl)]);

        Assembly mockNonEntryAssembly = Substitute.For<Assembly>();
        mockNonEntryAssembly.GetTypes().Returns([Phase3TypeFactory.NonEntryType]);

        var serviceProvider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ =>
            {
                _.ScanAssemblies(mockEntryAssembly, mockNonEntryAssembly);
                _.SetEntryAssembly(typeof(Phase3EntryImpl).Assembly);
            });

        serviceProvider.GetService<IPhase3ConflictService>().ShouldNotBeNull();
    }

    [Fact] public void DuplicateRegistration_WithoutMatchingEntryAssembly_ThrowsAtStartup()
    {
        Assembly mockAssembly1 = Substitute.For<Assembly>();
        mockAssembly1.GetTypes().Returns([typeof(Phase3EntryImpl)]);

        Assembly mockAssembly2 = Substitute.For<Assembly>();
        mockAssembly2.GetTypes().Returns([Phase3TypeFactory.NonEntryType]);

        // No SetEntryAssembly — entry defaults to testhost, which matches neither impl.
        // Without a single entry-assembly winner, duplicates always throw.
        Should.Throw<MetalInjectionException>(() =>
            new ServiceCollection()
                .BuildMetalInjectionServiceProvider(_ =>
                {
                    _.ScanAssemblies(mockAssembly1, mockAssembly2);
                }));
    }

    // ── T5: SetEntryAssembly with no matching types in entry assembly ─────────────────────────

    [Fact] public void SetEntryAssembly_EntryAssemblyContributesNoTypes_NonEntryRegistrationWinsWithoutError()
    {
        // Entry assembly contributes zero types for IPhase3ConflictService.
        // The non-entry assembly contributes one implementation — it should win without error.
        Assembly emptyEntryAssembly = Substitute.For<Assembly>();
        emptyEntryAssembly.GetTypes().Returns([]);

        Assembly mockNonEntryAssembly = Substitute.For<Assembly>();
        mockNonEntryAssembly.GetTypes().Returns([typeof(Phase3EntryImpl)]);

        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ =>
            {
                _.ScanAssemblies(emptyEntryAssembly, mockNonEntryAssembly);
                _.SetEntryAssembly(typeof(Phase3EntryImpl).Assembly);
            });

        var result = provider.GetService<IPhase3ConflictService>();
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Phase3EntryImpl>();
    }

    [Fact] public void KeyedAttributeRegistration_Singleton_ResolvesWithKey()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(KeyedScanSingletonImpl)]);
        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        var result = provider.GetKeyedService<IKeyedScanService>("scan-key");

        result.ShouldNotBeNull();
        result.ShouldBeOfType<KeyedScanSingletonImpl>();
    }

    [Fact] public void KeyedAttributeRegistration_Scoped_ResolvesWithKeyInScope()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(KeyedScanScopedImpl)]);
        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        using var scope = provider.CreateScope();
        var result = scope.ServiceProvider.GetKeyedService<IKeyedScanService>("scope-scan-key");

        result.ShouldNotBeNull();
        result.ShouldBeOfType<KeyedScanScopedImpl>();
    }

    [Fact] public void KeyedAttributeRegistration_Transient_ResolvesWithKey()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(KeyedScanTransientImpl)]);
        var provider = new ServiceCollection()
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));

        var result = provider.GetKeyedService<IKeyedScanService>("transient-scan-key");

        result.ShouldNotBeNull();
        result.ShouldBeOfType<KeyedScanTransientImpl>();
    }
}

public interface ISingletonService1 { }
public interface ISingletonService2 { }

[Singleton<ISingletonService1>]
[Singleton<ISingletonService2>]
public class DoubleSingletonService : ISingletonService1, ISingletonService2
{
}

public interface IScopedStackedService1 { }
public interface IScopedStackedService2 { }

[ScopedService<IScopedStackedService1>]
[ScopedService<IScopedStackedService2>]
public class DoubleScopedService : IScopedStackedService1, IScopedStackedService2
{
}

public interface ITransientStackedService1 { }
public interface ITransientStackedService2 { }

[TransientService<ITransientStackedService1>]
[TransientService<ITransientStackedService2>]
public class DoubleTransientService : ITransientStackedService1, ITransientStackedService2
{
}

// â”€â”€ Phase 1 types â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public interface IPhase1Service { }

public class Phase1SingletonViaInterface : IPhase1Service, ISingleton<IPhase1Service> { }
public class Phase1ScopedViaInterface : IPhase1Service, IScopedService<IPhase1Service> { }
public class Phase1TransientViaInterface : IPhase1Service, ITransientService<IPhase1Service> { }

[ScopedService<IPhase1Service>]
public class Phase1ScopedViaAttribute : IPhase1Service { }

[TransientService<IPhase1Service>]
public class Phase1TransientViaAttribute : IPhase1Service { }

[Singleton<IPhase1Service>] // intentionally does not implement IPhase1Service
public class Phase1TypeMismatch { }

[Singleton<Phase1SelfService>]
public class Phase1SelfService { }

// â”€â”€ Phase 2 types â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public interface IPhase2MultiService { }
[Singleton<IPhase2MultiService>] public class Phase2MultiImpl1 : IPhase2MultiService { }
[Singleton<IPhase2MultiService>] public class Phase2MultiImpl2 : IPhase2MultiService { }

public interface IPhase2MultiServiceA { }
[Singleton<IPhase2MultiServiceA>] public class Phase2MultiServiceAImpl1 : IPhase2MultiServiceA { }
[Singleton<IPhase2MultiServiceA>] public class Phase2MultiServiceAImpl2 : IPhase2MultiServiceA { }

public interface IPhase2MultiServiceB { }
[Singleton<IPhase2MultiServiceB>] public class Phase2MultiServiceBImpl1 : IPhase2MultiServiceB { }
[Singleton<IPhase2MultiServiceB>] public class Phase2MultiServiceBImpl2 : IPhase2MultiServiceB { }

public interface IPhase2IgnoreService { }
[Singleton<IPhase2IgnoreService>] public class Phase2IgnoreServiceImpl : IPhase2IgnoreService { }

public interface IPhase2OtherService { }
public class Phase2OtherServiceImpl : IPhase2OtherService, ISingleton<IPhase2OtherService> { }

[ConfigSection("Phase2:Settings")]
public class Phase2ConfigIgnoreSettings { }

// â”€â”€ Phase 3 types â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public interface IPhase3ConflictService { }
public class Phase3EntryImpl : IPhase3ConflictService, ISingleton<IPhase3ConflictService> { }

/// <summary>
/// Creates Phase3NonEntryImpl in a separately-named dynamic assembly so its
/// Assembly.FullName differs from the test assembly, enabling the entry-assembly
/// dominance logic to distinguish between the two conflicting implementations.
/// </summary>
internal static class Phase3TypeFactory
{
    internal static readonly Type NonEntryType = CreateNonEntryType();

    private static Type CreateNonEntryType()
    {
        var ab = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName("Phase3.NonEntry"), AssemblyBuilderAccess.Run);
        var mb = ab.DefineDynamicModule("Phase3.NonEntry");
        var tb = mb.DefineType(
            "Phase3NonEntryImpl",
            TypeAttributes.Public,
            null,
            new Type[] { typeof(IPhase3ConflictService), typeof(ISingleton<IPhase3ConflictService>) });
        return tb.CreateType()!;
    }
}

// -- G-20 types --
public interface IKeyedScanService { }

[Singleton<IKeyedScanService>("scan-key")]
public class KeyedScanSingletonImpl : IKeyedScanService { }

[ScopedService<IKeyedScanService>("scope-scan-key")]
public class KeyedScanScopedImpl : IKeyedScanService { }

[TransientService<IKeyedScanService>("transient-scan-key")]
public class KeyedScanTransientImpl : IKeyedScanService { }
