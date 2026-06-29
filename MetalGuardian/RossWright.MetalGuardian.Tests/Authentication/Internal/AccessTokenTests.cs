using System.Security.Claims;
using System.Text;
using System.Text.Json;
using RossWright.MetalGuardian.Authentication;
using RossWright.MetalGuardian.Internal;

namespace RossWright.MetalGuardian.Tests.Authentication.Internal;

public class AccessTokenTests
{
    [Fact]
    public void Constructor_ValidToken_ParsesSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1);
        var expSeconds = ((DateTimeOffset)exp).ToUnixTimeSeconds();
        var token = CreateValidToken(userId, expSeconds);

        // Act
        var accessToken = new AccessToken(token);

        // Assert
        accessToken.Token.ShouldBe(token);
        accessToken.UserId.ShouldBe(userId);
        accessToken.ExpiresOn.ShouldBe(DateTimeOffset.FromUnixTimeSeconds(expSeconds));
    }

    [Fact]
    public void Constructor_NullToken_ThrowsException()
    {
        // Arrange
        string? token = null;

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() => new AccessToken(token!));
        exception.Message.ShouldBe("Invalid Access Token");
    }

    [Fact]
    public void Constructor_EmptyToken_ThrowsException()
    {
        // Arrange
        var token = string.Empty;

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() => new AccessToken(token));
        exception.Message.ShouldBe("Invalid Access Token");
    }

    [Fact]
    public void Constructor_WhitespaceToken_ThrowsException()
    {
        // Arrange
        var token = "   ";

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() => new AccessToken(token));
        exception.Message.ShouldBe("Invalid Access Token");
    }

    [Fact]
    public void Constructor_TokenWithUserName_ParsesUserName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var token = CreateValidToken(userId, exp, userName: "testuser");

        // Act
        var accessToken = new AccessToken(token);

        // Assert
        accessToken.UserName.ShouldBe("testuser");
    }

    [Fact]
    public void Constructor_TokenWithoutUserName_UserNameIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var token = CreateValidToken(userId, exp, userName: null);

        // Act
        var accessToken = new AccessToken(token);

        // Assert
        accessToken.UserName.ShouldBeNull();
    }

    [Fact]
    public void Constructor_TokenWithProvisionalLogin_ParsesIsProvisional()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var token = CreateValidToken(userId, exp, isProvisional: true);

        // Act
        var accessToken = new AccessToken(token);

        // Assert
        accessToken.IsProvisional.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_TokenWithoutProvisionalLogin_IsProvisionalIsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var token = CreateValidToken(userId, exp, isProvisional: null);

        // Act
        var accessToken = new AccessToken(token);

        // Assert
        accessToken.IsProvisional.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_TokenWithIsKnownDevice_ParsesIsKnownDevice()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var token = CreateValidToken(userId, exp, isKnownDevice: true);

        // Act
        var accessToken = new AccessToken(token);

        // Assert
        accessToken.IsKnownDevice.ShouldBe(true);
    }

    [Fact]
    public void Constructor_TokenWithoutIsKnownDevice_IsKnownDeviceIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var token = CreateValidToken(userId, exp, isKnownDevice: null);

        // Act
        var accessToken = new AccessToken(token);

        // Assert
        accessToken.IsKnownDevice.ShouldBeNull();
    }

    [Fact]
    public void Constructor_TokenWithInvalidUserId_SetsUserIdToEmpty()
    {
        // Arrange
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var claims = new Dictionary<string, object>
        {
            ["exp"] = exp,
            [ClaimTypes.NameIdentifier] = "not-a-guid"
        };
        var json = JsonSerializer.Serialize(claims);
        var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=');
        var token = $"header.{base64Payload}.signature";

        // Act
        var accessToken = new AccessToken(token);

        // Assert
        accessToken.UserId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_TokenWithoutExpClaim_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new Dictionary<string, object>
        {
            [ClaimTypes.NameIdentifier] = userId.ToString()
        };
        var json = JsonSerializer.Serialize(claims);
        var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=');
        var token = $"header.{base64Payload}.signature";

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() => new AccessToken(token));
        exception.Message.ShouldBe("Invalid Access Token");
    }

    [Fact]
    public void Constructor_TokenWithoutUserIdClaim_ThrowsException()
    {
        // Arrange
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var claims = new Dictionary<string, object>
        {
            ["exp"] = exp
        };
        var json = JsonSerializer.Serialize(claims);
        var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=');
        var token = $"header.{base64Payload}.signature";

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() => new AccessToken(token));
        exception.Message.ShouldBe("Invalid Access Token");
    }

    [Fact]
    public void Constructor_TokenWithPaddingRequired2_AddsCorrectPadding()
    {
        // Arrange - Create a payload that when base64-encoded has length % 4 == 2
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var claims = new Dictionary<string, object>
        {
            ["exp"] = exp,
            [ClaimTypes.NameIdentifier] = userId.ToString(),
            ["x"] = "a" // Adjust content to get correct length
        };
        var json = JsonSerializer.Serialize(claims);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        // Trim to ensure length % 4 == 2
        while (base64.Length % 4 != 2)
        {
            base64 = base64.TrimEnd('=');
            if (base64.Length % 4 == 2) break;
            // Add a character to adjust
            claims["x"] = new string('a', ((string)claims["x"]).Length + 1);
            json = JsonSerializer.Serialize(claims);
            base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=');
        }
        var token = $"header.{base64}.signature";

        // Act
        var accessToken = new AccessToken(token);

        // Assert
        accessToken.UserId.ShouldBe(userId);
    }

    [Fact]
    public void Constructor_TokenWithPaddingRequired3_AddsCorrectPadding()
    {
        // Arrange - Create a payload that when base64-encoded has length % 4 == 3
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var claims = new Dictionary<string, object>
        {
            ["exp"] = exp,
            [ClaimTypes.NameIdentifier] = userId.ToString(),
            ["x"] = "ab" // Adjust content to get correct length
        };
        var json = JsonSerializer.Serialize(claims);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        // Trim to ensure length % 4 == 3
        while (base64.Length % 4 != 3)
        {
            base64 = base64.TrimEnd('=');
            if (base64.Length % 4 == 3) break;
            // Add a character to adjust
            claims["x"] = new string('a', ((string)claims["x"]).Length + 1);
            json = JsonSerializer.Serialize(claims);
            base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=');
        }
        var token = $"header.{base64}.signature";

        // Act
        var accessToken = new AccessToken(token);

        // Assert
        accessToken.UserId.ShouldBe(userId);
    }

    [Fact]
    public void TryCreate_ValidToken_ReturnsAccessToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var token = CreateValidToken(userId, exp);

        // Act
        var accessToken = AccessToken.TryCreate(token);

        // Assert
        accessToken.ShouldNotBeNull();
        accessToken.UserId.ShouldBe(userId);
    }

    [Fact]
    public void TryCreate_NullToken_ReturnsNull()
    {
        // Arrange
        string? token = null;

        // Act
        var accessToken = AccessToken.TryCreate(token);

        // Assert
        accessToken.ShouldBeNull();
    }

    [Fact]
    public void TryCreate_EmptyToken_ReturnsNull()
    {
        // Arrange
        var token = string.Empty;

        // Act
        var accessToken = AccessToken.TryCreate(token);

        // Assert
        accessToken.ShouldBeNull();
    }

    [Fact]
    public void TryCreate_WhitespaceToken_ReturnsNull()
    {
        // Arrange
        var token = "   ";

        // Act
        var accessToken = AccessToken.TryCreate(token);

        // Assert
        accessToken.ShouldBeNull();
    }

    [Fact]
    public void TryCreate_InvalidToken_ReturnsNull()
    {
        // Arrange
        var token = "invalid.token.format";

        // Act
        var accessToken = AccessToken.TryCreate(token);

        // Assert
        accessToken.ShouldBeNull();
    }

    [Fact]
    public void GetAdditionalClaim_ExistingClaim_ReturnsValue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var additionalClaims = new Dictionary<string, object>
        {
            ["customClaim"] = "customValue"
        };
        var token = CreateValidToken(userId, exp, additionalClaims: additionalClaims);
        var accessToken = new AccessToken(token);

        // Act
        var value = accessToken.GetAdditionalClaim("customClaim");

        // Assert
        value.ShouldBe("customValue");
    }

    [Fact]
    public void GetAdditionalClaim_NonExistingClaim_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var token = CreateValidToken(userId, exp);
        var accessToken = new AccessToken(token);

        // Act
        var value = accessToken.GetAdditionalClaim("nonExistingClaim");

        // Assert
        value.ShouldBeNull();
    }

    [Fact]
    public void AsClaimsIdentity_CreatesClaimsIdentityWithAllClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var additionalClaims = new Dictionary<string, object>
        {
            ["customClaim"] = "customValue"
        };
        var token = CreateValidToken(userId, exp, userName: "testuser", additionalClaims: additionalClaims);
        var accessToken = new AccessToken(token);

        // Act
        var claimsIdentity = accessToken.AsClaimsIdentity();

        // Assert
        claimsIdentity.ShouldNotBeNull();
        claimsIdentity.AuthenticationType.ShouldBe("MetalGuardian");
        claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value.ShouldBe(userId.ToString());
        claimsIdentity.FindFirst(ClaimTypes.Name)?.Value.ShouldBe("testuser");
        claimsIdentity.FindFirst("customClaim")?.Value.ShouldBe("customValue");
    }

    [Fact]
    public void AsClaimsIdentity_IncludesExpClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var token = CreateValidToken(userId, exp);
        var accessToken = new AccessToken(token);

        // Act
        var claimsIdentity = accessToken.AsClaimsIdentity();

        // Assert
        claimsIdentity.FindFirst("exp")?.Value.ShouldBe(exp.ToString());
    }

    [Fact]
    public void Constructor_TokenWithNullJsonPayload_ThrowsException()
    {
        // Arrange - Create a token with "null" as the JSON payload
        var nullJson = "null";
        var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(nullJson)).TrimEnd('=');
        var token = $"header.{base64Payload}.signature";

        // Act & Assert
        var exception = Should.Throw<MetalGuardianException>(() => new AccessToken(token));
        exception.Message.ShouldBe("Invalid Access Token");
    }

    [Fact]
    public void Constructor_TokenWithNoPaddingNeeded_ParsesSuccessfully()
    {
        // Arrange - Create a payload that when base64-encoded has length % 4 == 0 (no padding needed)
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var claims = new Dictionary<string, object>
        {
            ["exp"] = exp,
            [ClaimTypes.NameIdentifier] = userId.ToString()
        };
        var json = JsonSerializer.Serialize(claims);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        // Ensure length % 4 == 0 by adding/removing content
        while (base64.TrimEnd('=').Length % 4 != 0)
        {
            claims["pad"] = new string('x', (claims.ContainsKey("pad") ? ((string)claims["pad"]).Length : 0) + 1);
            json = JsonSerializer.Serialize(claims);
            base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }
        var base64NoPadding = base64.TrimEnd('=');
        var token = $"header.{base64NoPadding}.signature";

        // Act
        var accessToken = new AccessToken(token);

        // Assert
        accessToken.UserId.ShouldBe(userId);
    }

    private static string CreateValidToken(Guid userId, long exp, string? userName = null, bool? isProvisional = null, bool? isKnownDevice = null, Dictionary<string, object>? additionalClaims = null)
    {
        var claims = new Dictionary<string, object>
        {
            ["exp"] = exp,
            [ClaimTypes.NameIdentifier] = userId.ToString()
        };

        if (userName != null)
            claims[ClaimTypes.Name] = userName;

        if (isProvisional.HasValue)
            claims[MetalGuardianClaimTypes.ProvisionalLogin] = isProvisional.Value.ToString().ToLower();

        if (isKnownDevice.HasValue)
            claims[MetalGuardianClaimTypes.IsKnownDevice] = isKnownDevice.Value.ToString().ToLower();

        if (additionalClaims != null)
        {
            foreach (var claim in additionalClaims)
                claims[claim.Key] = claim.Value;
        }

        var json = JsonSerializer.Serialize(claims);
        var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=');
        return $"header.{base64Payload}.signature";
    }
}
