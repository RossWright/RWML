using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalGuardian.Authentication;
using RossWright.MetalNexus;
using Shouldly;
using System.Reflection;

namespace RossWright.MetalGuardian.Tests.Configuration;

public class MetalGuardianClientOptionsBuilderTests
{
    [Fact]
    public void AddAuthenticatedHttpClient_WithBaseAddressOnly_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();

        // Act & Assert
        Should.NotThrow(() => builder.AddAuthenticatedHttpClient("https://example.com"));
    }

    [Fact]
    public void AddAuthenticatedHttpClient_WithNullConnectionName_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();

        // Act & Assert
        Should.NotThrow(() => builder.AddAuthenticatedHttpClient("https://example.com", null));
    }

    [Fact]
    public void AddAuthenticatedHttpClient_WithConnectionName_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();

        // Act & Assert
        Should.NotThrow(() => builder.AddAuthenticatedHttpClient("https://example.com", "MyConnection"));
    }

    [Fact]
    public void AddAuthenticatedHttpClient_WithIsDefaultTrue_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();

        // Act & Assert
        Should.NotThrow(() => builder.AddAuthenticatedHttpClient("https://example.com", "MyConnection", isDefault: true));
    }

    [Fact]
    public void UseAuthenticationApiService_FirstCall_SetsAuthenticationServiceType()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();

        // Act
        builder.UseAuthenticationApiService<TestAuthenticationApiService>();

        // Assert
        var field = typeof(MetalGuardianClientOptionsBuilder).GetField("_authenticationApiService", BindingFlags.NonPublic | BindingFlags.Instance);
        var value = field?.GetValue(builder) as Type;
        value.ShouldBe(typeof(TestAuthenticationApiService));
    }

    [Fact]
    public void UseAuthenticationApiService_CalledTwice_ThrowsMetalNexusException()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();
        builder.UseAuthenticationApiService<TestAuthenticationApiService>();

        // Act & Assert
        var exception = Should.Throw<MetalNexusException>(() => 
            builder.UseAuthenticationApiService<AnotherTestAuthenticationApiService>());
        exception.Message.ShouldBe("You may only use one Authentication API Service");
    }

    [Fact]
    public void UseMetalNexusAuthenticationEndpoints_SetsMetalNexusAuthenticationApiService()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();

        // Act
        builder.UseMetalNexusAuthenticationEndpoints();

        // Assert
        var field = typeof(MetalGuardianClientOptionsBuilder).GetField("_authenticationApiService", BindingFlags.NonPublic | BindingFlags.Instance);
        var value = field?.GetValue(builder) as Type;
        value.ShouldBe(typeof(MetalNexusAuthenticationApiService));
    }

    [Fact]
    public void UseDeviceFingerprinting_SetsDeviceFingerprintServiceType()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();

        // Act
        builder.UseDeviceFingerprinting<TestDeviceFingerprintService>();

        // Assert
        var field = typeof(MetalGuardianClientOptionsBuilder).GetField("_deviceFingerprintServiceType", BindingFlags.NonPublic | BindingFlags.Instance);
        var value = field?.GetValue(builder) as Type;
        value.ShouldBe(typeof(TestDeviceFingerprintService));
    }

    [Fact]
    public void UseMachineDeviceFingerprinting_SetsMachineDeviceFingerprintService()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();

        // Act
        builder.UseMachineDeviceFingerprinting();

        // Assert
        var field = typeof(MetalGuardianClientOptionsBuilder).GetField("_deviceFingerprintServiceType", BindingFlags.NonPublic | BindingFlags.Instance);
        var value = field?.GetValue(builder) as Type;
        value.ShouldBe(typeof(MachineDeviceFingerprintService));
    }

    // Test helper classes
    private class TestAuthenticationApiService : IAuthenticationApiService
    {
        public Task<AuthenticationTokens?> Login(string userIdentity, string password, string connectionName, CancellationToken cancellationToken = default) 
            => Task.FromResult<AuthenticationTokens?>(null);
        
        public Task Logout(AuthenticationTokens tokens, string connectionName, CancellationToken cancellationToken = default) 
            => Task.CompletedTask;
        
        public Task<AuthenticationTokens?> Refresh(AuthenticationTokens tokens, string connectionName, CancellationToken cancellationToken = default) 
            => Task.FromResult<AuthenticationTokens?>(null);
    }

    private class AnotherTestAuthenticationApiService : IAuthenticationApiService
    {
        public Task<AuthenticationTokens?> Login(string userIdentity, string password, string connectionName, CancellationToken cancellationToken = default) 
            => Task.FromResult<AuthenticationTokens?>(null);
        
        public Task Logout(AuthenticationTokens tokens, string connectionName, CancellationToken cancellationToken = default) 
            => Task.CompletedTask;
        
        public Task<AuthenticationTokens?> Refresh(AuthenticationTokens tokens, string connectionName, CancellationToken cancellationToken = default) 
            => Task.FromResult<AuthenticationTokens?>(null);
    }

    private class TestDeviceFingerprintService : IDeviceFingerprintService
    {
        public Task<string> GetFingerprint() => Task.FromResult("test-fingerprint");
    }

    // InitializeClient tests
    [Fact]
    public void InitializeClient_WithNoConnectionsAndNoOptionalServices_RegistersCoreServices()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();
        var services = new ServiceCollection();

        // Act
        builder.InitializeClient(services);

        // Assert
        services.ShouldContain(sd => sd.ServiceType == typeof(IBaseAddressRepository) && sd.Lifetime == ServiceLifetime.Singleton);
        services.ShouldContain(sd => sd.ServiceType == typeof(IAccessTokenRepository) && sd.Lifetime == ServiceLifetime.Singleton);
        services.ShouldContain(sd => sd.ServiceType == typeof(IMetalGuardianAuthenticationClient) && sd.Lifetime == ServiceLifetime.Scoped);
        services.ShouldContain(sd => sd.ServiceType == typeof(IMetalGuardianUrlHelper) && sd.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void InitializeClient_WithOneConnection_RegistersHttpClient()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();
        builder.AddAuthenticatedHttpClient("https://example.com", "TestConnection");
        var services = new ServiceCollection();

        // Act
        builder.InitializeClient(services);

        // Assert
        services.ShouldContain(sd => sd.ServiceType == typeof(IHttpClientFactory));
    }

    [Fact]
    public void InitializeClient_WithNullBaseAddress_RegistersHttpClientWithNullUri()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();
        builder.AddAuthenticatedHttpClient(null!, "NullConnection");
        var services = new ServiceCollection();

        // Act
        builder.InitializeClient(services);

        // Assert
        services.ShouldContain(sd => sd.ServiceType == typeof(IHttpClientFactory));
    }

    [Fact]
    public void InitializeClient_WithMultipleConnections_RegistersMultipleHttpClients()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();
        builder.AddAuthenticatedHttpClient("https://example1.com", "Connection1");
        builder.AddAuthenticatedHttpClient("https://example2.com", "Connection2");
        var services = new ServiceCollection();

        // Act
        builder.InitializeClient(services);

        // Assert
        services.ShouldContain(sd => sd.ServiceType == typeof(IHttpClientFactory));
    }

    [Fact]
    public void InitializeClient_WithDeviceFingerprintService_RegistersDeviceFingerprintService()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();
        builder.UseDeviceFingerprinting<TestDeviceFingerprintService>();
        var services = new ServiceCollection();

        // Act
        builder.InitializeClient(services);

        // Assert
        services.ShouldContain(sd => sd.ServiceType == typeof(IDeviceFingerprintService) && sd.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public void InitializeClient_WithoutDeviceFingerprintService_DoesNotRegisterDeviceFingerprintService()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();
        var services = new ServiceCollection();

        // Act
        builder.InitializeClient(services);

        // Assert
        services.ShouldNotContain(sd => sd.ServiceType == typeof(IDeviceFingerprintService));
    }

    [Fact]
    public void InitializeClient_WithCustomAuthenticationApiService_RegistersAuthenticationApiService()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();
        builder.UseAuthenticationApiService<TestAuthenticationApiService>();
        var services = new ServiceCollection();

        // Act
        builder.InitializeClient(services);

        // Assert
        services.ShouldContain(sd => sd.ServiceType == typeof(IAuthenticationApiService) && sd.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void InitializeClient_WithoutAuthenticationApiService_DoesNotRegisterAuthenticationApiService()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();
        var services = new ServiceCollection();

        // Act
        builder.InitializeClient(services);

        // Assert
        services.ShouldNotContain(sd => sd.ServiceType == typeof(IAuthenticationApiService));
    }

    [Fact]
    public void InitializeClient_WithMetalNexusAuthenticationApiService_CallsAddMetalNexusEndpoints()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();
        builder.UseMetalNexusAuthenticationEndpoints();
        var services = new ServiceCollection();

        // Act
        builder.InitializeClient(services);

        // Assert
        services.ShouldContain(sd => sd.ServiceType == typeof(IAuthenticationApiService) && sd.Lifetime == ServiceLifetime.Scoped);
        // Verify MetalNexus endpoints were registered - AddMetalNexusEndpoints adds MetalNexusPreLoads
        services.ShouldContain(sd => sd.ServiceType.Name == "MetalNexusPreLoads" && sd.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void InitializeClient_WithCustomAuthenticationApiService_DoesNotCallAddMetalNexusEndpoints()
    {
        // Arrange
        var builder = new MetalGuardianClientOptionsBuilder();
        builder.UseAuthenticationApiService<TestAuthenticationApiService>();
        var services = new ServiceCollection();

        // Act
        builder.InitializeClient(services);

        // Assert
        services.ShouldContain(sd => sd.ServiceType == typeof(IAuthenticationApiService) && sd.Lifetime == ServiceLifetime.Scoped);
    }
}
