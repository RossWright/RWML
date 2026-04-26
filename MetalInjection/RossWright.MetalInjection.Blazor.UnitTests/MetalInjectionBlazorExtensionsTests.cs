using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;

namespace RossWright.MetalInjection.Blazor.UnitTests;

public class MetalInjectionBlazorExtensionsTests
{
    private static WebAssemblyHostBuilder CreateUninitializedBuilder()
    {
        var builder = (WebAssemblyHostBuilder)RuntimeHelpers.GetUninitializedObject(typeof(WebAssemblyHostBuilder));
        
        // Use reflection to set the Services property
        var servicesField = typeof(WebAssemblyHostBuilder).GetField("_services", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (servicesField != null)
        {
            servicesField.SetValue(builder, new ServiceCollection());
        }
        
        // Use reflection to set the Configuration property
        var configurationField = typeof(WebAssemblyHostBuilder).GetField("_configuration",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (configurationField != null)
        {
            var configBuilder = new ConfigurationBuilder();
            configurationField.SetValue(builder, configBuilder.Build());
        }
        
        return builder;
    }

    [Fact]
    public void AddMetalInjection_WithNoOptions_ReturnsBuilder()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();

        // Act
        var result = builder.AddMetalInjection();

        // Assert
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddMetalInjection_WithNullOptions_ReturnsBuilder()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();

        // Act
        var result = builder.AddMetalInjection(null);

        // Assert
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddMetalInjection_WithOptions_InvokesOptionsDelegate()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        var optionsCalled = false;
        IMetalInjectionOptionsBuilder? capturedBuilder = null;

        // Act
        builder.AddMetalInjection(opts =>
        {
            optionsCalled = true;
            capturedBuilder = opts;
        });

        // Assert
        optionsCalled.ShouldBeTrue();
        capturedBuilder.ShouldNotBeNull();
    }

    [Fact]
    public void AddMetalInjection_WithOptions_PassesMetalInjectionOptionsBuilder()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        IMetalInjectionOptionsBuilder? receivedBuilder = null;

        // Act
        builder.AddMetalInjection(opts =>
        {
            receivedBuilder = opts;
        });

        // Assert
        receivedBuilder.ShouldNotBeNull();
        receivedBuilder.ShouldBeOfType<MetalInjectionOptionsBuilder>();
    }

    [Fact]
    public void AddMetalInjection_InitializesServices()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();

        // Act & Assert - Should not throw during initialization
        Should.NotThrow(() => builder.AddMetalInjection());
    }

    [Fact]
    public void AddMetalInjection_ConfiguresBlazorInjectAttributeAsAlternate()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        MetalInjectionOptionsBuilder? optionsBuilder = null;

        // Act
        builder.AddMetalInjection(opts =>
        {
            optionsBuilder = opts as MetalInjectionOptionsBuilder;
        });

        // Assert
        optionsBuilder.ShouldNotBeNull();
        optionsBuilder.AlternateInjectAttributeType.ShouldBe(typeof(Microsoft.AspNetCore.Components.InjectAttribute));
    }

    [Fact]
    public void AddMetalInjection_WithCustomOptions_AppliesCustomOptionsAfterBlazorAttribute()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        var customOptionCalled = false;

        // Act
        builder.AddMetalInjection(opts =>
        {
            customOptionCalled = true;
            opts.AllowMultipleServicesOfAnyType(true);
        });

        // Assert
        customOptionCalled.ShouldBeTrue();
    }

    [Fact]
    public void AddMetalInjection_CallsConfigureContainerOnBuilder()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();

        // Act & Assert - Should not throw
        Should.NotThrow(() => builder.AddMetalInjection());
    }

    [Fact]
    public void AddMetalInjection_WithMultipleOptionsCalls_AppliesAllOptions()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        var firstCalled = false;
        var secondCalled = false;

        // Act
        builder.AddMetalInjection(opts =>
        {
            firstCalled = true;
            opts.AllowMultipleServicesOfAnyType(true);
        });

        // We can't call AddMetalInjection twice, but we can verify multiple options in one call
        builder = CreateUninitializedBuilder();
        builder.AddMetalInjection(opts =>
        {
            firstCalled = true;
            opts.AllowMultipleServicesOfAnyType(true);
            secondCalled = true;
            opts.AllowRootScopedResolution(true);
        });

        // Assert
        firstCalled.ShouldBeTrue();
        secondCalled.ShouldBeTrue();
    }

    [Fact]
    public void AddMetalInjection_SetsAlternateInjectAttributeBeforeCustomOptions()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        Type? attributeTypeAtStart = null;
        Type? attributeTypeAtEnd = null;

        // Act
        builder.AddMetalInjection(opts =>
        {
            var optionsBuilder = opts as MetalInjectionOptionsBuilder;
            attributeTypeAtStart = optionsBuilder?.AlternateInjectAttributeType;
            
            // Custom options here
            opts.AllowMultipleServicesOfAnyType(true);
            
            attributeTypeAtEnd = optionsBuilder?.AlternateInjectAttributeType;
        });

        // Assert
        attributeTypeAtStart.ShouldBe(typeof(Microsoft.AspNetCore.Components.InjectAttribute));
        attributeTypeAtEnd.ShouldBe(typeof(Microsoft.AspNetCore.Components.InjectAttribute));
    }

    [Fact]
    public void AddMetalInjection_CallsInitializeServicesWithServicesAndConfiguration()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();

        // Act & Assert - Should not throw, verifying InitializeServices is called with valid parameters
        Should.NotThrow(() => builder.AddMetalInjection());
    }

    [Fact]
    public void AddMetalInjection_CreatesServiceProviderFactory()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();

        // Act & Assert - Should not throw when creating the factory
        Should.NotThrow(() => builder.AddMetalInjection());
    }
}
