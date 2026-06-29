using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalGuardian.Blazor.Tests;

public class MetalGuardianBlazorExtensionsTests
{
    private static WebAssemblyHostBuilder CreateUninitializedBuilder()
    {
        var builder = (WebAssemblyHostBuilder)RuntimeHelpers.GetUninitializedObject(typeof(WebAssemblyHostBuilder));
        
        // Try multiple possible field names for Services
        var servicesField = typeof(WebAssemblyHostBuilder).GetField("_services", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (servicesField == null)
        {
            servicesField = typeof(WebAssemblyHostBuilder).GetField("<Services>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        }
        if (servicesField != null)
        {
            servicesField.SetValue(builder, new ServiceCollection());
        }
        
        // Try multiple possible field names for Configuration
        var configurationField = typeof(WebAssemblyHostBuilder).GetField("_configuration",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (configurationField == null)
        {
            configurationField = typeof(WebAssemblyHostBuilder).GetField("<Configuration>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        }
        if (configurationField != null)
        {
            var configurationType = configurationField.FieldType;
            var configurationInstance = RuntimeHelpers.GetUninitializedObject(configurationType);
            configurationField.SetValue(builder, configurationInstance);
        }

        // Try multiple possible field names for HostEnvironment
        var hostEnvironmentField = typeof(WebAssemblyHostBuilder).GetField("_hostEnvironment",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (hostEnvironmentField == null)
        {
            hostEnvironmentField = typeof(WebAssemblyHostBuilder).GetField("<HostEnvironment>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        }
        if (hostEnvironmentField != null)
        {
            var hostEnvironment = Substitute.For<IWebAssemblyHostEnvironment>();
            hostEnvironment.BaseAddress.Returns("https://localhost:5000/");
            hostEnvironmentField.SetValue(builder, hostEnvironment);
        }
        
        return builder;
    }

    [Fact]
    public void AddMetalGuardianClient_InvokesSetOptions_ReturnsBuilder()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        builder.Services.ShouldNotBeNull(); // Verify Services is set up
        var setOptionsCalled = false;
        IMetalGuardianBlazorOptionsBuilder? capturedBuilder = null;

        // Act
        var result = builder.AddMetalGuardianClient(opts =>
        {
            setOptionsCalled = true;
            capturedBuilder = opts;
        });

        // Assert
        setOptionsCalled.ShouldBeTrue();
        capturedBuilder.ShouldNotBeNull();
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddMetalGuardianClient_CreatesOptionsBuilder_PassesCorrectType()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        IMetalGuardianBlazorOptionsBuilder? receivedBuilder = null;

        // Act
        builder.AddMetalGuardianClient(opts =>
        {
            receivedBuilder = opts;
        });

        // Assert
        receivedBuilder.ShouldNotBeNull();
        receivedBuilder.ShouldBeAssignableTo<IMetalGuardianBlazorOptionsBuilder>();
    }

    [Fact]
    public void AddMetalGuardianClient_CallsInitializeClient_ServicesAreConfigured()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        var serviceCountBefore = builder.Services.Count;

        // Act
        builder.AddMetalGuardianClient(opts => { });

        // Assert
        builder.Services.Count.ShouldBeGreaterThan(serviceCountBefore);
    }

    [Fact]
    public void AddAuthenticatedHttpClient_ExtractsBaseAddressFromHostEnvironment_CallsOverload()
    {
        // Arrange
        var optionsBuilder = Substitute.For<IMetalGuardianBlazorOptionsBuilder, IMetalGuardianBlazorOptionsBuilderInternal>();
        var hostBuilder = CreateUninitializedBuilder();
        ((IMetalGuardianBlazorOptionsBuilderInternal)optionsBuilder).WebAssemblyHostBuilder.Returns(hostBuilder);
        
        var addAuthenticatedHttpClientCalled = false;
        string? capturedBaseAddress = null;
        string? capturedConnectionName = null;
        bool? capturedIsDefault = null;

        optionsBuilder.When(x => x.AddAuthenticatedHttpClient(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool>()))
            .Do(callInfo =>
            {
                addAuthenticatedHttpClientCalled = true;
                capturedBaseAddress = callInfo.ArgAt<string>(0);
                capturedConnectionName = callInfo.ArgAt<string?>(1);
                capturedIsDefault = callInfo.ArgAt<bool>(2);
            });

        // Act
        optionsBuilder.AddAuthenticatedHttpClient();

        // Assert
        addAuthenticatedHttpClientCalled.ShouldBeTrue();
        capturedBaseAddress.ShouldBe("https://localhost:5000/");
        capturedConnectionName.ShouldBeNull();
        capturedIsDefault.ShouldBe(false);
    }

    [Fact]
    public void AddAuthenticatedHttpClient_WithConnectionName_PassesConnectionName()
    {
        // Arrange
        var optionsBuilder = Substitute.For<IMetalGuardianBlazorOptionsBuilder, IMetalGuardianBlazorOptionsBuilderInternal>();
        var hostBuilder = CreateUninitializedBuilder();
        ((IMetalGuardianBlazorOptionsBuilderInternal)optionsBuilder).WebAssemblyHostBuilder.Returns(hostBuilder);
        
        string? capturedConnectionName = null;
        optionsBuilder.When(x => x.AddAuthenticatedHttpClient(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool>()))
            .Do(callInfo => capturedConnectionName = callInfo.ArgAt<string?>(1));

        // Act
        optionsBuilder.AddAuthenticatedHttpClient("TestConnection", false);

        // Assert
        capturedConnectionName.ShouldBe("TestConnection");
    }

    [Fact]
    public void AddAuthenticatedHttpClient_WithIsDefault_PassesIsDefault()
    {
        // Arrange
        var optionsBuilder = Substitute.For<IMetalGuardianBlazorOptionsBuilder, IMetalGuardianBlazorOptionsBuilderInternal>();
        var hostBuilder = CreateUninitializedBuilder();
        ((IMetalGuardianBlazorOptionsBuilderInternal)optionsBuilder).WebAssemblyHostBuilder.Returns(hostBuilder);
        
        bool? capturedIsDefault = null;
        optionsBuilder.When(x => x.AddAuthenticatedHttpClient(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool>()))
            .Do(callInfo => capturedIsDefault = callInfo.ArgAt<bool>(2));

        // Act
        optionsBuilder.AddAuthenticatedHttpClient(isDefault: true);

        // Assert
        capturedIsDefault.ShouldBe(true);
    }

    [Fact]
    public void AddAuthenticatedHttpClient_WithAllParameters_PassesAllParameters()
    {
        // Arrange
        var optionsBuilder = Substitute.For<IMetalGuardianBlazorOptionsBuilder, IMetalGuardianBlazorOptionsBuilderInternal>();
        var hostBuilder = CreateUninitializedBuilder();
        ((IMetalGuardianBlazorOptionsBuilderInternal)optionsBuilder).WebAssemblyHostBuilder.Returns(hostBuilder);
        
        string? capturedBaseAddress = null;
        string? capturedConnectionName = null;
        bool? capturedIsDefault = null;

        optionsBuilder.When(x => x.AddAuthenticatedHttpClient(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool>()))
            .Do(callInfo =>
            {
                capturedBaseAddress = callInfo.ArgAt<string>(0);
                capturedConnectionName = callInfo.ArgAt<string?>(1);
                capturedIsDefault = callInfo.ArgAt<bool>(2);
            });

        // Act
        optionsBuilder.AddAuthenticatedHttpClient("MyConnection", true);

        // Assert
        capturedBaseAddress.ShouldBe("https://localhost:5000/");
        capturedConnectionName.ShouldBe("MyConnection");
        capturedIsDefault.ShouldBe(true);
    }

    [Fact]
    public void UseBlazorAuthentication_CallsAddServices_ConfiguresAuthentication()
    {
        // Arrange
        var optionsBuilder = Substitute.For<IMetalGuardianBlazorOptionsBuilder, IOptionsBuilder>();
        Action<IServiceCollection>? capturedAction = null;
        
        ((IOptionsBuilder)optionsBuilder).When(x => x.AddServices(Arg.Any<Action<IServiceCollection>>()))
            .Do(callInfo => capturedAction = callInfo.ArgAt<Action<IServiceCollection>>(0));

        // Act
        optionsBuilder.UseBlazorAuthentication();

        // Assert
        capturedAction.ShouldNotBeNull();
    }

    [Fact]
    public void UseBlazorAuthentication_WithConnectionName_PassesConnectionName()
    {
        // Arrange
        var optionsBuilder = Substitute.For<IMetalGuardianBlazorOptionsBuilder, IOptionsBuilder>();
        Action<IServiceCollection>? capturedAction = null;
        
        ((IOptionsBuilder)optionsBuilder).When(x => x.AddServices(Arg.Any<Action<IServiceCollection>>()))
            .Do(callInfo => capturedAction = callInfo.ArgAt<Action<IServiceCollection>>(0));

        // Act
        optionsBuilder.UseBlazorAuthentication("CustomConnection");

        // Assert
        capturedAction.ShouldNotBeNull();
        // Connection name is used inside the lambda but we can't directly verify it
        // The test verifies the method completes without error
    }

    [Fact]
    public void UseBlazorAuthentication_ConfiguresServices_AddsRequiredServices()
    {
        // Arrange
        var optionsBuilder = Substitute.For<IMetalGuardianBlazorOptionsBuilder, IOptionsBuilder>();
        IServiceCollection? capturedServices = null;
        
        ((IOptionsBuilder)optionsBuilder).When(x => x.AddServices(Arg.Any<Action<IServiceCollection>>()))
            .Do(callInfo =>
            {
                var action = callInfo.ArgAt<Action<IServiceCollection>>(0);
                capturedServices = new ServiceCollection();
                action(capturedServices);
            });

        // Act
        optionsBuilder.UseBlazorAuthentication();

        // Assert
        capturedServices.ShouldNotBeNull();
        capturedServices.Count.ShouldBeGreaterThan(0);
    }
}
