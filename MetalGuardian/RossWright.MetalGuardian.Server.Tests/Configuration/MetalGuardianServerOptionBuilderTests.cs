using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using RossWright.MetalGuardian;
using RossWright.MetalGuardian.Authentication;
using Shouldly;
using System.Linq.Expressions;

namespace RossWright.MetalGuardian.Server.Tests.Configuration;

public class MetalGuardianServerOptionBuilderTests
{
    [Fact]
    public void UseJwtConfigurationSection_WithSectionName_StoresConfigurationSectionName()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var sectionName = "CustomSection";

        // Act
        builder.UseJwtConfigurationSection(sectionName);

        // Assert
        Should.NotThrow(() => builder.UseJwtConfigurationSection(sectionName));
    }

    [Fact]
    public void UseMetalNexusAuthenticationEndpoints_SetsFlag()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act
        builder.UseMetalNexusAuthenticationEndpoints();

        // Assert
        Should.NotThrow(() => builder.UseMetalNexusAuthenticationEndpoints());
    }

    [Fact]
    public void UseAuthenticationRepository_WithValidRepository_SetsRepositoryType()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Assert
        Should.NotThrow(() => builder.UseAuthenticationRepository<TestAuthenticationRepository>());
    }

    [Fact]
    public void UseAuthenticationRepository_AfterMapDatabaseAuthentication_ThrowsException()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.MapDatabaseAuthentication<TestDbContext, TestUser, TestRefreshToken>(
            identity => u => u.Name == identity);

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() =>
            builder.UseAuthenticationRepository<TestAuthenticationRepository>());

        exception.Message.ShouldBe("You may only call one of: UseAuthenticationRepository, " +
            "MapDatabaseAuthentication or MapDatabaseAuthenticationWithDevices");
    }

    [Fact]
    public void MapDatabaseAuthentication_WithValidParameters_CreatesFactory()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        Func<string, Expression<Func<TestUser, bool>>> predicate = identity => u => u.Name == identity;

        // Act
        builder.MapDatabaseAuthentication<TestDbContext, TestUser, TestRefreshToken>(predicate);

        // Assert
        Should.NotThrow(() =>
            builder.MapDatabaseAuthentication<TestDbContext, TestUser, TestRefreshToken>(predicate));
    }

    [Fact]
    public void MapDatabaseAuthentication_AfterUseAuthenticationRepository_ThrowsException()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() =>
            builder.MapDatabaseAuthentication<TestDbContext, TestUser, TestRefreshToken>(
                identity => u => u.Name == identity));

        exception.Message.ShouldBe("You may only call one of: UseAuthenticationRepository, " +
            "MapDatabaseAuthentication or MapDatabaseAuthenticationWithDevices");
    }

    [Fact]
    public void UseUserDeviceRepository_WithValidRepository_SetsRepositoryType()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act
        builder.UseUserDeviceRepository<TestUserDeviceRepository>();

        // Assert
        Should.NotThrow(() => builder.UseUserDeviceRepository<TestUserDeviceRepository>());
    }

    [Fact]
    public void UseUserDeviceRepository_AfterMapDatabaseAuthenticationWithDevices_ThrowsException()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDevices, TestUser, TestRefreshToken, TestUserDevice>(
            identity => u => u.Name == identity);

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() =>
            builder.UseUserDeviceRepository<TestUserDeviceRepository>());

        exception.Message.ShouldBe("You may only call one of: UseUserDeviceRepository or MapDatabaseAuthenticationWithDevices");
    }

    // Test helper types
    public abstract class TestDbContext : DbContext, IMetalGuardianDbContext<TestUser, TestRefreshToken>
    {
        public abstract DbSet<TestUser> Users { get; }
        public abstract DbSet<TestRefreshToken> RefreshTokens { get; }
    }

    public class TestUser : IAuthenticationUser
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDisabled { get; set; }
        public string PasswordSalt { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }

    public class TestRefreshToken : IRefreshToken
    {
        public Guid UserId { get; set; }
        public IAuthenticationUser User { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresOn { get; set; }
        public DateTime LastSeen { get; set; }
    }

    public class TestAuthenticationRepository : IAuthenticationRepository
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

    public class TestUserDeviceRepository : IUserDeviceRepository
    {
        public Task Add(Action<IUserDevice> setProperties, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<IUserDevice?> Get(Guid userId, string fingerprint, CancellationToken cancellationToken) =>
            Task.FromResult<IUserDevice?>(null);

        public Task Update(Guid userId, string fingerprint, Action<IUserDevice> setProperties, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    public class TestUserDevice : IUserDevice
    {
        public Guid UserId { get; set; }
        public IAuthenticationUser User { get; set; } = null!;
        public string Fingerprint { get; set; } = string.Empty;
        public DateTime? ExpiresOn { get; set; }
        public DateTime LastSeen { get; set; }
    }

    public abstract class TestDbContextWithDevices : DbContext, IMetalGuardianDbContext<TestUser, TestRefreshToken, TestUserDevice>
    {
        public abstract DbSet<TestUser> Users { get; }
        public abstract DbSet<TestRefreshToken> RefreshTokens { get; }
        public abstract DbSet<TestUserDevice> UserDevices { get; }
    }

    [Fact]
    public void MapDatabaseAuthenticationWithDevices_WithValidParameters_CreatesFactory()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        Func<string, Expression<Func<TestUser, bool>>> predicate = identity => u => u.Name == identity;

        // Act
        builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDevices, TestUser, TestRefreshToken, TestUserDevice>(predicate);

        // Assert
        Should.NotThrow(() =>
            builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDevices, TestUser, TestRefreshToken, TestUserDevice>(predicate));
    }

    [Fact]
    public void MapDatabaseAuthenticationWithDevices_AfterUseAuthenticationRepository_ThrowsException()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() =>
            builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDevices, TestUser, TestRefreshToken, TestUserDevice>(
                identity => u => u.Name == identity));

        exception.Message.ShouldBe("You may only call one of: UseAuthenticationRepository, " +
            "MapDatabaseAuthentication or MapDatabaseAuthenticationWithDevices");
    }

    [Fact]
    public void MapDatabaseAuthenticationWithDevices_AfterUseUserDeviceRepository_ThrowsException()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.UseUserDeviceRepository<TestUserDeviceRepository>();

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() =>
            builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDevices, TestUser, TestRefreshToken, TestUserDevice>(
                identity => u => u.Name == identity));

        exception.Message.ShouldBe("You may only call one of: UseUserDeviceRepository or MapDatabaseAuthenticationWithDevices");
    }

    [Fact]
    public void UseUserClaimsProvider_WithValidProvider_AddsProviderType()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act
        builder.UseUserClaimsProvider<TestUserClaimsProvider>();

        // Assert
        Should.NotThrow(() => builder.UseUserClaimsProvider<TestUserClaimsProvider>());
    }

    [Fact]
    public void AddUserClaimMapping_FirstCall_CreatesProviderAndAddsMapping()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var claimName = "role";
        Func<TestUser, string?> getValue = user => user.Name;

        // Act
        builder.AddUserClaimMapping(claimName, getValue);

        // Assert
        Should.NotThrow(() => builder.AddUserClaimMapping<TestUser>("another", u => u.Name));
    }

    [Fact]
    public void AddUserClaimMapping_WithSameTUser_AddsAnotherMapping()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.AddUserClaimMapping<TestUser>("role", user => user.Name);

        // Act & Assert
        Should.NotThrow(() => builder.AddUserClaimMapping<TestUser>("email", user => user.PasswordSalt));
    }

    [Fact]
    public void AddUserClaimMapping_WithDifferentTUser_ThrowsException()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.AddUserClaimMapping<TestUser>("role", user => user.Name);

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() =>
            builder.AddUserClaimMapping<AlternateTestUser>("role", user => user.Name));

        exception.Message.ShouldContain("Only one user data model can be used");
        exception.Message.ShouldContain(nameof(AlternateTestUser));
        exception.Message.ShouldContain(nameof(TestUser));
    }

    [Fact]
    public void AddUserClaimsArrayMapping_FirstCall_CreatesProviderAndAddsMapping()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var claimName = "roles";
        Func<TestUser, string[]> getValues = user => new[] { "admin", "user" };

        // Act
        builder.AddUserClaimsArrayMapping(claimName, getValues);

        // Assert
        Should.NotThrow(() => builder.AddUserClaimsArrayMapping<TestUser>("groups", u => new[] { "group1" }));
    }

    [Fact]
    public void AddUserClaimsArrayMapping_WithSameTUser_AddsAnotherMapping()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.AddUserClaimsArrayMapping<TestUser>("roles", user => new[] { "admin" });

        // Act & Assert
        Should.NotThrow(() => builder.AddUserClaimsArrayMapping<TestUser>("groups", user => new[] { "group1" }));
    }

    [Fact]
    public void AddUserClaimsArrayMapping_WithDifferentTUser_ThrowsException()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.AddUserClaimsArrayMapping<TestUser>("roles", user => new[] { "admin" });

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() =>
            builder.AddUserClaimsArrayMapping<AlternateTestUser>("roles", user => new[] { "admin" }));

        exception.Message.ShouldContain("Only one user data model can be used");
        exception.Message.ShouldContain(nameof(AlternateTestUser));
        exception.Message.ShouldContain(nameof(TestUser));
    }

    [Fact]
    public async Task SimpleUserClaimsProviderImpl_GetClaims_WithNoClaims_ReturnsNull()
    {
        // Arrange
        var provider = new MetalGuardianServerOptionBuilder.SimpleUserClaimsProviderImpl<TestUser>();
        var user = new TestUser { Name = "testuser" };

        // Act
        var result = await provider.GetClaims(user, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task SimpleUserClaimsProviderImpl_GetClaims_WithSingleClaims_ReturnsClaims()
    {
        // Arrange
        var provider = new MetalGuardianServerOptionBuilder.SimpleUserClaimsProviderImpl<TestUser>();
        provider.ClaimFuncs.Add(("role", user => user.Name));
        provider.ClaimFuncs.Add(("email", user => "test@example.com"));
        var user = new TestUser { Name = "testuser" };

        // Act
        var result = await provider.GetClaims(user, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain(c => c.Item1 == "role" && c.Item2 == "testuser");
        result.ShouldContain(c => c.Item1 == "email" && c.Item2 == "test@example.com");
    }

    [Fact]
    public async Task SimpleUserClaimsProviderImpl_GetClaims_FiltersOutNullAndWhitespace()
    {
        // Arrange
        var provider = new MetalGuardianServerOptionBuilder.SimpleUserClaimsProviderImpl<TestUser>();
        provider.ClaimFuncs.Add(("role", user => user.Name));
        provider.ClaimFuncs.Add(("empty", user => null));
        provider.ClaimFuncs.Add(("whitespace", user => "   "));
        var user = new TestUser { Name = "testuser" };

        // Act
        var result = await provider.GetClaims(user, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(1);
        result.ShouldContain(c => c.Item1 == "role" && c.Item2 == "testuser");
    }

    [Fact]
    public async Task SimpleUserClaimsProviderImpl_GetClaims_WithArrayClaims_ReturnsClaims()
    {
        // Arrange
        var provider = new MetalGuardianServerOptionBuilder.SimpleUserClaimsProviderImpl<TestUser>();
        provider.ClaimArrayFuncs.Add(("roles", user => new[] { "admin", "user" }));
        var user = new TestUser { Name = "testuser" };

        // Act
        var result = await provider.GetClaims(user, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(2);
        result.ShouldContain(c => c.Item1 == "roles" && c.Item2 == "admin");
        result.ShouldContain(c => c.Item1 == "roles" && c.Item2 == "user");
    }

    [Fact]
    public async Task SimpleUserClaimsProviderImpl_GetClaims_ArrayClaims_FiltersOutNullAndWhitespace()
    {
        // Arrange
        var provider = new MetalGuardianServerOptionBuilder.SimpleUserClaimsProviderImpl<TestUser>();
        provider.ClaimArrayFuncs.Add(("roles", user => new[] { "admin", "", "   ", "user" }));
        var user = new TestUser { Name = "testuser" };

        // Act
        var result = await provider.GetClaims(user, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(2);
        result.ShouldContain(c => c.Item1 == "roles" && c.Item2 == "admin");
        result.ShouldContain(c => c.Item1 == "roles" && c.Item2 == "user");
    }

    [Fact]
    public async Task SimpleUserClaimsProviderImpl_GetClaims_ArrayClaims_TrimsQuotes()
    {
        // Arrange
        var provider = new MetalGuardianServerOptionBuilder.SimpleUserClaimsProviderImpl<TestUser>();
        provider.ClaimArrayFuncs.Add(("roles", user => new[] { "\"admin\"", "\"user\"" }));
        var user = new TestUser { Name = "testuser" };

        // Act
        var result = await provider.GetClaims(user, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(2);
        result.ShouldContain(c => c.Item1 == "roles" && c.Item2 == "admin");
        result.ShouldContain(c => c.Item1 == "roles" && c.Item2 == "user");
    }

    [Fact]
    public async Task SimpleUserClaimsProviderImpl_GetClaims_CombinesSingleAndArrayClaims()
    {
        // Arrange
        var provider = new MetalGuardianServerOptionBuilder.SimpleUserClaimsProviderImpl<TestUser>();
        provider.ClaimFuncs.Add(("name", user => user.Name));
        provider.ClaimArrayFuncs.Add(("roles", user => new[] { "admin", "user" }));
        var user = new TestUser { Name = "testuser" };

        // Act
        var result = await provider.GetClaims(user, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(3);
        result.ShouldContain(c => c.Item1 == "name" && c.Item2 == "testuser");
        result.ShouldContain(c => c.Item1 == "roles" && c.Item2 == "admin");
        result.ShouldContain(c => c.Item1 == "roles" && c.Item2 == "user");
    }

    // Additional test helper types
    public class AlternateTestUser : IAuthenticationUser
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDisabled { get; set; }
        public string PasswordSalt { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }

    public class TestUserClaimsProvider : IUserClaimsProvider
    {
        public Task<IEnumerable<(string, string)>?> GetClaims(IAuthenticationUser user, CancellationToken cancellationToken) =>
            Task.FromResult<IEnumerable<(string, string)>?>(null);
    }

    [Fact]
    public void UseOneTimePassword_WithoutConfigure_CreatesDefaultOptions()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act
        builder.UseOneTimePassword();

        // Assert
        Should.NotThrow(() => builder.UseOneTimePassword());
    }

    [Fact]
    public void UseOneTimePassword_WithConfigure_CallsConfigureAction()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var configuredDigits = 8;
        var configuredExpiration = 15;
        OneTimePasswordOptions? capturedOptions = null;

        // Act
        builder.UseOneTimePassword(options =>
        {
            capturedOptions = options;
            options.NumberOfDigits = configuredDigits;
            options.ExpirationInMinutes = configuredExpiration;
        });

        // Assert
        capturedOptions.ShouldNotBeNull();
        capturedOptions.NumberOfDigits.ShouldBe(configuredDigits);
        capturedOptions.ExpirationInMinutes.ShouldBe(configuredExpiration);
    }

    [Fact]
    public void UseOneTimePassword_WithNullConfigure_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act & Assert
        Should.NotThrow(() => builder.UseOneTimePassword(null));
    }

    [Fact]
    public void InitializeServer_WithValidConfiguration_RegistersServices()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key-that-is-long-enough"
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        services.Any(sd => sd.ServiceType == typeof(IMetalGuardianServerConfiguration)).ShouldBeTrue();
    }

    [Fact]
    public void InitializeServer_WithConfigurationBinding_BindsFromConfiguration()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configValues = new Dictionary<string, string?>
        {
            ["MetalGuardian:JwtIssuer"] = "issuer-from-config",
            ["MetalGuardian:JwtAudience"] = "audience-from-config",
            ["MetalGuardian:JwtIssuerSigningKey"] = "signing-key-from-config"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
        
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        services.Any(sd => sd.ServiceType == typeof(IMetalGuardianServerConfiguration)).ShouldBeTrue();
    }

    [Fact]
    public void InitializeServer_WithCustomConfigurationSection_BindsFromCustomSection()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var customSectionName = "CustomJwtConfig";
        var configValues = new Dictionary<string, string?>
        {
            [$"{customSectionName}:JwtIssuer"] = "issuer-from-custom",
            [$"{customSectionName}:JwtAudience"] = "audience-from-custom",
            [$"{customSectionName}:JwtIssuerSigningKey"] = "signing-key-from-custom"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
        
        builder.UseJwtConfigurationSection(customSectionName);
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        services.Any(sd => sd.ServiceType == typeof(IMetalGuardianServerConfiguration)).ShouldBeTrue();
    }

    [Fact]
    public void InitializeServer_WithMissingJwtIssuer_ThrowsException()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key"
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() =>
            builder.InitializeServer(services, configuration));
        
        exception.Message.ShouldBe("Invalid configuration MetalGuardian.JwtIssuer");
    }

    [Fact]
    public void InitializeServer_WithMissingJwtAudience_ThrowsException()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = null,
            JwtIssuerSigningKey = "test-signing-key"
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() =>
            builder.InitializeServer(services, configuration));
        
        exception.Message.ShouldBe("Invalid configuration MetalGuardian.JwtAudience");
    }

    [Fact]
    public void InitializeServer_WithMissingJwtIssuerSigningKey_ThrowsException()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "   "
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() =>
            builder.InitializeServer(services, configuration));
        
        exception.Message.ShouldBe("Invalid configuration MetalGuardian.JwtIssuerSigningKey");
    }

    [Fact]
    public void InitializeServer_WithMetalNexusEndpoints_RegistersHandlers()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key"
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.UseMetalNexusAuthenticationEndpoints();
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        services.Any(sd => sd.ServiceType == typeof(IMetalGuardianServerConfiguration)).ShouldBeTrue();
    }

    [Fact]
    public void InitializeServer_WithAuthenticationRepositoryType_RegistersRepository()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key"
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        services.Any(sd => sd.ServiceType == typeof(IAuthenticationRepository) && 
                          sd.ImplementationType == typeof(TestAuthenticationRepository)).ShouldBeTrue();
    }

    [Fact]
    public void InitializeServer_WithAuthenticationRepositoryFactory_RegistersFactory()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key"
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.MapDatabaseAuthentication<TestDbContext, TestUser, TestRefreshToken>(
            identity => u => u.Name == identity);

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        services.Any(sd => sd.ServiceType == typeof(IAuthenticationRepository) && 
                          sd.ImplementationFactory != null).ShouldBeTrue();
    }

    [Fact]
    public void InitializeServer_WithNoAuthenticationRepository_ThrowsException()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key"
        };
        
        builder.UseJwtConfiguration(jwtConfig);

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() =>
            builder.InitializeServer(services, configuration));
        
        exception.Message.ShouldBe("An Authentication repository must be registered. " +
            "In the initialialization of MetalGuardian you must call UseAuthenticationRepository, " +
            "MapDatabaseAuthentication or MapDatabaseAuthenticationWithDevices");
    }

    [Fact]
    public void InitializeServer_WithUserDeviceRepositoryType_RegistersRepository()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key"
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();
        builder.UseUserDeviceRepository<TestUserDeviceRepository>();

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        services.Any(sd => sd.ServiceType == typeof(IUserDeviceRepository) && 
                          sd.ImplementationType == typeof(TestUserDeviceRepository)).ShouldBeTrue();
    }

    [Fact]
    public void InitializeServer_WithUserDeviceRepositoryFactory_RegistersFactory()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key"
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDevices, TestUser, TestRefreshToken, TestUserDevice>(
            identity => u => u.Name == identity);

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        services.Any(sd => sd.ServiceType == typeof(IUserDeviceRepository) && 
                          sd.ImplementationFactory != null).ShouldBeTrue();
    }

    [Fact]
    public void InitializeServer_RegistersAccessTokenFactory()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key"
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        services.Any(sd => sd.ServiceType == typeof(IAccessTokenFactory)).ShouldBeTrue();
    }

    [Fact]
    public void InitializeServer_RegistersAuthenticationService()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key"
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        services.Any(sd => sd.ServiceType == typeof(IMetalGuardianAuthenticationService)).ShouldBeTrue();
    }

    [Fact]
    public void InitializeServer_WithUserClaimsProviders_RegistersProviders()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key"
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();
        builder.UseUserClaimsProvider<TestUserClaimsProvider>();

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        services.Count(sd => sd.ServiceType == typeof(IUserClaimsProvider)).ShouldBeGreaterThan(0);
    }

    [Fact]
    public void InitializeServer_WithSimpleUserClaimsProvider_RegistersProvider()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key"
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();
        builder.AddUserClaimMapping<TestUser>("role", user => user.Name);

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        services.Count(sd => sd.ServiceType == typeof(IUserClaimsProvider)).ShouldBeGreaterThan(0);
    }

    [Fact]
    public void InitializeServer_WithOneTimePassword_RegistersOtpServices()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        services.AddDistributedMemoryCache();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key"
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();
        builder.UseOneTimePassword(options =>
        {
            options.NumberOfDigits = 8;
            options.ExpirationInMinutes = 15;
        });

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        services.Any(sd => sd.ServiceType == typeof(IOtpService)).ShouldBeTrue();
    }

    [Fact]
    public void InitializeServer_RegistersHttpContextAccessor()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key"
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        services.Any(sd => sd.ServiceType == typeof(IHttpContextAccessor)).ShouldBeTrue();
    }

    [Fact]
    public void InitializeServer_RegistersCurrentUser()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key"
        };
        
        builder.UseJwtConfiguration(jwtConfig);
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        services.Any(sd => sd.ServiceType == typeof(ICurrentUser)).ShouldBeTrue();
    }

    // ─── JWT Bearer registration tests ──────────────────────────────────────────

    private static (MetalGuardianServerOptionBuilder builder, IServiceCollection services) BuildJwtServices()
    {
        var builder = new MetalGuardianServerOptionBuilder();
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = Substitute.For<IConfiguration>();
        var jwtConfig = new MetalGuardianServerConfiguration
        {
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            JwtIssuerSigningKey = "test-signing-key-with-enough-length"
        };
        builder.UseJwtConfiguration(jwtConfig);
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();
        builder.InitializeServer(services, configuration);
        return (builder, services);
    }

    [Fact]
    public void InitializeServer_RegistersJwtBearerScheme()
    {
        // Arrange & Act
        var (_, services) = BuildJwtServices();

        // Assert
        services.Any(sd => sd.ServiceType.FullName!.Contains("JwtBearer")).ShouldBeTrue();
    }

    [Fact]
    public async Task JwtBearerEvents_OnMessageReceived_SetsTokenFromQueryString()
    {
        // Arrange
        var (_, services) = BuildJwtServices();
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>
            ().Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?access_token=my-token");
        var messageReceivedContext = new MessageReceivedContext(
            httpContext,
            new Microsoft.AspNetCore.Authentication.AuthenticationScheme(
                JwtBearerDefaults.AuthenticationScheme,
                null,
                typeof(JwtBearerHandler)),
            options);

        // Act
        await options.Events.OnMessageReceived(messageReceivedContext);

        // Assert
        messageReceivedContext.Token.ShouldBe("my-token");
    }

    [Fact]
    public async Task JwtBearerEvents_OnMessageReceived_DoesNotOverrideToken_WhenQueryStringIsAbsent()
    {
        // Arrange
        var (_, services) = BuildJwtServices();
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new DefaultHttpContext();
        // No access_token query param
        var messageReceivedContext = new MessageReceivedContext(
            httpContext,
            new Microsoft.AspNetCore.Authentication.AuthenticationScheme(
                JwtBearerDefaults.AuthenticationScheme,
                null,
                typeof(JwtBearerHandler)),
            options);

        // Act
        await options.Events.OnMessageReceived(messageReceivedContext);

        // Assert
        messageReceivedContext.Token.ShouldBeNull();
    }

    [Fact]
    public async Task JwtBearerEvents_OnAuthenticationFailed_SetsTokenExpiredHeader_WhenSecurityTokenExpired()
    {
        // Arrange
        var (_, services) = BuildJwtServices();
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new DefaultHttpContext();
        var authFailedContext = new AuthenticationFailedContext(
            httpContext,
            new Microsoft.AspNetCore.Authentication.AuthenticationScheme(
                JwtBearerDefaults.AuthenticationScheme,
                null,
                typeof(JwtBearerHandler)),
            options)
        {
            Exception = new SecurityTokenExpiredException()
        };

        // Act
        await options.Events.OnAuthenticationFailed(authFailedContext);

        // Assert
        httpContext.Response.Headers["Token-Expired"].ToString().ShouldBe("true");
    }

    [Fact]
    public async Task JwtBearerEvents_OnAuthenticationFailed_DoesNotSetTokenExpiredHeader_WhenOtherException()
    {
        // Arrange
        var (_, services) = BuildJwtServices();
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new DefaultHttpContext();
        var authFailedContext = new AuthenticationFailedContext(
            httpContext,
            new Microsoft.AspNetCore.Authentication.AuthenticationScheme(
                JwtBearerDefaults.AuthenticationScheme,
                null,
                typeof(JwtBearerHandler)),
            options)
        {
            Exception = new Exception("generic error")
        };

        // Act
        await options.Events.OnAuthenticationFailed(authFailedContext);

        // Assert
        httpContext.Response.Headers.ContainsKey("Token-Expired").ShouldBeFalse();
    }
}
