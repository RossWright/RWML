using Shouldly;

namespace RossWright.MetalGuardian.Tests.Authentication.Internal;

public class AccessTokenRepositoryTests
{
    [Fact]
    public void Contains_ConnectionNameExists_ReturnsTrue()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.AccessTokenRepository();
        var tokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
        repository.Set("connection1", tokens);

        // Act
        var result = repository.Contains("connection1");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Contains_ConnectionNameDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.AccessTokenRepository();

        // Act
        var result = repository.Contains("nonexistent");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void TryGet_ConnectionNameExists_ReturnsTrueAndTokens()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.AccessTokenRepository();
        var tokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
        repository.Set("connection1", tokens);

        // Act
        var result = repository.TryGet("connection1", out var retrievedTokens);

        // Assert
        result.ShouldBeTrue();
        retrievedTokens.ShouldNotBeNull();
        retrievedTokens.AccessToken.ShouldBe("access");
        retrievedTokens.RefreshToken.ShouldBe("refresh");
    }

    [Fact]
    public void TryGet_ConnectionNameDoesNotExist_ReturnsFalseAndNullTokens()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.AccessTokenRepository();

        // Act
        var result = repository.TryGet("nonexistent", out var retrievedTokens);

        // Assert
        result.ShouldBeFalse();
        retrievedTokens.ShouldBeNull();
    }

    [Fact]
    public void Set_NewConnection_AddsTokens()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.AccessTokenRepository();
        var tokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };

        // Act
        repository.Set("connection1", tokens);

        // Assert
        repository.Contains("connection1").ShouldBeTrue();
        repository.TryGet("connection1", out var retrievedTokens).ShouldBeTrue();
        retrievedTokens.ShouldNotBeNull();
        retrievedTokens.AccessToken.ShouldBe("access");
        retrievedTokens.RefreshToken.ShouldBe("refresh");
    }

    [Fact]
    public void Set_ExistingConnection_UpdatesTokens()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.AccessTokenRepository();
        var originalTokens = new AuthenticationTokens { AccessToken = "access1", RefreshToken = "refresh1" };
        var updatedTokens = new AuthenticationTokens { AccessToken = "access2", RefreshToken = "refresh2" };
        repository.Set("connection1", originalTokens);

        // Act
        repository.Set("connection1", updatedTokens);

        // Assert
        repository.TryGet("connection1", out var retrievedTokens).ShouldBeTrue();
        retrievedTokens.ShouldNotBeNull();
        retrievedTokens.AccessToken.ShouldBe("access2");
        retrievedTokens.RefreshToken.ShouldBe("refresh2");
    }

    [Fact]
    public void Remove_ExistingConnection_RemovesTokens()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.AccessTokenRepository();
        var tokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
        repository.Set("connection1", tokens);

        // Act
        repository.Remove("connection1");

        // Assert
        repository.Contains("connection1").ShouldBeFalse();
        repository.TryGet("connection1", out var retrievedTokens).ShouldBeFalse();
        retrievedTokens.ShouldBeNull();
    }

    [Fact]
    public void Remove_NonExistentConnection_DoesNotThrow()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.AccessTokenRepository();

        // Act & Assert
        Should.NotThrow(() => repository.Remove("nonexistent"));
    }

    [Fact]
    public void Set_MultipleConnections_StoresAllIndependently()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.AccessTokenRepository();
        var tokens1 = new AuthenticationTokens { AccessToken = "access1", RefreshToken = "refresh1" };
        var tokens2 = new AuthenticationTokens { AccessToken = "access2", RefreshToken = "refresh2" };

        // Act
        repository.Set("connection1", tokens1);
        repository.Set("connection2", tokens2);

        // Assert
        repository.Contains("connection1").ShouldBeTrue();
        repository.Contains("connection2").ShouldBeTrue();
        
        repository.TryGet("connection1", out var retrievedTokens1).ShouldBeTrue();
        retrievedTokens1.ShouldNotBeNull();
        retrievedTokens1.AccessToken.ShouldBe("access1");
        retrievedTokens1.RefreshToken.ShouldBe("refresh1");

        repository.TryGet("connection2", out var retrievedTokens2).ShouldBeTrue();
        retrievedTokens2.ShouldNotBeNull();
        retrievedTokens2.AccessToken.ShouldBe("access2");
        retrievedTokens2.RefreshToken.ShouldBe("refresh2");
    }

    [Fact]
    public void Remove_OneOfMultipleConnections_OnlyRemovesThatConnection()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.AccessTokenRepository();
        var tokens1 = new AuthenticationTokens { AccessToken = "access1", RefreshToken = "refresh1" };
        var tokens2 = new AuthenticationTokens { AccessToken = "access2", RefreshToken = "refresh2" };
        repository.Set("connection1", tokens1);
        repository.Set("connection2", tokens2);

        // Act
        repository.Remove("connection1");

        // Assert
        repository.Contains("connection1").ShouldBeFalse();
        repository.Contains("connection2").ShouldBeTrue();
        repository.TryGet("connection2", out var retrievedTokens2).ShouldBeTrue();
        retrievedTokens2.ShouldNotBeNull();
        retrievedTokens2.AccessToken.ShouldBe("access2");
    }
}
