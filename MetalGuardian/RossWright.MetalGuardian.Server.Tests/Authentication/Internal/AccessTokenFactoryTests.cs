using NSubstitute;
using RossWright.MetalGuardian;
using RossWright.MetalGuardian.Authentication;
using Shouldly;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RossWright.MetalGuardian.Server.Tests.Authentication.Internal;

public class AccessTokenFactoryTests
{
    [Fact]
    public void Constructor_WithValidConfiguration_InitializesSuccessfully()
    {
        // Arrange
        var configuration = CreateValidConfiguration();

        // Act
        var sut = new AccessTokenFactory(configuration);

        // Assert
        sut.ShouldNotBeNull();
        sut.StrictTokenValidationParameters.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithNullJwtIssuer_ThrowsMetalGuardianException()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        configuration.JwtIssuer.Returns((string?)null);

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() => new AccessTokenFactory(configuration));
        exception.Message.ShouldContain("JwtIssuer");
    }

    [Fact]
    public void Constructor_WithEmptyJwtIssuer_ThrowsMetalGuardianException()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        configuration.JwtIssuer.Returns(string.Empty);

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() => new AccessTokenFactory(configuration));
        exception.Message.ShouldContain("JwtIssuer");
    }

    [Fact]
    public void Constructor_WithNullJwtAudience_ThrowsMetalGuardianException()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        configuration.JwtAudience.Returns((string?)null);

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() => new AccessTokenFactory(configuration));
        exception.Message.ShouldContain("JwtAudience");
    }

    [Fact]
    public void Constructor_WithEmptyJwtAudience_ThrowsMetalGuardianException()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        configuration.JwtAudience.Returns(string.Empty);

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() => new AccessTokenFactory(configuration));
        exception.Message.ShouldContain("JwtAudience");
    }

    [Fact]
    public void Constructor_WithNullJwtIssuerSigningKey_ThrowsMetalGuardianException()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        configuration.JwtIssuerSigningKey.Returns((string?)null);

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() => new AccessTokenFactory(configuration));
        exception.Message.ShouldContain("JwtIssuerSigningKey");
    }

    [Fact]
    public void Constructor_WithEmptyJwtIssuerSigningKey_ThrowsMetalGuardianException()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        configuration.JwtIssuerSigningKey.Returns(string.Empty);

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() => new AccessTokenFactory(configuration));
        exception.Message.ShouldContain("JwtIssuerSigningKey");
    }

    [Fact]
    public void Constructor_WithZeroAccessTokenExpireMins_ThrowsMetalGuardianException()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        configuration.JwtAccessTokenExpireMins.Returns(0);

        // Act & Assert
        Should.Throw<MetalGuardianException>(() => new AccessTokenFactory(configuration));
    }

    [Fact]
    public void Constructor_WithNegativeAccessTokenExpireMins_ThrowsMetalGuardianException()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        configuration.JwtAccessTokenExpireMins.Returns(-1);

        // Act & Assert
        Should.Throw<MetalGuardianException>(() => new AccessTokenFactory(configuration));
    }

    [Fact]
    public void Constructor_WithMultipleNullFields_ThrowsExceptionListingAll()
    {
        // Arrange
        var configuration = Substitute.For<IMetalGuardianServerConfiguration>();
        configuration.JwtIssuer.Returns((string?)null);
        configuration.JwtAudience.Returns((string?)null);
        configuration.JwtIssuerSigningKey.Returns((string?)null);
        configuration.JwtAccessTokenExpireMins.Returns(60);

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() => new AccessTokenFactory(configuration));
        exception.Message.ShouldContain("JwtIssuer");
        exception.Message.ShouldContain("JwtAudience");
        exception.Message.ShouldContain("JwtIssuerSigningKey");
    }

    [Fact]
    public void Create_WithValidUser_ReturnsTokenString()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        var sut = new AccessTokenFactory(configuration);
        var user = CreateValidUser();

        // Act
        var result = sut.Create(user, null);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Create_WithValidUser_CreatesValidJwt()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        var sut = new AccessTokenFactory(configuration);
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var user = CreateUser(userId, userName);

        // Act
        var token = sut.Create(user, null);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Issuer.ShouldBe("test-issuer");
        jwtToken.Audiences.ShouldContain("test-audience");
        jwtToken.Claims.ShouldContain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
        jwtToken.Claims.ShouldContain(c => c.Type == ClaimTypes.Name && c.Value == userName);
    }

    [Fact]
    public void Create_WithDefaultUserId_ThrowsArgumentException()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        var sut = new AccessTokenFactory(configuration);
        var user = CreateUser(Guid.Empty, "testuser");

        // Act & Assert
        Should.Throw<ArgumentException>(() => sut.Create(user, null));
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        var sut = new AccessTokenFactory(configuration);
        var user = CreateUser(Guid.NewGuid(), string.Empty);

        // Act & Assert
        Should.Throw<ArgumentException>(() => sut.Create(user, null));
    }

    [Fact]
    public void Create_WithNullName_DoesNotThrow()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        var sut = new AccessTokenFactory(configuration);
        var user = CreateUser(Guid.NewGuid(), null);

        // Act
        var result = sut.Create(user, null);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Create_WithAdditionalClaims_IncludesClaimsInToken()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        var sut = new AccessTokenFactory(configuration);
        var user = CreateValidUser();
        var claims = new List<(string, string)>
        {
            ("custom-claim", "custom-value"),
            ("another-claim", "another-value")
        };

        // Act
        var token = sut.Create(user, claims);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Claims.ShouldContain(c => c.Type == "custom-claim" && c.Value == "custom-value");
        jwtToken.Claims.ShouldContain(c => c.Type == "another-claim" && c.Value == "another-value");
    }

    [Fact]
    public void Create_WithCustomExpirationMins_UsesCustomExpiration()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        configuration.JwtAccessTokenExpireMins.Returns(60);
        var sut = new AccessTokenFactory(configuration);
        var user = CreateValidUser();
        var customExpirationMins = 120;

        // Act
        var token = sut.Create(user, null, customExpirationMins);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var expectedExpiration = DateTime.UtcNow.AddMinutes(customExpirationMins);
        jwtToken.ValidTo.ShouldBeInRange(expectedExpiration.AddSeconds(-5), expectedExpiration.AddSeconds(5));
    }

    [Fact]
    public void Create_WithoutCustomExpirationMins_UsesDefaultExpiration()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        configuration.JwtAccessTokenExpireMins.Returns(60);
        var sut = new AccessTokenFactory(configuration);
        var user = CreateValidUser();

        // Act
        var token = sut.Create(user, null);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var expectedExpiration = DateTime.UtcNow.AddMinutes(60);
        jwtToken.ValidTo.ShouldBeInRange(expectedExpiration.AddSeconds(-5), expectedExpiration.AddSeconds(5));
    }

    [Fact]
    public void Validate_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        var sut = new AccessTokenFactory(configuration);
        var user = CreateValidUser();
        var token = sut.Create(user, null);

        // Act
        var result = sut.Validate(token);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithNullToken_ReturnsFalse()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        var sut = new AccessTokenFactory(configuration);

        // Act
        var result = sut.Validate(null);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Validate_WithEmptyToken_ReturnsFalse()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        var sut = new AccessTokenFactory(configuration);

        // Act
        var result = sut.Validate(string.Empty);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Validate_WithWhitespaceToken_ReturnsFalse()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        var sut = new AccessTokenFactory(configuration);

        // Act
        var result = sut.Validate("   ");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Validate_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        var sut = new AccessTokenFactory(configuration);

        // Act
        var result = sut.Validate("invalid-token-string");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Validate_WithTokenFromDifferentIssuer_ReturnsFalse()
    {
        // Arrange
        var configuration1 = CreateValidConfiguration();
        configuration1.JwtIssuer.Returns("issuer-1");
        var factory1 = new AccessTokenFactory(configuration1);
        var user = CreateValidUser();
        var token = factory1.Create(user, null);

        var configuration2 = CreateValidConfiguration();
        configuration2.JwtIssuer.Returns("issuer-2");
        var factory2 = new AccessTokenFactory(configuration2);

        // Act
        var result = factory2.Validate(token);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Validate_WithTokenFromDifferentAudience_ReturnsFalse()
    {
        // Arrange
        var configuration1 = CreateValidConfiguration();
        configuration1.JwtAudience.Returns("audience-1");
        var factory1 = new AccessTokenFactory(configuration1);
        var user = CreateValidUser();
        var token = factory1.Create(user, null);

        var configuration2 = CreateValidConfiguration();
        configuration2.JwtAudience.Returns("audience-2");
        var factory2 = new AccessTokenFactory(configuration2);

        // Act
        var result = factory2.Validate(token);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Validate_WithTokenSignedWithDifferentKey_ReturnsFalse()
    {
        // Arrange
        var configuration1 = CreateValidConfiguration();
        configuration1.JwtIssuerSigningKey.Returns("this-is-a-very-secure-key-string-1");
        var factory1 = new AccessTokenFactory(configuration1);
        var user = CreateValidUser();
        var token = factory1.Create(user, null);

        var configuration2 = CreateValidConfiguration();
        configuration2.JwtIssuerSigningKey.Returns("this-is-a-very-secure-key-string-2");
        var factory2 = new AccessTokenFactory(configuration2);

        // Act
        var result = factory2.Validate(token);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Validate_WithExpiredToken_ReturnsTrue()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        configuration.JwtAccessTokenExpireMins.Returns(60);
        var sut = new AccessTokenFactory(configuration);
        var user = CreateValidUser();
        var token = sut.Create(user, null, expirationMins: -1);

        // Act
        var result = sut.Validate(token);

        // Assert
        result.ShouldBeTrue();
    }

    private static IMetalGuardianServerConfiguration CreateValidConfiguration()
    {
        var configuration = Substitute.For<IMetalGuardianServerConfiguration>();
        configuration.JwtIssuer.Returns("test-issuer");
        configuration.JwtAudience.Returns("test-audience");
        configuration.JwtIssuerSigningKey.Returns("this-is-a-very-secure-key-string-with-enough-length");
        configuration.JwtAccessTokenExpireMins.Returns(60);
        return configuration;
    }

    private static IAuthenticationUser CreateValidUser()
    {
        return CreateUser(Guid.NewGuid(), "testuser");
    }

    private static IAuthenticationUser CreateUser(Guid userId, string? name)
    {
        var user = Substitute.For<IAuthenticationUser>();
        user.UserId.Returns(userId);
        user.Name.Returns(name);
        return user;
    }
}
