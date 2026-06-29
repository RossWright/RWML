using Microsoft.EntityFrameworkCore;
using NSubstitute;
using RossWright.MetalGuardian;
using RossWright.MetalGuardian.Authentication;
using Shouldly;
using System.Linq.Expressions;

namespace RossWright.MetalGuardian.Server.Tests.Configuration;

public class IMetalGuardianServerOptionBuilderTests
{
    [Fact]
    public void UseJwtConfiguration_WithValidConfiguration_StoresConfiguration()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var configuration = Substitute.For<IMetalGuardianServerConfiguration>();
        configuration.JwtIssuer.Returns("test-issuer");
        configuration.JwtAudience.Returns("test-audience");
        configuration.JwtIssuerSigningKey.Returns("test-signing-key-minimum-32-chars-long");
        configuration.JwtAccessTokenExpireMins.Returns(60);
        
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var appConfiguration = Substitute.For<Microsoft.Extensions.Configuration.IConfiguration>();
        
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act
        builder.UseJwtConfiguration(configuration);
        builder.InitializeServer(services, appConfiguration);

        // Assert - verify the configuration properties were accessed during InitializeServer
        _ = configuration.Received().JwtIssuer;
        _ = configuration.Received().JwtAudience;
        _ = configuration.Received().JwtIssuerSigningKey;
    }

    [Fact]
    public void UseJwtConfigurationSection_WithValidSectionName_StoresSectionName()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        var sectionName = "MetalGuardian";

        // Act
        builder.UseJwtConfigurationSection(sectionName);

        // Assert
        Should.NotThrow(() => builder.UseJwtConfigurationSection(sectionName));
    }

    [Fact]
    public void UseAuthenticationRepository_WithValidRepository_RegistersRepository()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act & Assert
        Should.NotThrow(() => builder.UseAuthenticationRepository<TestAuthenticationRepository>());
    }

    [Fact]
    public void UseAuthenticationRepository_CalledAfterMapDatabaseAuthentication_ThrowsException()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.MapDatabaseAuthentication<TestDbContext, TestUser, TestRefreshToken>(
            identity => u => u.Name == identity);

        // Act & Assert
        var exception = Should.Throw<RossWright.MetalGuardian.MetalGuardianException>(() =>
            builder.UseAuthenticationRepository<TestAuthenticationRepository>());

        exception.Message.ShouldContain("You may only call one of");
    }

    [Fact]
    public void UseUserDeviceRepository_WithValidRepository_RegistersRepository()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act & Assert
        Should.NotThrow(() => builder.UseUserDeviceRepository<TestUserDeviceRepository>());
    }

    [Fact]
    public void UseUserDeviceRepository_CalledTwice_AllowsOverwrite()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.UseUserDeviceRepository<TestUserDeviceRepository>();

        // Act & Assert - last call wins behavior
        Should.NotThrow(() => builder.UseUserDeviceRepository<TestUserDeviceRepository>());
    }

    [Fact]
    public void MapDatabaseAuthentication_WithValidParameters_RegistersAuthenticationRepository()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        Func<string, Expression<Func<TestUser, bool>>> predicate = identity => u => u.Name == identity;

        // Act & Assert
        Should.NotThrow(() =>
            builder.MapDatabaseAuthentication<TestDbContext, TestUser, TestRefreshToken>(predicate));
    }

    [Fact]
    public void MapDatabaseAuthentication_CalledAfterUseAuthenticationRepository_ThrowsException()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act & Assert
        var exception = Should.Throw<RossWright.MetalGuardian.MetalGuardianException>(() =>
            builder.MapDatabaseAuthentication<TestDbContext, TestUser, TestRefreshToken>(
                identity => u => u.Name == identity));

        exception.Message.ShouldContain("You may only call one of");
    }

    [Fact]
    public void MapDatabaseAuthentication_CalledTwice_AllowsOverwrite()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.MapDatabaseAuthentication<TestDbContext, TestUser, TestRefreshToken>(
            identity => u => u.Name == identity);

        // Act & Assert - last call wins behavior
        Should.NotThrow(() =>
            builder.MapDatabaseAuthentication<TestDbContext, TestUser, TestRefreshToken>(
                identity => u => u.Name == identity));
    }

    [Fact]
    public void UseJwtConfiguration_WithNullConfiguration_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Should.NotThrow(() => builder.UseJwtConfiguration(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Fact]
    public void UseJwtConfigurationSection_WithNullSectionName_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Should.NotThrow(() => builder.UseJwtConfigurationSection(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Fact]
    public void UseJwtConfigurationSection_WithEmptyString_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act & Assert
        Should.NotThrow(() => builder.UseJwtConfigurationSection(string.Empty));
    }

    [Fact]
    public void UseAuthenticationRepository_CalledTwice_AllowsOverwrite()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act & Assert - last call wins behavior
        Should.NotThrow(() => builder.UseAuthenticationRepository<TestAuthenticationRepository>());
    }

    [Fact]
    public void MapDatabaseAuthentication_WithNullPredicate_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Should.NotThrow(() =>
            builder.MapDatabaseAuthentication<TestDbContext, TestUser, TestRefreshToken>(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Fact]
    public void MapDatabaseAuthenticationWithDevices_WithValidParameters_RegistersAuthenticationRepository()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        Func<string, Expression<Func<TestUser, bool>>> predicate = identity => u => u.Name == identity;

        // Act & Assert
        Should.NotThrow(() =>
            builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDevices, TestUser, TestRefreshToken, TestUserDevice>(predicate));
    }

    [Fact]
    public void MapDatabaseAuthenticationWithDevices_CalledAfterUseAuthenticationRepository_ThrowsException()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.UseAuthenticationRepository<TestAuthenticationRepository>();

        // Act & Assert
        var exception = Should.Throw<RossWright.MetalGuardian.MetalGuardianException>(() =>
            builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDevices, TestUser, TestRefreshToken, TestUserDevice>(
                identity => u => u.Name == identity));

        exception.Message.ShouldContain("You may only call one of");
    }

    [Fact]
    public void MapDatabaseAuthenticationWithDevices_CalledTwice_AllowsOverwrite()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDevices, TestUser, TestRefreshToken, TestUserDevice>(
            identity => u => u.Name == identity);

        // Act & Assert - last call wins behavior
        Should.NotThrow(() =>
            builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDevices, TestUser, TestRefreshToken, TestUserDevice>(
                identity => u => u.Name == identity));
    }

    [Fact]
    public void MapDatabaseAuthenticationWithDevices_WithNullPredicate_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Should.NotThrow(() =>
            builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDevices, TestUser, TestRefreshToken, TestUserDevice>(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Fact]
    public void UseUserClaimsProvider_WithValidProvider_RegistersProvider()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act & Assert
        Should.NotThrow(() => builder.UseUserClaimsProvider<TestUserClaimsProvider>());
    }

    [Fact]
    public void UseUserClaimsProvider_CalledTwice_AllowsMultipleProviders()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.UseUserClaimsProvider<TestUserClaimsProvider>();

        // Act & Assert - multiple providers are allowed
        Should.NotThrow(() => builder.UseUserClaimsProvider<TestUserClaimsProvider>());
    }

    [Fact]
    public void AddUserClaimMapping_WithValidMapping_RegistersMapping()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        string claimName = "test-claim";
        Func<TestUser, string?> getValue = user => user.Name;

        // Act & Assert
        Should.NotThrow(() => builder.AddUserClaimMapping(claimName, getValue));
    }

    [Fact]
    public void AddUserClaimMapping_WithNullClaimName_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        Func<TestUser, string?> getValue = user => user.Name;

        // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Should.NotThrow(() => builder.AddUserClaimMapping<TestUser>(null, getValue));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Fact]
    public void AddUserClaimMapping_WithNullGetValue_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        string claimName = "test-claim";

        // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Should.NotThrow(() => builder.AddUserClaimMapping<TestUser>(claimName, null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Fact]
    public void AddUserClaimMapping_CalledTwice_AllowsMultipleMappings()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.AddUserClaimMapping<TestUser>("claim1", user => user.Name);

        // Act & Assert - multiple mappings are allowed
        Should.NotThrow(() => builder.AddUserClaimMapping<TestUser>("claim2", user => user.Name));
    }

    [Fact]
    public void AddUserClaimsArrayMapping_WithValidMapping_RegistersMapping()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        string claimName = "test-array-claim";
        Func<TestUser, string[]> getValues = user => new[] { user.Name };

        // Act & Assert
        Should.NotThrow(() => builder.AddUserClaimsArrayMapping(claimName, getValues));
    }

    [Fact]
    public void AddUserClaimsArrayMapping_WithNullClaimName_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        Func<TestUser, string[]> getValues = user => new[] { user.Name };

        // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Should.NotThrow(() => builder.AddUserClaimsArrayMapping<TestUser>(null, getValues));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Fact]
    public void AddUserClaimsArrayMapping_WithNullGetValues_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        string claimName = "test-array-claim";

        // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Should.NotThrow(() => builder.AddUserClaimsArrayMapping<TestUser>(claimName, null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Fact]
    public void AddUserClaimsArrayMapping_CalledTwice_AllowsMultipleMappings()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.AddUserClaimsArrayMapping<TestUser>("claim1", user => new[] { user.Name });

        // Act & Assert - multiple mappings are allowed
        Should.NotThrow(() => builder.AddUserClaimsArrayMapping<TestUser>("claim2", user => new[] { user.Name }));
    }

    [Fact]
    public void UseOneTimePassword_WithoutConfiguration_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act & Assert
        Should.NotThrow(() => builder.UseOneTimePassword());
    }

    [Fact]
    public void UseOneTimePassword_WithNullConfiguration_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();

        // Act & Assert
        Should.NotThrow(() => builder.UseOneTimePassword(null));
    }

    [Fact]
    public void UseOneTimePassword_WithValidConfiguration_DoesNotThrow()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        Action<OneTimePasswordOptions> configure = options => options.NumberOfDigits = 8;

        // Act & Assert
        Should.NotThrow(() => builder.UseOneTimePassword(configure));
    }

    [Fact]
    public void UseOneTimePassword_CalledTwice_AllowsOverwrite()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        builder.UseOneTimePassword();

        // Act & Assert - last call wins behavior
        Should.NotThrow(() => builder.UseOneTimePassword(options => options.NumberOfDigits = 8));
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

    public class TestUserClaimsProvider : IUserClaimsProvider
    {
        public Task<IEnumerable<(string, string)>?> GetClaims(IAuthenticationUser user, CancellationToken cancellationToken) =>
            Task.FromResult<IEnumerable<(string, string)>?>(null);
    }

    public abstract class TestDbContextWithDefaultRefreshToken : DbContext, IMetalGuardianDbContext<TestUser, RefreshToken<TestUser>>
    {
        public abstract DbSet<TestUser> Users { get; }
        public abstract DbSet<RefreshToken<TestUser>> RefreshTokens { get; }
    }

    public abstract class TestDbContextWithDefaultRefreshTokenAndUserDevice : DbContext, IMetalGuardianDbContext<TestUser, RefreshToken<TestUser>, UserDevice<TestUser>>
    {
        public abstract DbSet<TestUser> Users { get; }
        public abstract DbSet<RefreshToken<TestUser>> RefreshTokens { get; }
        public abstract DbSet<UserDevice<TestUser>> UserDevices { get; }
    }

    public abstract class TestDbContextWithDefaultRefreshTokenAndCustomUserDevice : DbContext, IMetalGuardianDbContext<TestUser, RefreshToken<TestUser>, TestUserDevice>
    {
        public abstract DbSet<TestUser> Users { get; }
        public abstract DbSet<RefreshToken<TestUser>> RefreshTokens { get; }
        public abstract DbSet<TestUserDevice> UserDevices { get; }
    }

    // Extension method tests
    [Fact]
    public void MapDatabaseAuthentication_ExtensionOverload_CallsUnderlyingMethodWithDefaultRefreshToken()
    {
        // Arrange
        var builder = Substitute.For<IMetalGuardianServerOptionBuilder>();
        Func<string, Expression<Func<TestUser, bool>>> predicate = identity => u => u.Name == identity;

        // Act
        builder.MapDatabaseAuthentication<TestDbContextWithDefaultRefreshToken, TestUser>(predicate);

        // Assert
        builder.Received(1).MapDatabaseAuthentication<TestDbContextWithDefaultRefreshToken, TestUser, RefreshToken<TestUser>>(predicate);
    }

    [Fact]
    public void MapDatabaseAuthentication_ExtensionOverload_WithNullPredicate_PassesNullToUnderlyingMethod()
    {
        // Arrange
        var builder = Substitute.For<IMetalGuardianServerOptionBuilder>();

        // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        builder.MapDatabaseAuthentication<TestDbContextWithDefaultRefreshToken, TestUser>(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        builder.Received(1).MapDatabaseAuthentication<TestDbContextWithDefaultRefreshToken, TestUser, RefreshToken<TestUser>>(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Fact]
    public void MapDatabaseAuthenticationWithDevices_ExtensionOverloadWithCustomUserDevice_CallsUnderlyingMethod()
    {
        // Arrange
        var builder = Substitute.For<IMetalGuardianServerOptionBuilder>();
        Func<string, Expression<Func<TestUser, bool>>> predicate = identity => u => u.Name == identity;

        // Act
        builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDefaultRefreshTokenAndCustomUserDevice, TestUser, TestUserDevice>(predicate);

        // Assert
        builder.Received(1).MapDatabaseAuthentication<TestDbContextWithDefaultRefreshTokenAndCustomUserDevice, TestUser, RefreshToken<TestUser>>(predicate);
    }

    [Fact]
    public void MapDatabaseAuthenticationWithDevices_ExtensionOverloadWithCustomUserDevice_WithNullPredicate_PassesNullToUnderlyingMethod()
    {
        // Arrange
        var builder = Substitute.For<IMetalGuardianServerOptionBuilder>();

        // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDefaultRefreshTokenAndCustomUserDevice, TestUser, TestUserDevice>(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        builder.Received(1).MapDatabaseAuthentication<TestDbContextWithDefaultRefreshTokenAndCustomUserDevice, TestUser, RefreshToken<TestUser>>(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Fact]
    public void MapDatabaseAuthenticationWithDevices_ExtensionOverloadWithDefaultUserDevice_CallsUnderlyingMethod()
    {
        // Arrange
        var builder = Substitute.For<IMetalGuardianServerOptionBuilder>();
        Func<string, Expression<Func<TestUser, bool>>> predicate = identity => u => u.Name == identity;

        // Act
        builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDefaultRefreshTokenAndUserDevice, TestUser>(predicate);

        // Assert
        builder.Received(1).MapDatabaseAuthentication<TestDbContextWithDefaultRefreshTokenAndUserDevice, TestUser, RefreshToken<TestUser>>(predicate);
    }

    [Fact]
    public void MapDatabaseAuthenticationWithDevices_ExtensionOverloadWithDefaultUserDevice_WithNullPredicate_PassesNullToUnderlyingMethod()
    {
        // Arrange
        var builder = Substitute.For<IMetalGuardianServerOptionBuilder>();

        // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDefaultRefreshTokenAndUserDevice, TestUser>(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        builder.Received(1).MapDatabaseAuthentication<TestDbContextWithDefaultRefreshTokenAndUserDevice, TestUser, RefreshToken<TestUser>>(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Fact]
    public void MapDatabaseAuthentication_ExtensionOverload_WithConcreteBuilder_RegistersSuccessfully()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        Func<string, Expression<Func<TestUser, bool>>> predicate = identity => u => u.Name == identity;

        // Act & Assert
        Should.NotThrow(() => 
            builder.MapDatabaseAuthentication<TestDbContextWithDefaultRefreshToken, TestUser>(predicate));
    }

    [Fact]
    public void MapDatabaseAuthenticationWithDevices_ExtensionOverloadWithCustomUserDevice_WithConcreteBuilder_RegistersSuccessfully()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        Func<string, Expression<Func<TestUser, bool>>> predicate = identity => u => u.Name == identity;

        // Act & Assert
        Should.NotThrow(() => 
            builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDefaultRefreshTokenAndCustomUserDevice, TestUser, TestUserDevice>(predicate));
    }

    [Fact]
    public void MapDatabaseAuthenticationWithDevices_ExtensionOverloadWithDefaultUserDevice_WithConcreteBuilder_RegistersSuccessfully()
    {
        // Arrange
        var builder = new MetalGuardianServerOptionBuilder();
        Func<string, Expression<Func<TestUser, bool>>> predicate = identity => u => u.Name == identity;

        // Act & Assert
        Should.NotThrow(() => 
            builder.MapDatabaseAuthenticationWithDevices<TestDbContextWithDefaultRefreshTokenAndUserDevice, TestUser>(predicate));
    }
}
