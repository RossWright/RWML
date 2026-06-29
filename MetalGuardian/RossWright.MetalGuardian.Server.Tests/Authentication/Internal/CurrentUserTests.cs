using Microsoft.AspNetCore.Http;
using NSubstitute;
using RossWright.MetalGuardian.Authentication;
using System.Security.Claims;

namespace RossWright.MetalGuardian.Server.Tests.Authentication.Internal;

public class CurrentUserTests
{
    [Fact]
    public void UserId_WhenGetUserIdReturnsGuid_ReturnsGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, expectedGuid.ToString()) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.UserId;

        // Assert
        result.ShouldBe(expectedGuid);
    }

    [Fact]
    public void UserId_WhenGetUserIdReturnsNull_ReturnsGuidEmpty()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.UserId;

        // Assert
        result.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void UserName_WhenGetUserNameReturnsValue_ReturnsValue()
    {
        // Arrange
        var expectedName = "testuser";
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim(ClaimTypes.Name, expectedName) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.UserName;

        // Assert
        result.ShouldBe(expectedName);
    }

    [Fact]
    public void GetClaim_WhenClaimExists_ReturnsValue()
    {
        // Arrange
        var expectedValue = "claimValue";
        var claimName = "customClaim";
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim(claimName, expectedValue) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.GetClaim(claimName);

        // Assert
        result.ShouldBe(expectedValue);
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
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.GetClaim("nonExistent");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetClaimValues_WhenClaimsExist_ReturnsValues()
    {
        // Arrange
        var claimName = "customClaim";
        var firstValue = "firstValue";
        var secondValue = "secondValue";
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[]
        {
            new Claim(claimName, firstValue),
            new Claim(claimName, secondValue)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.GetClaimValues(claimName);

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(2);
        result[0].ShouldBe(firstValue);
        result[1].ShouldBe(secondValue);
    }

    [Fact]
    public void GetClaimValues_WhenHttpContextIsNull_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.GetClaimValues("customClaim");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetGuidClaim_WhenClaimIsValidGuid_ReturnsGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        var claimName = "guidClaim";
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim(claimName, expectedGuid.ToString()) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.GetGuidClaim(claimName);

        // Assert
        result.ShouldBe(expectedGuid);
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
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.GetGuidClaim("nonExistent");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetGuidClaims_WhenHttpContextIsNull_ReturnsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.GetGuidClaims("customClaim");

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
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.GetGuidClaims("customClaim");

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(0);
    }

    [Fact]
    public void GetGuidClaims_WhenSingleClaimExistsWithValidGuid_ReturnsArray()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        var claimName = "customClaim";
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim(claimName, expectedGuid.ToString()) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.GetGuidClaims(claimName);

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
        var claimName = "customClaim";
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[]
        {
            new Claim(claimName, firstGuid.ToString()),
            new Claim(claimName, secondGuid.ToString())
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.GetGuidClaims(claimName);

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(2);
        result[0].ShouldBe(firstGuid);
        result[1].ShouldBe(secondGuid);
    }

    [Fact]
    public void GetGuidClaims_WhenClaimIsNotValidGuid_ReturnsNullEntry()
    {
        // Arrange
        var claimName = "customClaim";
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var claims = new[] { new Claim(claimName, "not-a-guid") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.GetGuidClaims(claimName);

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(1);
        result[0].ShouldBeNull();
    }

    [Fact]
    public void HasRole_WhenHttpContextIsNull_ReturnsFalse()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.HasRole("Admin");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasRole_WhenRoleDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.HasRole("Admin");

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
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.HasRole("Admin");

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
        var currentUser = new CurrentUser(httpContextAccessor);

        // Act
        var result = currentUser.HasRole("Admin");

        // Assert
        result.ShouldBeTrue();
    }
}
