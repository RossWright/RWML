using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RossWright.MetalCommand;
using Shouldly;

namespace RossWright.MetalNexus.Tests;

public class MetalNexusExtensionsTests
{
    [Fact]
    public void AddHttpClient_WithConnectionName_CallsHttpClientFactoryExtension()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionName = "TestConnection";
        Action<HttpClient> configureClient = _ => { };

        // Act
        var result = services.AddHttpClient(connectionName, configureClient);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeAssignableTo<IHttpClientBuilder>();
    }

    [Fact]
    public void AddHttpClient_WithoutConnectionName_CallsHttpClientFactoryExtensionWithDefaultName()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<HttpClient> configureClient = _ => { };

        // Act
        var result = services.AddHttpClient(configureClient);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeAssignableTo<IHttpClientBuilder>();
    }

    [Fact]
    public void AddMetalNexusClient_OnConsoleApplicationBuilder_CallsServiceCollectionExtensionAndReturnsBuilder()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();
        var services = new ServiceCollection();
        builder.Services.Returns(services);
        var optionsCalled = false;
        Action<IMetalNexusClientOptionsBuilder> buildOptions = _ => optionsCalled = true;

        // Act
        var result = builder.AddMetalNexusClient(buildOptions);

        // Assert
        result.ShouldBe(builder);
        optionsCalled.ShouldBeTrue();
    }

    [Fact]
    public void AddHttpClient_OnConsoleApplicationBuilder_WithoutConnectionName_CallsHttpClientFactoryExtensionAndReturnsBuilder()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();
        var services = new ServiceCollection();
        builder.Services.Returns(services);
        Action<HttpClient> configureClient = _ => { };

        // Act
        var result = builder.AddHttpClient(configureClient);

        // Assert
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddHttpClient_OnConsoleApplicationBuilder_WithConnectionName_CallsHttpClientFactoryExtensionAndReturnsBuilder()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();
        var services = new ServiceCollection();
        builder.Services.Returns(services);
        var connectionName = "TestConnection";
        Action<HttpClient> configureClient = _ => { };

        // Act
        var result = builder.AddHttpClient(connectionName, configureClient);

        // Assert
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddHttpClient_WithConnectionName_ConfiguresClientCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionName = "TestConnection";
        var configuredClient = false;
        Action<HttpClient> configureClient = client =>
        {
            client.BaseAddress = new Uri("https://example.com");
            configuredClient = true;
        };

        // Act
        var result = services.AddHttpClient(connectionName, configureClient);
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient(connectionName);

        // Assert
        result.ShouldNotBeNull();
        configuredClient.ShouldBeTrue();
        client.BaseAddress.ShouldBe(new Uri("https://example.com"));
    }

    [Fact]
    public void AddHttpClient_WithoutConnectionName_ConfiguresDefaultClientCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuredClient = false;
        Action<HttpClient> configureClient = client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            configuredClient = true;
        };

        // Act
        var result = services.AddHttpClient(configureClient);
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient(Microsoft.Extensions.Options.Options.DefaultName);

        // Assert
        result.ShouldNotBeNull();
        configuredClient.ShouldBeTrue();
        client.Timeout.ShouldBe(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void AddMetalNexusClient_OnConsoleApplicationBuilder_PassesOptionsBuilderToServiceCollection()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();
        var services = new ServiceCollection();
        builder.Services.Returns(services);
        IMetalNexusClientOptionsBuilder? capturedBuilder = null;
        Action<IMetalNexusClientOptionsBuilder> buildOptions = b => capturedBuilder = b;

        // Act
        var result = builder.AddMetalNexusClient(buildOptions);

        // Assert
        result.ShouldBe(builder);
        capturedBuilder.ShouldNotBeNull();
    }

    [Fact]
    public void AddHttpClient_OnConsoleApplicationBuilder_WithoutConnectionName_ConfiguresDefaultClient()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();
        var services = new ServiceCollection();
        builder.Services.Returns(services);
        var configuredClient = false;
        Action<HttpClient> configureClient = client =>
        {
            client.BaseAddress = new Uri("https://api.example.com");
            configuredClient = true;
        };

        // Act
        var result = builder.AddHttpClient(configureClient);
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient(Microsoft.Extensions.Options.Options.DefaultName);

        // Assert
        result.ShouldBe(builder);
        configuredClient.ShouldBeTrue();
        client.BaseAddress.ShouldBe(new Uri("https://api.example.com"));
    }

    [Fact]
    public void AddHttpClient_OnConsoleApplicationBuilder_WithConnectionName_ConfiguresNamedClient()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();
        var services = new ServiceCollection();
        builder.Services.Returns(services);
        var connectionName = "ApiClient";
        var configuredClient = false;
        Action<HttpClient> configureClient = client =>
        {
            client.MaxResponseContentBufferSize = 1024;
            configuredClient = true;
        };

        // Act
        var result = builder.AddHttpClient(connectionName, configureClient);
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient(connectionName);

        // Assert
        result.ShouldBe(builder);
        configuredClient.ShouldBeTrue();
        client.MaxResponseContentBufferSize.ShouldBe(1024);
    }
}
