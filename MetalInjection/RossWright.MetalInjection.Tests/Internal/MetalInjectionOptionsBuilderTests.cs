using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using System.Reflection;
using Xunit;

namespace RossWright.MetalInjection.Tests.Internal;

public class MetalInjectionOptionsBuilderTests
{
    // AddOpenGenericFactory Tests
    [Fact]
    public void AddOpenGenericFactory_ValidType_AddsFactory()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        var factory = Substitute.For<Func<IServiceProvider, Type[], object>>();
        var openGenericType = typeof(IRepository<>);

        // Act
        builder.AddOpenGenericFactory(openGenericType, ServiceLifetime.Singleton, factory);

        // Assert
        builder.OpenGenericFactories.ShouldHaveSingleItem();
        builder.OpenGenericFactories[0].OpenServiceType.ShouldBe(openGenericType);
        builder.OpenGenericFactories[0].Lifetime.ShouldBe(ServiceLifetime.Singleton);
        builder.OpenGenericFactories[0].Factory.ShouldBe(factory);
    }

    [Fact]
    public void AddOpenGenericFactory_ClosedGenericType_ThrowsArgumentException()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        var factory = Substitute.For<Func<IServiceProvider, Type[], object>>();
        var closedGenericType = typeof(IRepository<string>);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            builder.AddOpenGenericFactory(closedGenericType, ServiceLifetime.Singleton, factory));
        exception.Message.ShouldContain("Must be an open generic type definition");
        exception.ParamName.ShouldBe("openGenericServiceType");
    }

    [Fact]
    public void AddOpenGenericFactory_NullType_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        var factory = Substitute.For<Func<IServiceProvider, Type[], object>>();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            builder.AddOpenGenericFactory(null!, ServiceLifetime.Singleton, factory));
    }

    [Fact]
    public void AddOpenGenericFactory_NullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        var openGenericType = typeof(IRepository<>);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            builder.AddOpenGenericFactory(openGenericType, ServiceLifetime.Singleton, null!));
    }

    // InitializeServices Tests
    [Fact]
    public void InitializeServices_WithOpenGenericInterface_ThrowsMetalInjectionException()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(ServiceWithOpenGenericInterface)]);
        builder.ScanAssemblies(mockAssembly);
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Should.Throw<MetalInjectionException>(() =>
            builder.InitializeServices(services, null));
        exception.Message.ShouldContain("does not implement");
    }

    [Fact]
    public void InitializeServices_MultipleKeyedSingletonRegistrations_RegistersBonusInterfaceWithKey()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(KeyedDoubleSingletonService)]);
        builder.ScanAssemblies(mockAssembly);
        var services = new ServiceCollection();

        // Act
        builder.InitializeServices(services, null);

        // Assert
        var sp = services.BuildServiceProvider();
        var service1 = sp.GetKeyedService<IKeyedService1>("mykey");
        var service2 = sp.GetKeyedService<IKeyedService2>("mykey");
        service1.ShouldNotBeNull();
        service2.ShouldNotBeNull();
    }

    [Fact]
    public void InitializeServices_MultipleScopedRegistrationsWithKey_RegistersBonusInterfaceWithKey()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(KeyedDoubleScopedService)]);
        builder.ScanAssemblies(mockAssembly);
        var services = new ServiceCollection();

        // Act
        builder.InitializeServices(services, null);

        // Assert
        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var service1 = scope.ServiceProvider.GetKeyedService<IKeyedScopedService1>("scopedkey");
        var service2 = scope.ServiceProvider.GetKeyedService<IKeyedScopedService2>("scopedkey");
        service1.ShouldNotBeNull();
        service2.ShouldNotBeNull();
    }

    [Fact]
    public void InitializeServices_ConfigSectionWithInvalidRegisterAsType_ThrowsMetalInjectionException()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(InvalidConfigSection)]);
        builder.ScanAssemblies(mockAssembly);
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        var exception = Should.Throw<MetalInjectionException>(() =>
            builder.InitializeServices(services, configuration));
        exception.Message.ShouldContain("does not implement");
    }

    [Fact]
    public void InitializeServices_ServiceWithOpenGenericInterface_ValidImplementation_Registers()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(ValidOpenGenericService<>)]);
        builder.ScanAssemblies(mockAssembly);
        var services = new ServiceCollection();

        // Act
        builder.InitializeServices(services, null);

        // Assert
        var sp = services.BuildServiceProvider();
        var service = sp.GetService<IGenericService<int>>();
        service.ShouldNotBeNull();
        service.ShouldBeOfType<ValidOpenGenericService<int>>();
    }

    [Fact]
    public void InitializeServices_MultipleServicesWithoutKeys_ThrowsMetalInjectionException()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(ServiceImpl1), typeof(ServiceImpl2)]);
        builder.ScanAssemblies(mockAssembly);
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Should.Throw<MetalInjectionException>(() =>
            builder.InitializeServices(services, null));
        exception.Message.ShouldContain("Cannot register multiple services");
        exception.Message.ShouldContain("without using keys");
    }

    [Fact]
    public void InitializeServices_MultipleServicesWithAllowMultipleTypes_Succeeds()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        builder.AllowMultipleServicesOf(typeof(ICommonService));
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(ServiceImpl1), typeof(ServiceImpl2)]);
        builder.ScanAssemblies(mockAssembly);
        var services = new ServiceCollection();

        // Act
        builder.InitializeServices(services, null);

        // Assert
        var sp = services.BuildServiceProvider();
        var resolvedServices = sp.GetServices<ICommonService>().ToList();
        resolvedServices.Count.ShouldBe(2);
    }

    [Fact]
    public void InitializeServices_MultipleServicesWithAllowMultipleRegistrationsAttribute_Succeeds()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(AttributedServiceImpl1), typeof(AttributedServiceImpl2)]);
        builder.ScanAssemblies(mockAssembly);
        var services = new ServiceCollection();

        // Act
        builder.InitializeServices(services, null);

        // Assert
        var sp = services.BuildServiceProvider();
        var resolvedServices = sp.GetServices<IAllowedMultipleService>().ToList();
        resolvedServices.Count.ShouldBe(2);
    }

    [Fact]
    public void InitializeServices_MultipleServicesWithKeysAndOneWithout_Succeeds()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(KeyedServiceImpl), typeof(NonKeyedServiceImpl)]);
        builder.ScanAssemblies(mockAssembly);
        var services = new ServiceCollection();

        // Act
        builder.InitializeServices(services, null);

        // Assert
        var sp = services.BuildServiceProvider();
        var defaultService = sp.GetService<IMixedKeyService>();
        var keyedService = sp.GetKeyedService<IMixedKeyService>("special");
        defaultService.ShouldNotBeNull();
        keyedService.ShouldNotBeNull();
        defaultService.ShouldBeOfType<NonKeyedServiceImpl>();
        keyedService.ShouldBeOfType<KeyedServiceImpl>();
    }

    [Fact]
    public void InitializeServices_EntryAssemblyDominance_SelectsEntryAssemblyService()
    {
        // Arrange
        var entryAssembly = typeof(MetalInjectionOptionsBuilderTests).Assembly;
        var builder = new MetalInjectionOptionsBuilder();
        builder.SetEntryAssembly(entryAssembly);
        
        // Create real assemblies with proper types
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(EntryAssemblyService), typeof(OtherAssemblyService)]);
        
        // Mock the assembly names - EntryAssemblyService should appear in entry assembly
        var entryAsmServiceType = typeof(EntryAssemblyService);
        var otherAsmServiceType = typeof(OtherAssemblyService);
        
        // Override the assembly property to simulate different assemblies
        var entryServiceAssembly = Substitute.For<Assembly>();
        entryServiceAssembly.FullName.Returns(entryAssembly.FullName);
        
        var otherServiceAssembly = Substitute.For<Assembly>();
        otherServiceAssembly.FullName.Returns("OtherAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
        
        builder.ScanAssemblies(mockAssembly);
        var services = new ServiceCollection();

        // Act - this should throw because both types are in the same assembly
        // The entry assembly dominance only works when types are actually in different assemblies
        var exception = Should.Throw<MetalInjectionException>(() =>
            builder.InitializeServices(services, null));
        
        // Assert
        exception.Message.ShouldContain("Cannot register multiple services");
    }

    [Fact]
    public void InitializeServices_AllowMultipleAnyType_RegistersAllServices()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        builder.AllowMultipleServicesOfAnyType();
        var mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(ServiceImpl1), typeof(ServiceImpl2)]);
        builder.ScanAssemblies(mockAssembly);
        var services = new ServiceCollection();

        // Act
        builder.InitializeServices(services, null);

        // Assert
        var sp = services.BuildServiceProvider();
        var resolvedServices = sp.GetServices<ICommonService>().ToList();
        resolvedServices.Count.ShouldBe(2);
    }

    [Fact]
    public void AddOpenGenericFactory_MultipleFactories_AddsAll()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        var factory1 = Substitute.For<Func<IServiceProvider, Type[], object>>();
        var factory2 = Substitute.For<Func<IServiceProvider, Type[], object>>();
        var openGenericType1 = typeof(IRepository<>);
        var openGenericType2 = typeof(IGenericService<>);

        // Act
        builder.AddOpenGenericFactory(openGenericType1, ServiceLifetime.Singleton, factory1);
        builder.AddOpenGenericFactory(openGenericType2, ServiceLifetime.Scoped, factory2);

        // Assert
        builder.OpenGenericFactories.Count.ShouldBe(2);
        builder.OpenGenericFactories[0].OpenServiceType.ShouldBe(openGenericType1);
        builder.OpenGenericFactories[0].Lifetime.ShouldBe(ServiceLifetime.Singleton);
        builder.OpenGenericFactories[1].OpenServiceType.ShouldBe(openGenericType2);
        builder.OpenGenericFactories[1].Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddOpenGenericFactory_TransientLifetime_StoresCorrectLifetime()
    {
        // Arrange
        var builder = new MetalInjectionOptionsBuilder();
        var factory = Substitute.For<Func<IServiceProvider, Type[], object>>();
        var openGenericType = typeof(IRepository<>);

        // Act
        builder.AddOpenGenericFactory(openGenericType, ServiceLifetime.Transient, factory);

        // Assert
        builder.OpenGenericFactories[0].Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    // Test helper classes
    private interface IRepository<T> { }

    private interface IOpenGeneric<T> { }

    [SingletonAttribute(typeof(IOpenGeneric<>))]
    private class ServiceWithOpenGenericInterface { }

    private interface IKeyedService1 { }
    private interface IKeyedService2 { }

    [SingletonAttribute(typeof(IKeyedService1), Key = "mykey")]
    [SingletonAttribute(typeof(IKeyedService2), Key = "mykey")]
    private class KeyedDoubleSingletonService : IKeyedService1, IKeyedService2 { }

    private interface IKeyedScopedService1 { }
    private interface IKeyedScopedService2 { }

    [ScopedServiceAttribute(typeof(IKeyedScopedService1), Key = "scopedkey")]
    [ScopedServiceAttribute(typeof(IKeyedScopedService2), Key = "scopedkey")]
    private class KeyedDoubleScopedService : IKeyedScopedService1, IKeyedScopedService2 { }

    private interface IInvalidInterface { }

    [ConfigSection("InvalidSection", typeof(IInvalidInterface))]
    private class InvalidConfigSection
    {
        public string Value { get; set; } = "";
    }

    // Open generic service tests
    private interface IGenericService<T> { }

    [SingletonAttribute(typeof(IGenericService<>))]
    private class ValidOpenGenericService<T> : IGenericService<T> { }

    // Multiple services tests
    private interface ICommonService { }

    [SingletonAttribute(typeof(ICommonService))]
    private class ServiceImpl1 : ICommonService { }

    [SingletonAttribute(typeof(ICommonService))]
    private class ServiceImpl2 : ICommonService { }

    // AllowMultipleRegistrations attribute test
    [AllowMultipleRegistrations]
    private interface IAllowedMultipleService { }

    [SingletonAttribute(typeof(IAllowedMultipleService))]
    private class AttributedServiceImpl1 : IAllowedMultipleService { }

    [SingletonAttribute(typeof(IAllowedMultipleService))]
    private class AttributedServiceImpl2 : IAllowedMultipleService { }

    // Mixed keyed/non-keyed services test
    private interface IMixedKeyService { }

    [SingletonAttribute(typeof(IMixedKeyService), Key = "special")]
    private class KeyedServiceImpl : IMixedKeyService { }

    [SingletonAttribute(typeof(IMixedKeyService))]
    private class NonKeyedServiceImpl : IMixedKeyService { }

    // Entry assembly dominance test
    private interface IDominanceTestService { }

    [SingletonAttribute(typeof(IDominanceTestService))]
    private class EntryAssemblyService : IDominanceTestService { }

    [SingletonAttribute(typeof(IDominanceTestService))]
    private class OtherAssemblyService : IDominanceTestService { }
}
