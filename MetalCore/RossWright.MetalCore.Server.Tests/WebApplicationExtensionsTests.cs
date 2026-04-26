using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RossWright.Server.Tests;

public class WebApplicationExtensionsTests
{
    [Fact]
    public void AddServices_WithServiceCollectionAction_CallsActionWithServices()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var serviceAdded = false;

        // Act
        var result = builder.AddServices(services =>
        {
            serviceAdded = true;
            services.AddSingleton<TestService>();
        });

        // Assert
        serviceAdded.ShouldBeTrue();
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddServices_WithServiceCollectionAction_ReturnsBuilder()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.AddServices(services =>
        {
            services.AddSingleton<TestService>();
        });

        // Assert
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddServices_WithServiceCollectionAndConfigurationAction_CallsActionWithServicesAndConfiguration()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        IServiceCollection? receivedServices = null;
        IConfiguration? receivedConfiguration = null;

        // Act
        var result = builder.AddServices((services, config) =>
        {
            receivedServices = services;
            receivedConfiguration = config;
        });

        // Assert
        receivedServices.ShouldBe(builder.Services);
        receivedConfiguration.ShouldBe(builder.Configuration);
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddServices_WithServiceCollectionAndConfigurationAction_ReturnsBuilder()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.AddServices((services, config) =>
        {
            services.AddSingleton<TestService>();
        });

        // Assert
        result.ShouldBe(builder);
    }

    [Fact]
    public void UseApp_WithWebApplicationAction_CallsActionWithApp()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        var actionCalled = false;

        // Act
        var result = app.UseApp(application =>
        {
            actionCalled = true;
        });

        // Assert
        actionCalled.ShouldBeTrue();
        result.ShouldBe(app);
    }

    [Fact]
    public void UseApp_WithWebApplicationAction_ReturnsApp()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act
        var result = app.UseApp(application =>
        {
            // Configuration logic
        });

        // Assert
        result.ShouldBe(app);
    }

    [Fact]
    public void UseApp_WithWebApplicationAndConfigurationAction_CallsActionWithAppAndConfiguration()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        WebApplication? receivedApp = null;
        IConfiguration? receivedConfiguration = null;

        // Act
        var result = app.UseApp((application, config) =>
        {
            receivedApp = application;
            receivedConfiguration = config;
        });

        // Assert
        receivedApp.ShouldBe(app);
        receivedConfiguration.ShouldBe(app.Configuration);
        result.ShouldBe(app);
    }

    [Fact]
    public void UseApp_WithWebApplicationAndConfigurationAction_ReturnsApp()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act
        var result = app.UseApp((application, config) =>
        {
            // Configuration logic
        });

        // Assert
        result.ShouldBe(app);
    }

    private class TestService { }
}
