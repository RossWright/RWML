using RossWright.MetalGuardian;
using Shouldly;

namespace RossWright.MetalGuardian.Server.Tests;

public class RefreshTokenTests
{
    [Fact]
    public void User_WhenAccessedViaInterface_ReturnsGenericUserProperty()
    {
        // Arrange
        var testUser = new TestAuthenticationUser
        {
            UserId = Guid.NewGuid(),
            Name = "TestUser"
        };
        var refreshToken = new RefreshToken<TestAuthenticationUser>
        {
            User = testUser
        };

        // Act
        IRefreshToken interfaceToken = refreshToken;
        var result = interfaceToken.User;

        // Assert
        result.ShouldBe(testUser);
    }

    [Fact]
    public void User_WhenUserIsNull_ReturnsNull()
    {
        // Arrange
        var refreshToken = new RefreshToken<TestAuthenticationUser>
        {
            User = null!
        };

        // Act
        IRefreshToken interfaceToken = refreshToken;
        var result = interfaceToken.User;

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void User_WhenAccessedMultipleTimes_ReturnsSameInstance()
    {
        // Arrange
        var testUser = new TestAuthenticationUser
        {
            UserId = Guid.NewGuid(),
            Name = "TestUser"
        };
        var refreshToken = new RefreshToken<TestAuthenticationUser>
        {
            User = testUser
        };
        IRefreshToken interfaceToken = refreshToken;

        // Act
        var result1 = interfaceToken.User;
        var result2 = interfaceToken.User;

        // Assert
        result1.ShouldBeSameAs(result2);
        result1.ShouldBeSameAs(testUser);
    }

    [Fact]
    public void User_WhenAccessedViaInterface_ReturnsAsIAuthenticationUser()
    {
        // Arrange
        var testUser = new TestAuthenticationUser
        {
            UserId = Guid.NewGuid(),
            Name = "TestUser",
            IsDisabled = true
        };
        var refreshToken = new RefreshToken<TestAuthenticationUser>
        {
            User = testUser
        };

        // Act
        IRefreshToken interfaceToken = refreshToken;
        var result = interfaceToken.User;

        // Assert
        result.ShouldBeOfType<TestAuthenticationUser>();
        result.UserId.ShouldBe(testUser.UserId);
        result.Name.ShouldBe(testUser.Name);
        result.IsDisabled.ShouldBe(testUser.IsDisabled);
    }

    private class TestAuthenticationUser : IAuthenticationUser
    {
        public Guid UserId { get; init; }
        public string Name { get; init; } = string.Empty;
        public bool IsDisabled { get; init; }
        public string PasswordSalt { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}
