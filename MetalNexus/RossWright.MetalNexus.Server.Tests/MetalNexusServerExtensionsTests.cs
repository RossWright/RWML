using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using RossWright.MetalChain;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RossWright.MetalNexus.Server.Tests;

public class MetalNexusServerExtensionsTests
{
    [Fact]
    public void AddMetalNexusServer_CallsSetOptions_AndReturnsBuilder()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var wasCalled = false;
        
        // Act
        var result = builder.AddMetalNexusServer(options =>
        {
            wasCalled = true;
        });
        
        // Assert
        wasCalled.ShouldBeTrue();
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddMetalNexusServer_CreatesOptionsBuilder_AndCallsInitializeServer()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        IMetalNexusServerOptionsBuilder? capturedBuilder = null;
        
        // Act
        builder.AddMetalNexusServer(options =>
        {
            capturedBuilder = options;
        });
        
        // Assert
        capturedBuilder.ShouldNotBeNull();
    }

    [Fact]
    public void UseMetalNexus_AddsSecurityDefinition()
    {
        // Arrange
        var options = new SwaggerGenOptions();
        
        // Act
        options.UseMetalNexus();
        
        // Assert
        options.SwaggerGeneratorOptions.SecuritySchemes.ShouldContainKey("MetalGuardian");
        var securityScheme = options.SwaggerGeneratorOptions.SecuritySchemes["MetalGuardian"];
        securityScheme.Type.ShouldBe(SecuritySchemeType.Http);
        securityScheme.Scheme.ShouldBe("bearer");
        securityScheme.BearerFormat.ShouldBe("JWT");
        securityScheme.Description.ShouldBe("Enter your Access Token.");
    }

    [Fact]
    public void UseMetalNexus_AddsDocumentFilter()
    {
        // Arrange
        var options = new SwaggerGenOptions();
        
        // Act
        options.UseMetalNexus();
        
        // Assert
        options.DocumentFilterDescriptors.ShouldContain(d => d.Type.Name == "MetalNexusApiDocumentFilter");
    }

    [Fact]
    public void UseMetalNexus_SetsSchemaGeneratorOptions()
    {
        // Arrange
        var options = new SwaggerGenOptions();
        
        // Act
        options.UseMetalNexus();
        
        // Assert
        options.SchemaGeneratorOptions.ShouldNotBeNull();
        options.SchemaGeneratorOptions.SchemaIdSelector.ShouldNotBeNull();
    }

    [Fact]
    public void UseMetalNexus_SchemaIdSelector_ReplacesPlus()
    {
        // Arrange
        var options = new SwaggerGenOptions();
        options.UseMetalNexus();
        var testType = typeof(OuterClass.InnerClass);
        
        // Act
        var result = options.SchemaGeneratorOptions.SchemaIdSelector(testType);
        
        // Assert
        result.ShouldNotContain("+");
        result.ShouldContain(".");
    }

    [Fact]
    public void UseMetalNexusServer_ThrowsException_WhenMediatorIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var app = Substitute.For<IApplicationBuilder>();
        app.ApplicationServices.Returns(serviceProvider);
        
        // Act & Assert
        var exception = Should.Throw<MetalNexusException>(() => app.UseMetalNexusServer());
        exception.Message.ShouldBe("MetalNexus not added to service collections. Call AddMetalNexusServer prior to UseMetalNexusServer.");
    }

    [Fact]
    public void UseMetalNexusServer_ThrowsException_WhenRegistryIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var mediator = Substitute.For<IMediator>();
        services.AddSingleton(mediator);
        var serviceProvider = services.BuildServiceProvider();
        var app = Substitute.For<IApplicationBuilder>();
        app.ApplicationServices.Returns(serviceProvider);
        
        // Act & Assert
        var exception = Should.Throw<MetalNexusException>(() => app.UseMetalNexusServer());
        exception.Message.ShouldBe("MetalNexus not added to service collections. Call AddMetalNexusServer prior to UseMetalNexusServer.");
    }

    [Fact]
    public void UseMetalNexusServer_ThrowsException_WhenOptionsIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var mediator = Substitute.For<IMediator>();
        var registry = Substitute.For<IMetalNexusRegistry>();
        services.AddSingleton(mediator);
        services.AddSingleton(registry);
        var serviceProvider = services.BuildServiceProvider();
        var app = Substitute.For<IApplicationBuilder>();
        app.ApplicationServices.Returns(serviceProvider);
        
        // Act & Assert
        var exception = Should.Throw<MetalNexusException>(() => app.UseMetalNexusServer());
        exception.Message.ShouldBe("MetalNexus not added to service collections. Call AddMetalNexusServer prior to UseMetalNexusServer.");
    }

    [Fact]
    public void UseMetalNexusServer_Succeeds_WhenAuthServiceNotInstalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var mediator = Substitute.For<IMediator>();
        var registry = Substitute.For<IMetalNexusRegistry>();
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(typeof(TestRequest));
        endpoint.Path.Returns("/test");
        endpoint.HttpMethod.Returns(System.Net.Http.HttpMethod.Get);
        endpoint.HasPathParams.Returns(false);
        
        registry.Endpoints.Returns(new[] { endpoint });
        mediator.HasHandlerFor(Arg.Any<Type>()).Returns(true);
        
        services.AddScoped(_ => mediator);
        services.AddSingleton(mediator);
        services.AddSingleton(registry);
        services.AddSingleton(options);
        
        var builder = WebApplication.CreateBuilder();
        foreach (var service in services)
        {
            builder.Services.Add(service);
        }
        var app = builder.Build();
        
        // Act - should not throw
        app.UseMetalNexusServer();
        
        // Assert - if we get here, it succeeded
        mediator.Received().HasHandlerFor(typeof(TestRequest));
    }

    [Fact]
    public void UseMetalNexusServer_Succeeds_WhenAuthServiceInstalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var mediator = Substitute.For<IMediator>();
        var registry = Substitute.For<IMetalNexusRegistry>();
        var options = Substitute.For<IMetalNexusOptions>();
        var authService = Substitute.For<IAuthenticationService>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(typeof(TestRequest));
        endpoint.Path.Returns("/test");
        endpoint.HttpMethod.Returns(System.Net.Http.HttpMethod.Get);
        endpoint.HasPathParams.Returns(false);
        
        registry.Endpoints.Returns(new[] { endpoint });
        mediator.HasHandlerFor(Arg.Any<Type>()).Returns(true);
        
        services.AddScoped(_ => mediator);
        services.AddScoped(_ => authService);
        services.AddSingleton(mediator);
        services.AddSingleton(registry);
        services.AddSingleton(options);
        
        var builder = WebApplication.CreateBuilder();
        foreach (var service in services)
        {
            builder.Services.Add(service);
        }
        var app = builder.Build();
        
        // Act - should not throw
        app.UseMetalNexusServer();
        
        // Assert - if we get here, it succeeded
        mediator.Received().HasHandlerFor(typeof(TestRequest));
    }

    [Fact]
    public void UseMetalNexusServer_FiltersEndpointsByHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        var mediator = Substitute.For<IMediator>();
        var registry = Substitute.For<IMetalNexusRegistry>();
        var options = Substitute.For<IMetalNexusOptions>();
        
        var endpoint1 = Substitute.For<IEndpoint>();
        endpoint1.RequestType.Returns(typeof(TestRequest));
        endpoint1.Path.Returns("/test1");
        endpoint1.HttpMethod.Returns(System.Net.Http.HttpMethod.Get);
        endpoint1.HasPathParams.Returns(false);
        
        var endpoint2 = Substitute.For<IEndpoint>();
        endpoint2.RequestType.Returns(typeof(TestRequest2));
        endpoint2.Path.Returns("/test2");
        endpoint2.HttpMethod.Returns(System.Net.Http.HttpMethod.Post);
        endpoint2.HasPathParams.Returns(false);
        
        registry.Endpoints.Returns(new[] { endpoint1, endpoint2 });
        mediator.HasHandlerFor(typeof(TestRequest)).Returns(true);
        mediator.HasHandlerFor(typeof(TestRequest2)).Returns(false);
        
        services.AddScoped(_ => mediator);
        services.AddSingleton(mediator);
        services.AddSingleton(registry);
        services.AddSingleton(options);
        
        var builder = WebApplication.CreateBuilder();
        foreach (var service in services)
        {
            builder.Services.Add(service);
        }
        var app = builder.Build();
        
        // Act
        app.UseMetalNexusServer();
        
        // Assert
        mediator.Received(1).HasHandlerFor(typeof(TestRequest));
        mediator.Received(1).HasHandlerFor(typeof(TestRequest2));
    }

    [Fact]
    public void UseMetalNexusServer_CreatesScope_AndUsesMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        var mediator = Substitute.For<IMediator>();
        var registry = Substitute.For<IMetalNexusRegistry>();
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(typeof(TestRequest));
        endpoint.Path.Returns("/test");
        endpoint.HttpMethod.Returns(System.Net.Http.HttpMethod.Get);
        endpoint.HasPathParams.Returns(false);
        
        registry.Endpoints.Returns(new[] { endpoint });
        mediator.HasHandlerFor(Arg.Any<Type>()).Returns(true);
        
        services.AddScoped(_ => mediator);
        services.AddSingleton(mediator);
        services.AddSingleton(registry);
        services.AddSingleton(options);
        
        var builder = WebApplication.CreateBuilder();
        foreach (var service in services)
        {
            builder.Services.Add(service);
        }
        var app = builder.Build();
        
        // Act - should not throw
        app.UseMetalNexusServer();
        
        // Assert - if we get here, the scope was created successfully
        var _ = registry.Received().Endpoints;
        registry.Received(1).Seal();
    }

    // Helper classes for testing
    private class OuterClass
    {
        public class InnerClass
        {
        }
    }

    private class TestRequest : IRequest
    {
    }

    private class TestRequest2 : IRequest
    {
    }
}
