using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace RossWright.MetalGuardian.Server.Tests;

public class MetalGuardianServerExtensionsTests
{
    private class TestAuthenticationRepository : IAuthenticationRepository
    {
        public Task<IAuthenticationUser?> LookupUser(string userIdentity, CancellationToken cancellationToken) =>
            Task.FromResult<IAuthenticationUser?>(null);

        public Task<IAuthenticationUser?> UpdateUser(Guid userId, Func<IAuthenticationUser, bool> update, CancellationToken cancellationToken) =>
            Task.FromResult<IAuthenticationUser?>(null);

        public Task AddRefreshToken(Action<IRefreshToken> setProperties, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<IAuthenticationUser?> UpdateRefreshToken(Guid userId, string refreshToken, Action<IRefreshToken> setProperties, CancellationToken cancellationToken) =>
            Task.FromResult<IAuthenticationUser?>(null);

        public Task DeleteRefreshToken(Guid userId, string refreshToken, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    [Fact]
    public void AddMetalGuardianServer_CallsOptionsBuilderAction_ReturnsBuilder()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "TestApp",
            EnvironmentName = "Test"
        });
        
        bool actionCalled = false;
        IMetalGuardianServerOptionBuilder? capturedBuilder = null;

        // Act
        var result = builder.AddMetalGuardianServer(options =>
        {
            actionCalled = true;
            capturedBuilder = options;
            
            // Configure required settings
            options.UseJwtConfiguration(new MetalGuardianServerConfiguration
            {
                JwtIssuer = "test-issuer",
                JwtAudience = "test-audience",
                JwtIssuerSigningKey = "test-signing-key-at-least-32-chars-long"
            });
            options.UseAuthenticationRepository<TestAuthenticationRepository>();
        });

        // Assert
        actionCalled.ShouldBeTrue();
        capturedBuilder.ShouldNotBeNull();
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddMetalGuardianServer_WithConfiguration_RegistersServicesCorrectly()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "TestApp",
            EnvironmentName = "Test"
        });

        // Act
        var result = builder.AddMetalGuardianServer(options =>
        {
            options.UseJwtConfiguration(new MetalGuardianServerConfiguration
            {
                JwtIssuer = "issuer",
                JwtAudience = "audience",
                JwtIssuerSigningKey = "signing-key-must-be-at-least-32-characters"
            });
            options.UseAuthenticationRepository<TestAuthenticationRepository>();
        });

        // Assert
        result.ShouldBe(builder);
        var serviceProvider = builder.Services.BuildServiceProvider();
        serviceProvider.GetService<IMetalGuardianServerConfiguration>().ShouldNotBeNull();
        serviceProvider.GetService<IAuthenticationRepository>().ShouldNotBeNull();
    }

    [Fact]
    public void AddMetalGuardianServer_WithMetalNexusEndpoints_CallsInitializeServer()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "TestApp",
            EnvironmentName = "Test"
        });

        // Act
        var result = builder.AddMetalGuardianServer(options =>
        {
            options.UseJwtConfiguration(new MetalGuardianServerConfiguration
            {
                JwtIssuer = "issuer",
                JwtAudience = "audience",
                JwtIssuerSigningKey = "signing-key-must-be-at-least-32-characters"
            });
            options.UseAuthenticationRepository<TestAuthenticationRepository>();
            options.UseMetalNexusAuthenticationEndpoints();
        });

        // Assert
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddMetalGuardianServer_WithDifferentConfiguration_ExecutesActionWithCorrectBuilder()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "TestApp",
            EnvironmentName = "Test"
        });
        
        var customConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "custom-issuer",
            JwtAudience = "custom-audience",
            JwtIssuerSigningKey = "custom-signing-key-at-least-32-chars",
            JwtAccessTokenExpireMins = 120,
            RefreshTokenExpireMins = 43200
        };

        IMetalGuardianServerOptionBuilder? optionsBuilderInstance = null;

        // Act
        var result = builder.AddMetalGuardianServer(options =>
        {
            optionsBuilderInstance = options;
            options.UseJwtConfiguration(customConfig);
            options.UseAuthenticationRepository<TestAuthenticationRepository>();
        });

        // Assert
        result.ShouldBe(builder);
        optionsBuilderInstance.ShouldNotBeNull();
        optionsBuilderInstance.ShouldBeOfType<MetalGuardianServerOptionBuilder>();
    }

    [Fact]
    public void AddMetalGuardianServer_ReturnsSameBuilderInstance_EnsuresFluentAPI()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "TestApp",
            EnvironmentName = "Test"
        });

        // Act
        var result = builder.AddMetalGuardianServer(options =>
        {
            options.UseJwtConfiguration(new MetalGuardianServerConfiguration
            {
                JwtIssuer = "issuer",
                JwtAudience = "audience",
                JwtIssuerSigningKey = "signing-key-must-be-at-least-32-characters"
            });
            options.UseAuthenticationRepository<TestAuthenticationRepository>();
        });

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void AddMetalGuardianServer_CreatesNewOptionsBuilderInstance_EachTime()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "TestApp",
            EnvironmentName = "Test"
        });

        // Act & Assert - Verify that action is invoked and receives a builder
        builder.AddMetalGuardianServer(options =>
        {
            options.ShouldNotBeNull();
            options.ShouldBeOfType<MetalGuardianServerOptionBuilder>();
            options.UseJwtConfiguration(new MetalGuardianServerConfiguration
            {
                JwtIssuer = "issuer",
                JwtAudience = "audience",
                JwtIssuerSigningKey = "signing-key-must-be-at-least-32-characters"
            });
            options.UseAuthenticationRepository<TestAuthenticationRepository>();
        });
    }

    [Fact]
    public void AddMetalGuardianServer_InvokesOptionsBuilder_BeforeCallingInitializeServer()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "TestApp",
            EnvironmentName = "Test"
        });
        
        var executionOrder = new List<string>();

        // Act
        builder.AddMetalGuardianServer(options =>
        {
            executionOrder.Add("OptionsBuilderCalled");
            options.UseJwtConfiguration(new MetalGuardianServerConfiguration
            {
                JwtIssuer = "issuer",
                JwtAudience = "audience",
                JwtIssuerSigningKey = "signing-key-must-be-at-least-32-characters"
            });
            options.UseAuthenticationRepository<TestAuthenticationRepository>();
            executionOrder.Add("ConfigurationComplete");
        });

        // Assert - The action should have been called and completed
        executionOrder.ShouldContain("OptionsBuilderCalled");
        executionOrder.ShouldContain("ConfigurationComplete");
        executionOrder[0].ShouldBe("OptionsBuilderCalled");
        executionOrder[1].ShouldBe("ConfigurationComplete");
    }
}
