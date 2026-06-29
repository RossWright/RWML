using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Security.Claims;

namespace RossWright.MetalGuardian.Server.Tests.Authentication;

public class IHttpContextAccessorExtensionsTests
{
    [Fact]
    public void GetUserId_WhenHttpContextIsNull_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = httpContextAccessor.GetUserId();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetUserId_WhenUserIsNull_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns((ClaimsPrincipal?)null);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetUserId();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetUserId_WhenClaimDoesNotExist_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetUserId();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetUserId_WhenClaimIsNotValidGuid_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "not-a-guid") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetUserId();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetUserId_WhenClaimIsValidGuid_ReturnsGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, expectedGuid.ToString()) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetUserId();

        // Assert
        result.ShouldBe(expectedGuid);
    }

    [Fact]
    public void GetUserName_WhenHttpContextIsNull_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = httpContextAccessor.GetUserName();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetUserName_WhenUserIsNull_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns((ClaimsPrincipal?)null);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetUserName();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetUserName_WhenClaimDoesNotExist_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetUserName();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetUserName_WhenClaimExists_ReturnsValue()
    {
        // Arrange
        var expectedName = "testuser";
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim(ClaimTypes.Name, expectedName) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetUserName();

        // Assert
        result.ShouldBe(expectedName);
    }

    [Fact]
    public void HasRole_WhenHttpContextIsNull_ReturnsFalse()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = httpContextAccessor.HasRole("Admin");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasRole_WhenUserIsNull_ReturnsFalse()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns((ClaimsPrincipal?)null);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.HasRole("Admin");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasRole_WhenRoleDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim(ClaimTypes.Role, "User") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.HasRole("Admin");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasRole_WhenRoleExists_ReturnsTrue()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.HasRole("Admin");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void HasRole_WhenMultipleRolesExist_ReturnsTrue()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "User"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.HasRole("Admin");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void GetClaim_WhenHttpContextIsNull_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = httpContextAccessor.GetClaim("customClaim");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetClaim_WhenUserIsNull_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns((ClaimsPrincipal?)null);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetClaim("customClaim");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetClaim_WhenClaimDoesNotExist_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetClaim("customClaim");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetClaim_WhenClaimExists_ReturnsValue()
    {
        // Arrange
        var expectedValue = "claimValue";
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim("customClaim", expectedValue) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetClaim("customClaim");

        // Assert
        result.ShouldBe(expectedValue);
    }

    [Fact]
    public void GetClaim_WhenMultipleClaimsExist_ReturnsFirstValue()
    {
        // Arrange
        var firstValue = "firstValue";
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[]
        {
            new Claim("customClaim", firstValue),
            new Claim("customClaim", "secondValue")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetClaim("customClaim");

        // Assert
        result.ShouldBe(firstValue);
    }

    [Fact]
    public void GetClaimValues_WhenHttpContextIsNull_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = httpContextAccessor.GetClaimValues("customClaim");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetClaimValues_WhenUserIsNull_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns((ClaimsPrincipal?)null);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetClaimValues("customClaim");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetClaimValues_WhenClaimDoesNotExist_ReturnsEmptyArray()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetClaimValues("customClaim");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetClaimValues_WhenSingleClaimExists_ReturnsArray()
    {
        // Arrange
        var expectedValue = "claimValue";
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim("customClaim", expectedValue) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetClaimValues("customClaim");

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(1);
        result[0].ShouldBe(expectedValue);
    }

    [Fact]
    public void GetClaimValues_WhenMultipleClaimsExist_ReturnsAllValues()
    {
        // Arrange
        var firstValue = "firstValue";
        var secondValue = "secondValue";
        var thirdValue = "thirdValue";
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[]
        {
            new Claim("customClaim", firstValue),
            new Claim("customClaim", secondValue),
            new Claim("customClaim", thirdValue)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetClaimValues("customClaim");

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(3);
        result[0].ShouldBe(firstValue);
        result[1].ShouldBe(secondValue);
        result[2].ShouldBe(thirdValue);
    }

    [Fact]
    public void GetGuidClaim_WhenHttpContextIsNull_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = httpContextAccessor.GetGuidClaim("customClaim");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetGuidClaim_WhenUserIsNull_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns((ClaimsPrincipal?)null);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetGuidClaim("customClaim");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetGuidClaim_WhenClaimDoesNotExist_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetGuidClaim("customClaim");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetGuidClaim_WhenClaimIsNotValidGuid_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim("customClaim", "not-a-guid") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetGuidClaim("customClaim");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetGuidClaim_WhenClaimIsValidGuid_ReturnsGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim("customClaim", expectedGuid.ToString()) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetGuidClaim("customClaim");

        // Assert
        result.ShouldBe(expectedGuid);
    }

    [Fact]
    public void GetGuidClaims_WhenHttpContextIsNull_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = httpContextAccessor.GetGuidClaims("customClaim");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetGuidClaims_WhenUserIsNull_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns((ClaimsPrincipal?)null);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetGuidClaims("customClaim");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetGuidClaims_WhenClaimDoesNotExist_ReturnsEmptyArray()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetGuidClaims("customClaim");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetGuidClaims_WhenSingleClaimExistsWithValidGuid_ReturnsArray()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim("customClaim", expectedGuid.ToString()) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetGuidClaims("customClaim");

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(1);
        result[0].ShouldBe(expectedGuid);
    }

    [Fact]
    public void GetGuidClaims_WhenMultipleClaimsExistWithValidGuids_ReturnsAllGuids()
    {
        // Arrange
        var firstGuid = Guid.NewGuid();
        var secondGuid = Guid.NewGuid();
        var thirdGuid = Guid.NewGuid();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[]
        {
            new Claim("customClaim", firstGuid.ToString()),
            new Claim("customClaim", secondGuid.ToString()),
            new Claim("customClaim", thirdGuid.ToString())
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetGuidClaims("customClaim");

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(3);
        result[0].ShouldBe(firstGuid);
        result[1].ShouldBe(secondGuid);
        result[2].ShouldBe(thirdGuid);
    }

    [Fact]
    public void GetGuidClaims_WhenClaimIsNotValidGuid_ReturnsNullEntry()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim("customClaim", "not-a-guid") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetGuidClaims("customClaim");

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(1);
        result[0].ShouldBeNull();
    }

    [Fact]
    public void GetGuidClaims_WhenMultipleClaimsExistWithMixedValidInvalidGuids_ReturnsArrayWithNullsForInvalid()
    {
        // Arrange
        var validGuid = Guid.NewGuid();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[]
        {
            new Claim("customClaim", validGuid.ToString()),
            new Claim("customClaim", "not-a-guid"),
            new Claim("customClaim", Guid.NewGuid().ToString())
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = httpContextAccessor.GetGuidClaims("customClaim");

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(3);
        result[0].ShouldBe(validGuid);
        result[1].ShouldBeNull();
        result[2].ShouldNotBeNull();
    }
}
