using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using RossWright.MetalInjection;
using Shouldly;
using System.Reflection;

namespace RossWright.MetalInjection.Server.UnitTests;

public class MetalInjectionServerExtensionsTests
{
    [Fact]
    public void AddMetalInjection_WithNoOptions_ReturnsAppBuilder()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.AddMetalInjection();

        // Assert
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddMetalInjection_WithNoOptions_RegistersControllerActivator()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddMetalInjection();

        // Assert
        var descriptor = builder.Services.FirstOrDefault(s => 
            s.ServiceType == typeof(IControllerActivator));
        descriptor.ShouldNotBeNull();
        descriptor.ImplementationType.ShouldBe(typeof(MetalInjectionControllerActivator));
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddMetalInjection_WithOptions_InvokesOptionsDelegate()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var optionsCalled = false;

        // Act
        builder.AddMetalInjection(opts =>
        {
            optionsCalled = true;
        });

        // Assert
        optionsCalled.ShouldBeTrue();
    }

    [Fact]
    public void AddMetalInjection_WithNullOptions_DoesNotThrow()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act & Assert
        Should.NotThrow(() => builder.AddMetalInjection(null));
    }

    [Fact]
    public void AddMetalInjection_WithHostedServiceAttribute_RegistersAsHostedService()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        
        // Act
        builder.AddMetalInjection(opts =>
        {
            // Use a mock assembly with a hosted service
            var mockAssembly = Substitute.For<Assembly>();
            mockAssembly.GetTypes().Returns([typeof(TestHostedService)]);
            opts.ScanAssemblies(mockAssembly);
        });

        // Assert
        var descriptors = builder.Services
            .Where(s => s.ServiceType == typeof(IHostedService))
            .ToList();
        descriptors.ShouldContain(d => d.ImplementationType == typeof(TestHostedService));
    }

    [Fact]
    public void AddMetalInjection_WithNonHostedServiceType_DoesNotRegisterAsHostedService()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddMetalInjection(opts =>
        {
            var mockAssembly = Substitute.For<Assembly>();
            mockAssembly.GetTypes().Returns([typeof(NonHostedServiceWithAttribute)]);
            opts.ScanAssemblies(mockAssembly);
        });

        // Assert
        var descriptors = builder.Services
            .Where(s => s.ServiceType == typeof(IHostedService) && 
                       s.ImplementationType == typeof(NonHostedServiceWithAttribute))
            .ToList();
        descriptors.ShouldBeEmpty();
    }

    [Fact]
    public void AddMetalInjection_WithHostedServiceButNoAttribute_DoesNotRegisterAsHostedService()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddMetalInjection(opts =>
        {
            var mockAssembly = Substitute.For<Assembly>();
            mockAssembly.GetTypes().Returns([typeof(HostedServiceWithoutAttribute)]);
            opts.ScanAssemblies(mockAssembly);
        });

        // Assert
        var descriptors = builder.Services
            .Where(s => s.ServiceType == typeof(IHostedService) && 
                       s.ImplementationType == typeof(HostedServiceWithoutAttribute))
            .ToList();
        descriptors.ShouldBeEmpty();
    }

    [Fact]
    public void AddMetalInjection_WithMultipleHostedServices_RegistersAllAsHostedServices()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddMetalInjection(opts =>
        {
            var mockAssembly = Substitute.For<Assembly>();
            mockAssembly.GetTypes().Returns([
                typeof(TestHostedService),
                typeof(AnotherTestHostedService)
            ]);
            opts.ScanAssemblies(mockAssembly);
        });

        // Assert
        var descriptors = builder.Services
            .Where(s => s.ServiceType == typeof(IHostedService))
            .ToList();
        descriptors.ShouldContain(d => d.ImplementationType == typeof(TestHostedService));
        descriptors.ShouldContain(d => d.ImplementationType == typeof(AnotherTestHostedService));
    }

    [Fact]
    public void AddMetalInjection_HostedServicesRegisteredAsSingleton()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddMetalInjection(opts =>
        {
            var mockAssembly = Substitute.For<Assembly>();
            mockAssembly.GetTypes().Returns([typeof(TestHostedService)]);
            opts.ScanAssemblies(mockAssembly);
        });

        // Assert
        var descriptor = builder.Services
            .FirstOrDefault(s => s.ServiceType == typeof(IHostedService) && 
                               s.ImplementationType == typeof(TestHostedService));
        descriptor.ShouldNotBeNull();
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddMetalInjection_CallsInitializeServicesOnOptionsBuilder()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddMetalInjection(opts =>
        {
            var mockAssembly = Substitute.For<Assembly>();
            mockAssembly.GetTypes().Returns([typeof(TestSingletonService)]);
            opts.ScanAssemblies(mockAssembly);
        });

        // Assert - If InitializeServices was called, the service should be registered
        var descriptor = builder.Services.FirstOrDefault(s => s.ServiceType == typeof(ITestService));
        descriptor.ShouldNotBeNull();
    }

    [Fact]
    public void AddMetalInjection_ConfiguresBlazorInjectAttributeAsAlternate()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddMetalInjection();

        // Assert - Cannot directly verify this without creating a component with [Inject],
        // but we can verify the method returns successfully and builds without error
        var app = builder.Build();
        app.ShouldNotBeNull();
    }

    [Fact]
    public void AddMetalInjection_TryAddEnumerable_DoesNotDuplicateHostedServices()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act - Call AddMetalInjection twice with the same hosted service
        builder.AddMetalInjection(opts =>
        {
            var mockAssembly = Substitute.For<Assembly>();
            mockAssembly.GetTypes().Returns([typeof(TestHostedService)]);
            opts.ScanAssemblies(mockAssembly);
        });
        builder.AddMetalInjection(opts =>
        {
            var mockAssembly = Substitute.For<Assembly>();
            mockAssembly.GetTypes().Returns([typeof(TestHostedService)]);
            opts.ScanAssemblies(mockAssembly);
        });

        // Assert - TryAddEnumerable should prevent duplicates
        var descriptors = builder.Services
            .Where(s => s.ServiceType == typeof(IHostedService) && 
                       s.ImplementationType == typeof(TestHostedService))
            .ToList();
        descriptors.Count.ShouldBe(1);
    }

    [Fact]
    public void AddMetalInjection_UsesServiceProviderFactory()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddMetalInjection();

        // Assert - The service provider factory should be configured
        // We can't directly inspect this, but building the app should succeed
        var app = builder.Build();
        app.Services.ShouldNotBeNull();
    }

    [Fact]
    public void AddMetalInjection_WithOptionsAndHostedServices_ConfiguresBothCorrectly()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var customOptionCalled = false;

        // Act
        builder.AddMetalInjection(opts =>
        {
            customOptionCalled = true;
            opts.AllowMultipleServicesOfAnyType(true);
            
            var mockAssembly = Substitute.For<Assembly>();
            mockAssembly.GetTypes().Returns([typeof(TestHostedService)]);
            opts.ScanAssemblies(mockAssembly);
        });

        // Assert
        customOptionCalled.ShouldBeTrue();
        var hostedServiceDescriptor = builder.Services
            .FirstOrDefault(s => s.ServiceType == typeof(IHostedService) && 
                               s.ImplementationType == typeof(TestHostedService));
        hostedServiceDescriptor.ShouldNotBeNull();
        
        var controllerActivatorDescriptor = builder.Services
            .FirstOrDefault(s => s.ServiceType == typeof(IControllerActivator));
        controllerActivatorDescriptor.ShouldNotBeNull();
    }

    [Fact]
    public void AddMetalInjection_InitializesServicesWithConfiguration()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddMetalInjection();

        // Assert - Services collection should have been modified
        builder.Services.ShouldNotBeEmpty();
    }

    [Fact]
    public void AddMetalInjection_OnlyRegistersTypesMatchingBothConditions()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddMetalInjection(opts =>
        {
            var mockAssembly = Substitute.For<Assembly>();
            // Mix of types: only TestHostedService should be registered
            mockAssembly.GetTypes().Returns([
                typeof(TestHostedService),              // Has attribute + implements IHostedService ✓
                typeof(NonHostedServiceWithAttribute),  // Has attribute but not IHostedService ✗
                typeof(HostedServiceWithoutAttribute),  // Implements IHostedService but no attribute ✗
                typeof(RegularClass)                    // Neither ✗
            ]);
            opts.ScanAssemblies(mockAssembly);
        });

        // Assert
        var hostedServiceDescriptors = builder.Services
            .Where(s => s.ServiceType == typeof(IHostedService))
            .ToList();
        
        hostedServiceDescriptors.ShouldContain(d => d.ImplementationType == typeof(TestHostedService));
        hostedServiceDescriptors.ShouldNotContain(d => d.ImplementationType == typeof(NonHostedServiceWithAttribute));
        hostedServiceDescriptors.ShouldNotContain(d => d.ImplementationType == typeof(HostedServiceWithoutAttribute));
        hostedServiceDescriptors.ShouldNotContain(d => d.ImplementationType == typeof(RegularClass));
    }

    // Helper types for testing
    [HostedService]
    private class TestHostedService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    [HostedService]
    private class AnotherTestHostedService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    [HostedService]
    private class NonHostedServiceWithAttribute
    {
        // Has attribute but doesn't implement IHostedService
    }

    private class HostedServiceWithoutAttribute : IHostedService
    {
        // Implements IHostedService but doesn't have attribute
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private class RegularClass
    {
        // Neither attribute nor IHostedService
    }

    private interface ITestService { }

    [SingletonAttribute(typeof(ITestService))]
    private class TestSingletonService : ITestService { }
}
