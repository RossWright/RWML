using RossWright;
using RossWright.MetalGuardian.Authentication;

namespace RossWright.MetalGuardian.Blazor.Tests.Internal;

public class BlazorAuthenticationTokenRepositoryTests
{
    private static BlazorAuthenticationTokenRepository CreateRepository(IBrowserLocalStorage localStorage) =>
        new(localStorage);

    // --- Constructor ---

    [Fact]
    public void Constructor_StoresLocalStorageService()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();

        var repository = CreateRepository(localStorage);

        repository.ShouldNotBeNull();
    }

    // --- LoadTokens ---

    [Fact]
    public async Task LoadTokens_CachedTokens_ReturnsCachedValue()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        var repository = CreateRepository(localStorage);
        var tokens = new AuthenticationTokens
        {
            AccessToken = "cached-access",
            RefreshToken = "cached-refresh"
        };
        await repository.SaveTokens("test-connection", tokens);

        var result = await repository.LoadTokens("test-connection");

        result.ShouldBe(tokens);
        await localStorage.DidNotReceive().Get(Arg.Any<string>());
    }

    [Fact]
    public async Task LoadTokens_NotCached_LoadsFromLocalStorage()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        localStorage.Get("test-connection-accessToken").Returns("access-123");
        localStorage.Get("test-connection-refreshToken").Returns("refresh-456");
        var repository = CreateRepository(localStorage);

        var result = await repository.LoadTokens("test-connection");

        result.ShouldNotBeNull();
        result!.AccessToken.ShouldBe("access-123");
        result.RefreshToken.ShouldBe("refresh-456");
    }

    [Fact]
    public async Task LoadTokens_NullAccessToken_ReturnsNull()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        localStorage.Get("test-connection-accessToken").Returns((string?)null);
        localStorage.Get("test-connection-refreshToken").Returns("refresh-456");
        var repository = CreateRepository(localStorage);

        var result = await repository.LoadTokens("test-connection");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task LoadTokens_EmptyAccessToken_ReturnsNull()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        localStorage.Get("test-connection-accessToken").Returns("");
        localStorage.Get("test-connection-refreshToken").Returns("refresh-456");
        var repository = CreateRepository(localStorage);

        var result = await repository.LoadTokens("test-connection");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task LoadTokens_WhitespaceAccessToken_ReturnsNull()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        localStorage.Get("test-connection-accessToken").Returns("   ");
        localStorage.Get("test-connection-refreshToken").Returns("refresh-456");
        var repository = CreateRepository(localStorage);

        var result = await repository.LoadTokens("test-connection");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task LoadTokens_NullRefreshToken_ReturnsNull()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        localStorage.Get("test-connection-accessToken").Returns("access-123");
        localStorage.Get("test-connection-refreshToken").Returns((string?)null);
        var repository = CreateRepository(localStorage);

        var result = await repository.LoadTokens("test-connection");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task LoadTokens_EmptyRefreshToken_ReturnsNull()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        localStorage.Get("test-connection-accessToken").Returns("access-123");
        localStorage.Get("test-connection-refreshToken").Returns("");
        var repository = CreateRepository(localStorage);

        var result = await repository.LoadTokens("test-connection");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task LoadTokens_WhitespaceRefreshToken_ReturnsNull()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        localStorage.Get("test-connection-accessToken").Returns("access-123");
        localStorage.Get("test-connection-refreshToken").Returns("   ");
        var repository = CreateRepository(localStorage);

        var result = await repository.LoadTokens("test-connection");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task LoadTokens_AddsToCacheAfterLoad()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        localStorage.Get("test-connection-accessToken").Returns("access-123");
        localStorage.Get("test-connection-refreshToken").Returns("refresh-456");
        var repository = CreateRepository(localStorage);

        var result1 = await repository.LoadTokens("test-connection");
        var result2 = await repository.LoadTokens("test-connection");

        result1.ShouldBe(result2);
        await localStorage.Received(1).Get("test-connection-accessToken");
        await localStorage.Received(1).Get("test-connection-refreshToken");
    }

    [Fact]
    public async Task LoadTokens_NullTokens_CachesNullValue()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        localStorage.Get("test-connection-accessToken").Returns((string?)null);
        localStorage.Get("test-connection-refreshToken").Returns((string?)null);
        var repository = CreateRepository(localStorage);

        var result1 = await repository.LoadTokens("test-connection");
        var result2 = await repository.LoadTokens("test-connection");

        result1.ShouldBeNull();
        result2.ShouldBeNull();
        await localStorage.Received(1).Get("test-connection-accessToken");
        await localStorage.Received(1).Get("test-connection-refreshToken");
    }

    // --- SaveTokens ---

    [Fact]
    public async Task SaveTokens_SavesToBothCacheAndLocalStorage()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        var repository = CreateRepository(localStorage);
        var tokens = new AuthenticationTokens
        {
            AccessToken = "access-789",
            RefreshToken = "refresh-012"
        };

        await repository.SaveTokens("test-connection", tokens);

        await localStorage.Received(1).Set("test-connection-accessToken", "access-789");
        await localStorage.Received(1).Set("test-connection-refreshToken", "refresh-012");
    }

    [Fact]
    public async Task SaveTokens_UpdatesCache()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        var repository = CreateRepository(localStorage);
        var tokens = new AuthenticationTokens
        {
            AccessToken = "access-new",
            RefreshToken = "refresh-new"
        };

        await repository.SaveTokens("test-connection", tokens);
        var result = await repository.LoadTokens("test-connection");

        result.ShouldBe(tokens);
        await localStorage.DidNotReceive().Get(Arg.Any<string>());
    }

    [Fact]
    public async Task SaveTokens_OverwritesExistingCache()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        var repository = CreateRepository(localStorage);
        var oldTokens = new AuthenticationTokens
        {
            AccessToken = "old-access",
            RefreshToken = "old-refresh"
        };
        var newTokens = new AuthenticationTokens
        {
            AccessToken = "new-access",
            RefreshToken = "new-refresh"
        };

        await repository.SaveTokens("test-connection", oldTokens);
        await repository.SaveTokens("test-connection", newTokens);
        var result = await repository.LoadTokens("test-connection");

        result.ShouldBe(newTokens);
    }

    // --- ClearTokens ---

    [Fact]
    public async Task ClearTokens_RemovesFromBothCacheAndLocalStorage()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        var repository = CreateRepository(localStorage);
        var tokens = new AuthenticationTokens
        {
            AccessToken = "access-clear",
            RefreshToken = "refresh-clear"
        };
        await repository.SaveTokens("test-connection", tokens);

        await repository.ClearTokens("test-connection");

        await localStorage.Received(1).Remove("test-connection-accessToken");
        await localStorage.Received(1).Remove("test-connection-refreshToken");
    }

    [Fact]
    public async Task ClearTokens_RemovedTokensNotInCache()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        localStorage.Get("test-connection-accessToken").Returns((string?)null);
        localStorage.Get("test-connection-refreshToken").Returns((string?)null);
        var repository = CreateRepository(localStorage);
        var tokens = new AuthenticationTokens
        {
            AccessToken = "access-clear",
            RefreshToken = "refresh-clear"
        };
        await repository.SaveTokens("test-connection", tokens);

        await repository.ClearTokens("test-connection");
        var result = await repository.LoadTokens("test-connection");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ClearTokens_NonExistentConnection_DoesNotThrow()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        var repository = CreateRepository(localStorage);

        await repository.ClearTokens("non-existent-connection");

        await localStorage.Received(1).Remove("non-existent-connection-accessToken");
        await localStorage.Received(1).Remove("non-existent-connection-refreshToken");
    }

    // --- Multiple Connection Names ---

    [Fact]
    public async Task LoadTokens_DifferentConnectionNames_MaintainsSeparateCaches()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        localStorage.Get("conn1-accessToken").Returns("access-1");
        localStorage.Get("conn1-refreshToken").Returns("refresh-1");
        localStorage.Get("conn2-accessToken").Returns("access-2");
        localStorage.Get("conn2-refreshToken").Returns("refresh-2");
        var repository = CreateRepository(localStorage);

        var result1 = await repository.LoadTokens("conn1");
        var result2 = await repository.LoadTokens("conn2");

        result1.ShouldNotBeNull();
        result1!.AccessToken.ShouldBe("access-1");
        result1.RefreshToken.ShouldBe("refresh-1");
        result2.ShouldNotBeNull();
        result2!.AccessToken.ShouldBe("access-2");
        result2.RefreshToken.ShouldBe("refresh-2");
    }

    [Fact]
    public async Task SaveTokens_DifferentConnectionNames_SavesIndependently()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        var repository = CreateRepository(localStorage);
        var tokens1 = new AuthenticationTokens
        {
            AccessToken = "access-conn1",
            RefreshToken = "refresh-conn1"
        };
        var tokens2 = new AuthenticationTokens
        {
            AccessToken = "access-conn2",
            RefreshToken = "refresh-conn2"
        };

        await repository.SaveTokens("conn1", tokens1);
        await repository.SaveTokens("conn2", tokens2);

        await localStorage.Received(1).Set("conn1-accessToken", "access-conn1");
        await localStorage.Received(1).Set("conn1-refreshToken", "refresh-conn1");
        await localStorage.Received(1).Set("conn2-accessToken", "access-conn2");
        await localStorage.Received(1).Set("conn2-refreshToken", "refresh-conn2");
    }

    [Fact]
    public async Task ClearTokens_OneConnection_DoesNotAffectOthers()
    {
        var localStorage = Substitute.For<IBrowserLocalStorage>();
        localStorage.Get("conn2-accessToken").Returns("access-2");
        localStorage.Get("conn2-refreshToken").Returns("refresh-2");
        var repository = CreateRepository(localStorage);
        var tokens1 = new AuthenticationTokens
        {
            AccessToken = "access-conn1",
            RefreshToken = "refresh-conn1"
        };
        var tokens2 = new AuthenticationTokens
        {
            AccessToken = "access-conn2",
            RefreshToken = "refresh-conn2"
        };
        await repository.SaveTokens("conn1", tokens1);
        await repository.SaveTokens("conn2", tokens2);

        await repository.ClearTokens("conn1");
        var result = await repository.LoadTokens("conn2");

        result.ShouldBe(tokens2);
    }
}
