using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;

namespace RossWright.MetalNexus.Blazor.UnitTests;

public class MetalNexusBlazorExtensionsTests
{
    private static WebAssemblyHostBuilder CreateUninitializedBuilder(string baseAddress = "https://localhost:5000/")
    {
        var builder = (WebAssemblyHostBuilder)RuntimeHelpers.GetUninitializedObject(typeof(WebAssemblyHostBuilder));
        
        // Try to find and set the Services backing field using multiple possible field names
        var serviceCollection = new ServiceCollection();
        var fieldNames = new[] { "_services", "<Services>k__BackingField", "services", "_serviceCollection" };
        foreach (var fieldName in fieldNames)
        {
            var field = typeof(WebAssemblyHostBuilder).GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(builder, serviceCollection);
                break;
            }
        }
        
        // Use reflection to set the HostEnvironment property - try multiple field names
        var hostEnvironment = Substitute.For<IWebAssemblyHostEnvironment>();
        hostEnvironment.BaseAddress.Returns(baseAddress);
        
        var hostEnvFieldNames = new[] { "_hostEnvironment", "<HostEnvironment>k__BackingField", "hostEnvironment" };
        foreach (var fieldName in hostEnvFieldNames)
        {
            var field = typeof(WebAssemblyHostBuilder).GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(builder, hostEnvironment);
                break;
            }
        }
        
        return builder;
    }

    [Fact]
    public void AddMetalNexusClient_CallsServiceExtensionsAndReturnsBuilder()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        var optionsCalled = false;
        Action<IMetalNexusClientOptionsBuilder> buildOptions = _ => optionsCalled = true;

        // Act
        var result = builder.AddMetalNexusClient(buildOptions);

        // Assert
        result.ShouldBe(builder);
        optionsCalled.ShouldBeTrue();
    }

    [Fact]
    public void AddMetalNexusClient_AddsJsScriptLoader_RegistersService()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        Action<IMetalNexusClientOptionsBuilder> buildOptions = _ => { };

        // Act
        builder.AddMetalNexusClient(buildOptions);

        // Assert
        builder.Services.ShouldContain(descriptor => descriptor.ServiceType == typeof(IJsScriptLoaderService));
    }

    [Fact]
    public void AddHttpClient_WithoutConnectionName_CallsOverloadWithDefaultName()
    {
        // Arrange
        var builder = CreateUninitializedBuilder("https://example.com/");

        // Act
        var result = builder.AddHttpClient();

        // Assert
        result.ShouldBe(builder);
        builder.Services.ShouldContain(descriptor => descriptor.ServiceType == typeof(IHttpClientFactory));
    }

    [Fact]
    public void AddHttpClient_WithoutConnectionName_ConfiguresHttpClientWithBaseAddress()
    {
        // Arrange
        var baseAddress = "https://test.example.com/";
        var builder = CreateUninitializedBuilder(baseAddress);

        // Act
        builder.AddHttpClient();

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(Microsoft.Extensions.Options.Options.DefaultName);
        httpClient.BaseAddress.ShouldNotBeNull();
        httpClient.BaseAddress.ToString().ShouldBe(baseAddress);
    }

    [Fact]
    public void AddHttpClient_WithoutConnectionName_CallsConfigureClient()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        var configureClientCalled = false;
        Action<HttpClient> configureClient = _ => configureClientCalled = true;

        // Act
        builder.AddHttpClient(configureClient);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        httpClientFactory.CreateClient(Microsoft.Extensions.Options.Options.DefaultName);
        configureClientCalled.ShouldBeTrue();
    }

    [Fact]
    public void AddHttpClient_WithConnectionName_RegistersHttpClientFactory()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        var connectionName = "TestConnection";

        // Act
        var result = builder.AddHttpClient(connectionName);

        // Assert
        result.ShouldBe(builder);
        builder.Services.ShouldContain(descriptor => descriptor.ServiceType == typeof(IHttpClientFactory));
    }

    [Fact]
    public void AddHttpClient_WithConnectionName_ConfiguresHttpClientWithBaseAddress()
    {
        // Arrange
        var baseAddress = "https://api.example.com/";
        var builder = CreateUninitializedBuilder(baseAddress);
        var connectionName = "ApiClient";

        // Act
        builder.AddHttpClient(connectionName);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(connectionName);
        httpClient.BaseAddress.ShouldNotBeNull();
        httpClient.BaseAddress.ToString().ShouldBe(baseAddress);
    }

    [Fact]
    public void AddHttpClient_WithConnectionName_CallsConfigureClient()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        var connectionName = "CustomClient";
        var configureClientCalled = false;
        Action<HttpClient> configureClient = _ => configureClientCalled = true;

        // Act
        builder.AddHttpClient(connectionName, configureClient);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        httpClientFactory.CreateClient(connectionName);
        configureClientCalled.ShouldBeTrue();
    }

    [Fact]
    public void AddHttpClient_WithConnectionName_NullConfigureClient_SetsBaseAddressOnly()
    {
        // Arrange
        var baseAddress = "https://localhost:8080/";
        var builder = CreateUninitializedBuilder(baseAddress);
        var connectionName = "MinimalClient";

        // Act
        builder.AddHttpClient(connectionName, null);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(connectionName);
        httpClient.BaseAddress.ShouldNotBeNull();
        httpClient.BaseAddress.ToString().ShouldBe(baseAddress);
    }
}
