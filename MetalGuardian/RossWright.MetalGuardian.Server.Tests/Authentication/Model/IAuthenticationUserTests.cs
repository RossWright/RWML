using RossWright.MetalGuardian;
using Shouldly;

namespace RossWright.MetalGuardian.Server.Tests;

public class IAuthenticationUserTests
{
    [Fact]
    public void IsPassword_MatchingPassword_ReturnsTrue()
    {
        // Arrange
        var user = new TestAuthenticationUser();
        var password = "TestPassword123";
        user.SetPassword(password);

        // Act
        var result = user.IsPassword(password);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsPassword_NonMatchingPassword_ReturnsFalse()
    {
        // Arrange
        var user = new TestAuthenticationUser();
        user.SetPassword("CorrectPassword");

        // Act
        var result = user.IsPassword("WrongPassword");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsPassword_NullUser_ReturnsFalse()
    {
        // Arrange
        TestAuthenticationUser? user = null;

        // Act
        var result = user!.IsPassword("anyPassword");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsPassword_NullPasswordSalt_ReturnsFalse()
    {
        // Arrange
        var user = new TestAuthenticationUser
        {
            PasswordSalt = null!,
            PasswordHash = "someHash"
        };

        // Act
        var result = user.IsPassword("anyPassword");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsPassword_NullPasswordHash_ReturnsFalse()
    {
        // Arrange
        var user = new TestAuthenticationUser
        {
            PasswordSalt = "someSalt",
            PasswordHash = null!
        };

        // Act
        var result = user.IsPassword("anyPassword");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsPassword_EmptyPasswordSalt_ReturnsFalse()
    {
        // Arrange
        var user = new TestAuthenticationUser
        {
            PasswordSalt = string.Empty,
            PasswordHash = "someHash"
        };

        // Act
        var result = user.IsPassword("anyPassword");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsPassword_EmptyPasswordHash_ReturnsFalse()
    {
        // Arrange
        var user = new TestAuthenticationUser
        {
            PasswordSalt = "someSalt",
            PasswordHash = string.Empty
        };

        // Act
        var result = user.IsPassword("anyPassword");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsPassword_EmptyPassword_CanMatchIfSet()
    {
        // Arrange
        var user = new TestAuthenticationUser();
        user.SetPassword(string.Empty);

        // Act
        var result = user.IsPassword(string.Empty);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsPassword_CaseSensitivePassword_ReturnsFalse()
    {
        // Arrange
        var user = new TestAuthenticationUser();
        user.SetPassword("Password");

        // Act
        var result = user.IsPassword("password");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void SetPassword_SetsPasswordSalt()
    {
        // Arrange
        var user = new TestAuthenticationUser();
        var password = "TestPassword123";

        // Act
        user.SetPassword(password);

        // Assert
        user.PasswordSalt.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void SetPassword_SetsPasswordHash()
    {
        // Arrange
        var user = new TestAuthenticationUser();
        var password = "TestPassword123";

        // Act
        user.SetPassword(password);

        // Assert
        user.PasswordHash.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void SetPassword_ReturnsSameUserInstance()
    {
        // Arrange
        var user = new TestAuthenticationUser();
        var password = "TestPassword123";

        // Act
        var result = user.SetPassword(password);

        // Assert
        result.ShouldBeSameAs(user);
    }

    [Fact]
    public void SetPassword_GeneratesUniqueSaltEachTime()
    {
        // Arrange
        var user1 = new TestAuthenticationUser();
        var user2 = new TestAuthenticationUser();
        var password = "SamePassword";

        // Act
        user1.SetPassword(password);
        user2.SetPassword(password);

        // Assert
        user1.PasswordSalt.ShouldNotBe(user2.PasswordSalt);
    }

    [Fact]
    public void SetPassword_GeneratesUniqueHashEachTime()
    {
        // Arrange
        var user1 = new TestAuthenticationUser();
        var user2 = new TestAuthenticationUser();
        var password = "SamePassword";

        // Act
        user1.SetPassword(password);
        user2.SetPassword(password);

        // Assert
        user1.PasswordHash.ShouldNotBe(user2.PasswordHash);
    }

    [Fact]
    public void SetPassword_OverwritesExistingPassword()
    {
        // Arrange
        var user = new TestAuthenticationUser();
        user.SetPassword("OldPassword");
        var oldSalt = user.PasswordSalt;
        var oldHash = user.PasswordHash;

        // Act
        user.SetPassword("NewPassword");

        // Assert
        user.PasswordSalt.ShouldNotBe(oldSalt);
        user.PasswordHash.ShouldNotBe(oldHash);
        user.IsPassword("NewPassword").ShouldBeTrue();
        user.IsPassword("OldPassword").ShouldBeFalse();
    }

    [Fact]
    public void SetPassword_EmptyPassword_SetsHashAndSalt()
    {
        // Arrange
        var user = new TestAuthenticationUser();

        // Act
        user.SetPassword(string.Empty);

        // Assert
        user.PasswordSalt.ShouldNotBeNullOrEmpty();
        user.PasswordHash.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void SetPassword_WithSpecialCharacters_AllowsVerification()
    {
        // Arrange
        var user = new TestAuthenticationUser();
        var password = "P@ssw0rd!#$%^&*()_+-=[]{}|;:',.<>?/~`";

        // Act
        user.SetPassword(password);

        // Assert
        user.IsPassword(password).ShouldBeTrue();
    }

    [Fact]
    public void SetPassword_WithUnicodeCharacters_AllowsVerification()
    {
        // Arrange
        var user = new TestAuthenticationUser();
        var password = "密码🔐パスワード";

        // Act
        user.SetPassword(password);

        // Assert
        user.IsPassword(password).ShouldBeTrue();
    }

    [Fact]
    public void SetPassword_WithWhitespace_AllowsVerification()
    {
        // Arrange
        var user = new TestAuthenticationUser();
        var password = "  password with spaces  ";

        // Act
        user.SetPassword(password);

        // Assert
        user.IsPassword(password).ShouldBeTrue();
        user.IsPassword("  password with spaces  ").ShouldBeTrue();
    }

    [Fact]
    public void SetPassword_VeryLongPassword_AllowsVerification()
    {
        // Arrange
        var user = new TestAuthenticationUser();
        var password = new string('a', 10000);

        // Act
        user.SetPassword(password);

        // Assert
        user.IsPassword(password).ShouldBeTrue();
    }

    [Fact]
    public void IsPassword_AfterSetPassword_VerifiesCorrectly()
    {
        // Arrange
        var user = new TestAuthenticationUser();
        var password = "TestPassword123";

        // Act
        user.SetPassword(password);
        var correctResult = user.IsPassword(password);
        var incorrectResult = user.IsPassword("WrongPassword");

        // Assert
        correctResult.ShouldBeTrue();
        incorrectResult.ShouldBeFalse();
    }

    [Fact]
    public void SetPassword_FluentChaining_WorksCorrectly()
    {
        // Arrange
        var user = new TestAuthenticationUser();

        // Act
        var result = user.SetPassword("Password1").SetPassword("Password2");

        // Assert
        result.ShouldBeSameAs(user);
        user.IsPassword("Password2").ShouldBeTrue();
        user.IsPassword("Password1").ShouldBeFalse();
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
