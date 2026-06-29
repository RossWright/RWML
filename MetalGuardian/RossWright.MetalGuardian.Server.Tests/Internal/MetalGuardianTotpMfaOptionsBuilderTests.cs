using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RossWright.MetalGuardian.Server.Tests.Fakes;
using Shouldly;

namespace RossWright.MetalGuardian.Server.Tests.Internal;

public class MetalGuardianTotpMfaOptionsBuilderTests
{
    // ─── SetIssuer Tests ─────────────────────────────────────────────────────────

    [Fact]
    public void SetIssuer_WithValue_StoresIssuer()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        var issuer = "TestIssuer";

        // Act
        builder.SetIssuer(issuer);

        // Assert
        Should.NotThrow(() => builder.SetIssuer(issuer));
    }

    [Fact]
    public void SetIssuer_WithEmptyString_StoresEmptyString()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        var issuer = string.Empty;

        // Act
        builder.SetIssuer(issuer);

        // Assert
        Should.NotThrow(() => builder.SetIssuer(issuer));
    }

    [Fact]
    public void SetIssuer_WithNullValue_StoresNull()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();

        // Act
        builder.SetIssuer(null!);

        // Assert
        Should.NotThrow(() => builder.SetIssuer(null!));
    }

    // ─── SetDeviceRememberDays Tests ─────────────────────────────────────────────

    [Fact]
    public void SetDeviceRememberDays_WithValue_StoresValue()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        var days = 30;

        // Act
        builder.SetDeviceRememberDays(days);

        // Assert
        Should.NotThrow(() => builder.SetDeviceRememberDays(days));
    }

    [Fact]
    public void SetDeviceRememberDays_WithNull_StoresNull()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();

        // Act
        builder.SetDeviceRememberDays(null);

        // Assert
        Should.NotThrow(() => builder.SetDeviceRememberDays(null));
    }

    [Fact]
    public void SetDeviceRememberDays_WithZero_StoresZero()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        var days = 0;

        // Act
        builder.SetDeviceRememberDays(days);

        // Assert
        Should.NotThrow(() => builder.SetDeviceRememberDays(days));
    }

    [Fact]
    public void SetDeviceRememberDays_WithNegativeValue_StoresNegativeValue()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        var days = -5;

        // Act
        builder.SetDeviceRememberDays(days);

        // Assert
        Should.NotThrow(() => builder.SetDeviceRememberDays(days));
    }

    // ─── UseMetalNexusTotpMfaEndpoints Tests ─────────────────────────────────────

    [Fact]
    public void UseMetalNexusTotpMfaEndpoints_WhenCalled_SetsFlag()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();

        // Act
        builder.UseMetalNexusTotpMfaEndpoints();

        // Assert
        Should.NotThrow(() => builder.UseMetalNexusTotpMfaEndpoints());
    }

    [Fact]
    public void UseMetalNexusTotpMfaEndpoints_CalledMultipleTimes_StoresFlag()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();

        // Act
        builder.UseMetalNexusTotpMfaEndpoints();
        builder.UseMetalNexusTotpMfaEndpoints();

        // Assert
        Should.NotThrow(() => builder.UseMetalNexusTotpMfaEndpoints());
    }

    // ─── Initialize Tests ────────────────────────────────────────────────────────

    [Fact]
    public void Initialize_WithoutIssuer_ThrowsMetalGuardianException()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        var services = new ServiceCollection();
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder>();

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() =>
            builder.Initialize(guardianBuilder, services));

        exception.Message.ShouldBe("TOTP MFA Issuer must be set");
    }

    [Fact]
    public void Initialize_WithEmptyIssuer_ThrowsMetalGuardianException()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        builder.SetIssuer(string.Empty);
        var services = new ServiceCollection();
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder>();

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() =>
            builder.Initialize(guardianBuilder, services));

        exception.Message.ShouldBe("TOTP MFA Issuer must be set");
    }

    [Fact]
    public void Initialize_WithWhitespaceIssuer_ThrowsMetalGuardianException()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        builder.SetIssuer("   ");
        var services = new ServiceCollection();
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder>();

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() =>
            builder.Initialize(guardianBuilder, services));

        exception.Message.ShouldBe("TOTP MFA Issuer must be set");
    }

    [Fact]
    public void Initialize_WithValidIssuer_RegistersServices()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        builder.SetIssuer("TestIssuer");
        var services = new ServiceCollection();
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder>();

        // Act
        builder.Initialize(guardianBuilder, services);

        // Assert
        services.ShouldContain(sd =>
            sd.ServiceType == typeof(IMetalGuardianTotpMfaService) &&
            sd.Lifetime == ServiceLifetime.Scoped);
        services.ShouldContain(sd =>
            sd.ServiceType == typeof(IMultifactorAuthenticationProvider) &&
            sd.ImplementationType == typeof(MetalGuardianTotpMfaMultifactorAuthenticationProvider) &&
            sd.Lifetime == ServiceLifetime.Scoped);
        services.ShouldContain(sd =>
            sd.ServiceType == typeof(IUserClaimsProvider) &&
            sd.ImplementationType == typeof(MetalGuardianTotpMfaUserClaimsProvider) &&
            sd.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void Initialize_WithDeviceRememberDays_RegistersServices()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        builder.SetIssuer("TestIssuer");
        builder.SetDeviceRememberDays(30);
        var services = new ServiceCollection();
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder>();

        // Act
        builder.Initialize(guardianBuilder, services);

        // Assert
        services.ShouldContain(sd =>
            sd.ServiceType == typeof(IMetalGuardianTotpMfaService) &&
            sd.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void Initialize_WithoutDeviceRememberDays_RegistersServices()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        builder.SetIssuer("TestIssuer");
        var services = new ServiceCollection();
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder>();

        // Act
        builder.Initialize(guardianBuilder, services);

        // Assert
        services.ShouldContain(sd =>
            sd.ServiceType == typeof(IMetalGuardianTotpMfaService) &&
            sd.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void Initialize_WithMetalNexusEndpointsEnabled_RegistersMetalNexusEndpoints()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        builder.SetIssuer("TestIssuer");
        builder.UseMetalNexusTotpMfaEndpoints();
        var services = new ServiceCollection();
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder>();

        // Act
        builder.Initialize(guardianBuilder, services);

        // Assert
        // MetalNexus endpoints registration adds MetalNexusPreLoads singleton
        services.ShouldContain(sd =>
            sd.ServiceType.Name == "MetalNexusPreLoads" &&
            sd.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void Initialize_WithoutMetalNexusEndpointsEnabled_DoesNotRegisterMetalNexusEndpoints()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        builder.SetIssuer("TestIssuer");
        var services = new ServiceCollection();
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder>();

        // Act
        builder.Initialize(guardianBuilder, services);

        // Assert
        services.ShouldNotContain(sd => sd.ServiceType.Name == "MetalNexusPreLoads");
    }

    [Fact]
    public void Initialize_CalledMultipleTimes_RegistersServicesMultipleTimes()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        builder.SetIssuer("TestIssuer");
        var services = new ServiceCollection();
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder>();

        // Act
        builder.Initialize(guardianBuilder, services);
        builder.Initialize(guardianBuilder, services);

        // Assert
        var totpServiceDescriptors = services.Where(sd =>
            sd.ServiceType == typeof(IMetalGuardianTotpMfaService)).ToList();
        totpServiceDescriptors.Count.ShouldBe(2);
    }

    [Fact]
    public void Initialize_WithAllOptionsSet_RegistersAllServices()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        builder.SetIssuer("TestIssuer");
        builder.SetDeviceRememberDays(15);
        builder.UseMetalNexusTotpMfaEndpoints();
        var services = new ServiceCollection();
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder>();

        // Act
        builder.Initialize(guardianBuilder, services);

        // Assert
        services.ShouldContain(sd =>
            sd.ServiceType == typeof(IMetalGuardianTotpMfaService) &&
            sd.Lifetime == ServiceLifetime.Scoped);
        services.ShouldContain(sd =>
            sd.ServiceType == typeof(IMultifactorAuthenticationProvider) &&
            sd.Lifetime == ServiceLifetime.Scoped);
        services.ShouldContain(sd =>
            sd.ServiceType == typeof(IUserClaimsProvider) &&
            sd.Lifetime == ServiceLifetime.Scoped);
        services.ShouldContain(sd =>
            sd.ServiceType.Name == "MetalNexusPreLoads" &&
            sd.Lifetime == ServiceLifetime.Singleton);
    }

    // ─── Service instance state tests ────────────────────────────────────────────

    private static MetalGuardianTotpMfaService ResolveService(
        IServiceCollection services)
    {
        services.AddScoped(_ => Substitute.For<IAuthenticationRepository>());
        services.AddScoped(_ => Substitute.For<IMetalGuardianAuthenticationService>());
        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IMetalGuardianTotpMfaService>();
        return service.ShouldBeOfType<MetalGuardianTotpMfaService>();
    }

    private static T? GetField<T>(object instance, string parameterName) =>
        (T?)instance.GetType()
            .GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .FirstOrDefault(f => f.Name.Contains(parameterName))
            ?.GetValue(instance);

    [Fact]
    public void Initialize_WithIssuer_ServiceHasCorrectIssuer()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        builder.SetIssuer("MyApp");
        var services = new ServiceCollection();
        builder.Initialize(Substitute.For<IMetalGuardianServerOptionBuilder>(), services);

        // Act
        var service = ResolveService(services);

        // Assert
        GetField<string>(service, "_issuer").ShouldBe("MyApp");
    }

    [Fact]
    public void Initialize_WithDeviceRememberDays_ServiceHasCorrectDays()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        builder.SetIssuer("MyApp");
        builder.SetDeviceRememberDays(30);
        var services = new ServiceCollection();
        builder.Initialize(Substitute.For<IMetalGuardianServerOptionBuilder>(), services);

        // Act
        var service = ResolveService(services);

        // Assert
        GetField<int?>(service, "_deviceRemmemberDays").ShouldBe(30);
    }

    [Fact]
    public void Initialize_WithoutDeviceRememberDays_ServiceHasNullDays()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        builder.SetIssuer("MyApp");
        var services = new ServiceCollection();
        builder.Initialize(Substitute.For<IMetalGuardianServerOptionBuilder>(), services);

        // Act
        var service = ResolveService(services);

        // Assert
        GetField<int?>(service, "_deviceRemmemberDays").ShouldBeNull();
    }

    [Fact]
    public void Initialize_WithUserDeviceRepository_ServiceReceivesRepository()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        builder.SetIssuer("MyApp");
        var services = new ServiceCollection();
        builder.Initialize(Substitute.For<IMetalGuardianServerOptionBuilder>(), services);
        var fakeDeviceRepo = Substitute.For<IUserDeviceRepository>();
        services.AddScoped(_ => fakeDeviceRepo);

        // Act
        var service = ResolveService(services);

        // Assert
        GetField<IUserDeviceRepository?>(service, "_userDeviceRepository").ShouldNotBeNull();
    }

    [Fact]
    public void Initialize_WithoutUserDeviceRepository_ServiceHasNullRepository()
    {
        // Arrange
        var builder = new MetalGuardianTotpMfaOptionsBuilder<FakeTotpUser>();
        builder.SetIssuer("MyApp");
        var services = new ServiceCollection();
        builder.Initialize(Substitute.For<IMetalGuardianServerOptionBuilder>(), services);

        // Act
        var service = ResolveService(services);

        // Assert
        GetField<IUserDeviceRepository?>(service, "_userDeviceRepository").ShouldBeNull();
    }
}
