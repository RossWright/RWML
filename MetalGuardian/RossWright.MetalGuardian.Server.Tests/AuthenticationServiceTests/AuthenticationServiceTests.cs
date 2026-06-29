using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalGuardian;
using RossWright.MetalGuardian.Authentication;
using RossWright.MetalGuardian.Server.Tests.AuthenticationServiceTests;
using System.Security.Claims;

namespace RossWright.MetalGuardian.Server.Tests;

public class MetalGuardianAuthenticationServiceTests
{
    // ── Test fixture helpers ──────────────────────────────────────────────────

    private static ServiceProvider BuildServiceProvider(
        FakeAuthenticationRepository repo,
        FakeUserDeviceRepository? deviceRepo = null,
        IEnumerable<IMultifactorAuthenticationProvider>? mfaProviders = null,
        IEnumerable<IUserClaimsProvider>? claimsProviders = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MetalGuardian:JwtIssuer"] = "test-issuer",
                ["MetalGuardian:JwtAudience"] = "test-audience",
                ["MetalGuardian:JwtIssuerSigningKey"] = "this-is-a-32-char-test-signing-key!",
                ["MetalGuardian:JwtAccessTokenExpireMins"] = "60",
            })
            .Build();

        var services = new ServiceCollection();

        // Register JWT configuration + access token factory (mirrors InitializeServer)
        var jwtConfig = new MetalGuardianServerConfiguration();
        configuration.Bind("MetalGuardian", jwtConfig);
        services.AddSingleton<IMetalGuardianServerConfiguration>(jwtConfig);
        services.AddSingleton<IAccessTokenFactory>(_ => new AccessTokenFactory(jwtConfig));

        // Repositories
        services.AddScoped<IAuthenticationRepository>(_ => repo);
        if (deviceRepo != null)
            services.AddScoped<IUserDeviceRepository>(_ => deviceRepo);

        // MFA providers
        foreach (var mfa in mfaProviders ?? [])
            services.AddScoped<IMultifactorAuthenticationProvider>(_ => mfa);

        // Claims providers
        foreach (var cp in claimsProviders ?? [])
            services.AddScoped<IUserClaimsProvider>(_ => cp);

        services.AddScoped<IMetalGuardianAuthenticationService, MetalGuardianAuthenticationService>();

        return services.BuildServiceProvider();
    }

    private static IMetalGuardianAuthenticationService GetService(ServiceProvider sp) =>
        sp.CreateScope().ServiceProvider.GetRequiredService<IMetalGuardianAuthenticationService>();

    private const string ValidPassword = "Test@1234";

    // ── Login(string, string) tests ───────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_NoMfa_ReturnsTokensWithRefreshToken()
    {
        var repo = new FakeAuthenticationRepository();
        repo.AddUser(FakeUser.WithPassword(ValidPassword) with { Name = "alice" });

        using var sp = BuildServiceProvider(repo);
        var sut = GetService(sp);

        var tokens = await sut.Login("alice", ValidPassword, null, CancellationToken.None);

        tokens.AccessToken.ShouldNotBeNullOrWhiteSpace();
        tokens.RefreshToken.ShouldNotBeNullOrWhiteSpace();
        repo.RefreshTokens.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Login_UnknownUser_ThrowsNotAuthenticatedException()
    {
        var repo = new FakeAuthenticationRepository();
        using var sp = BuildServiceProvider(repo);
        var sut = GetService(sp);

        await Should.ThrowAsync<NotAuthenticatedException>(
            () => sut.Login("nobody", ValidPassword, null, CancellationToken.None));
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsNotAuthenticatedException()
    {
        var repo = new FakeAuthenticationRepository();
        repo.AddUser(FakeUser.WithPassword(ValidPassword) with { Name = "alice" });
        using var sp = BuildServiceProvider(repo);
        var sut = GetService(sp);

        await Should.ThrowAsync<NotAuthenticatedException>(
            () => sut.Login("alice", "wrong", null, CancellationToken.None));
    }

    [Fact]
    public async Task Login_DisabledUser_ThrowsNotAuthenticatedException()
    {
        var repo = new FakeAuthenticationRepository();
        var user = FakeUser.WithPassword(ValidPassword) with { Name = "alice", IsDisabled = true };
        repo.AddUser(user);
        using var sp = BuildServiceProvider(repo);
        var sut = GetService(sp);

        await Should.ThrowAsync<NotAuthenticatedException>(
            () => sut.Login("alice", ValidPassword, null, CancellationToken.None));
    }

    [Fact]
    public async Task Login_MfaRequired_NoDeviceRepo_ReturnsProvisionalToken_NoRefreshToken()
    {
        var repo = new FakeAuthenticationRepository();
        repo.AddUser(FakeUser.WithPassword(ValidPassword) with { Name = "alice" });
        using var sp = BuildServiceProvider(repo, mfaProviders: [new FakeMfaProvider(true)]);
        var sut = GetService(sp);

        var tokens = await sut.Login("alice", ValidPassword, "fp", CancellationToken.None);

        tokens.AccessToken.ShouldNotBeNullOrWhiteSpace();
        tokens.RefreshToken.ShouldBeNullOrWhiteSpace();
        repo.RefreshTokens.ShouldBeEmpty();
    }

    [Fact]
    public async Task Login_MfaRequired_KnownDevice_ReturnsFullToken()
    {
        var repo = new FakeAuthenticationRepository();
        var user = FakeUser.WithPassword(ValidPassword) with { Name = "alice" };
        repo.AddUser(user);
        var deviceRepo = new FakeUserDeviceRepository();
        deviceRepo.AddDevice(new FakeUserDevice { UserId = user.UserId, Fingerprint = "fp", ExpiresOn = null });
        using var sp = BuildServiceProvider(repo, deviceRepo, [new FakeKnownDeviceMfaProvider()]);
        var sut = GetService(sp);

        var tokens = await sut.Login("alice", ValidPassword, "fp", CancellationToken.None);

        tokens.RefreshToken.ShouldNotBeNullOrWhiteSpace();
        repo.RefreshTokens.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Login_MfaRequired_UnknownDevice_ReturnsProvisionalToken()
    {
        var repo = new FakeAuthenticationRepository();
        var user = FakeUser.WithPassword(ValidPassword) with { Name = "alice" };
        repo.AddUser(user);
        var deviceRepo = new FakeUserDeviceRepository();
        // No device registered — device is unknown
        using var sp = BuildServiceProvider(repo, deviceRepo, [new FakeKnownDeviceMfaProvider()]);
        var sut = GetService(sp);

        var tokens = await sut.Login("alice", ValidPassword, "fp", CancellationToken.None);

        tokens.RefreshToken.ShouldBeNullOrWhiteSpace();
        repo.RefreshTokens.ShouldBeEmpty();
    }

    [Fact]
    public async Task Login_MfaRequired_DeviceRepoPresent_NoFingerprint_IsKnownDeviceNull_ReturnsProvisional()
    {
        var repo = new FakeAuthenticationRepository();
        var user = FakeUser.WithPassword(ValidPassword) with { Name = "alice" };
        repo.AddUser(user);
        var deviceRepo = new FakeUserDeviceRepository();
        deviceRepo.AddDevice(new FakeUserDevice { UserId = user.UserId, Fingerprint = "fp", ExpiresOn = null });
        using var sp = BuildServiceProvider(repo, deviceRepo, [new FakeKnownDeviceMfaProvider()]);
        var sut = GetService(sp);

        // No fingerprint supplied — isKnownDevice should be null → provisional
        var tokens = await sut.Login("alice", ValidPassword, null, CancellationToken.None);

        tokens.RefreshToken.ShouldBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_IAuthenticationUser_BypassesMfa_ReturnsFullToken()
    {
        var repo = new FakeAuthenticationRepository();
        var user = FakeUser.WithPassword(ValidPassword) with { Name = "alice" };
        repo.AddUser(user);
        using var sp = BuildServiceProvider(repo, mfaProviders: [new FakeMfaProvider(true)]);
        var sut = GetService(sp);

        var tokens = await sut.Login(user, CancellationToken.None);

        tokens.RefreshToken.ShouldNotBeNullOrWhiteSpace();
        repo.RefreshTokens.ShouldHaveSingleItem();
    }

    // ── Refresh tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Refresh_ValidUnexpiredToken_ReturnsNewTokens_AndRotatesRefreshToken()
    {
        var repo = new FakeAuthenticationRepository();
        var user = FakeUser.WithPassword(ValidPassword) with { Name = "alice" };
        repo.AddUser(user);
        using var sp = BuildServiceProvider(repo);
        var sut = GetService(sp);

        var initial = await sut.Login("alice", ValidPassword, null, CancellationToken.None);
        var refreshed = await sut.Refresh(initial, CancellationToken.None);

        refreshed.AccessToken.ShouldNotBeNullOrWhiteSpace();
        refreshed.RefreshToken.ShouldNotBeNullOrWhiteSpace();
        refreshed.RefreshToken.ShouldNotBe(initial.RefreshToken);
    }

    [Fact]
    public async Task Refresh_InvalidAccessTokenSignature_ThrowsNotAuthenticatedException()
    {
        var repo = new FakeAuthenticationRepository();
        using var sp = BuildServiceProvider(repo);
        var sut = GetService(sp);

        var badTokens = new AuthenticationTokens { AccessToken = "not.a.jwt", RefreshToken = "whatever" };

        await Should.ThrowAsync<NotAuthenticatedException>(
            () => sut.Refresh(badTokens, CancellationToken.None));
    }

    [Fact]
    public async Task Refresh_ExpiredRefreshToken_ThrowsNotAuthenticatedException()
    {
        var repo = new FakeAuthenticationRepository();
        var user = FakeUser.WithPassword(ValidPassword) with { Name = "alice" };
        repo.AddUser(user);
        using var sp = BuildServiceProvider(repo);
        var sut = GetService(sp);

        var initial = await sut.Login("alice", ValidPassword, null, CancellationToken.None);

        // Manually expire the refresh token
        var rt = repo.RefreshTokens.Single();
        rt.ExpiresOn = DateTime.UtcNow.AddMinutes(-1);

        await Should.ThrowAsync<NotAuthenticatedException>(
            () => sut.Refresh(initial, CancellationToken.None));
    }

    [Fact]
    public async Task Refresh_DisabledUserOnRefresh_ThrowsNotAuthenticatedException()
    {
        var repo = new FakeAuthenticationRepository();
        var user = FakeUser.WithPassword(ValidPassword) with { Name = "alice" };
        repo.AddUser(user);
        using var sp = BuildServiceProvider(repo);
        var sut = GetService(sp);

        var initial = await sut.Login("alice", ValidPassword, null, CancellationToken.None);
        user.IsDisabled = true;

        await Should.ThrowAsync<NotAuthenticatedException>(
            () => sut.Refresh(initial, CancellationToken.None));
    }

    // ── Logout tests ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_ValidTokens_DeletesRefreshToken()
    {
        var repo = new FakeAuthenticationRepository();
        var user = FakeUser.WithPassword(ValidPassword) with { Name = "alice" };
        repo.AddUser(user);
        using var sp = BuildServiceProvider(repo);
        var sut = GetService(sp);

        var tokens = await sut.Login("alice", ValidPassword, null, CancellationToken.None);
        repo.RefreshTokens.ShouldHaveSingleItem();

        await sut.Logout(tokens, CancellationToken.None);

        repo.RefreshTokens.ShouldBeEmpty();
    }

    [Fact]
    public async Task Logout_InvalidAccessToken_DoesNotThrow_NoTokenDeleted()
    {
        var repo = new FakeAuthenticationRepository();
        using var sp = BuildServiceProvider(repo);
        var sut = GetService(sp);

        var badTokens = new AuthenticationTokens { AccessToken = "invalid.token.value", RefreshToken = "whatever" };

        await Should.NotThrowAsync(() => sut.Logout(badTokens, CancellationToken.None));
        repo.RefreshTokens.ShouldBeEmpty();
    }

    // ── Claims gathering tests ────────────────────────────────────────────────

    [Fact]
    public async Task Login_SingleClaimsProvider_ClaimsAppearedInToken()
    {
        var repo = new FakeAuthenticationRepository();
        var user = FakeUser.WithPassword(ValidPassword) with { Name = "alice" };
        repo.AddUser(user);
        var claimsProvider = new FakeUserClaimsProvider();
        claimsProvider.Claims.Add(("custom-claim", "claim-value"));
        using var sp = BuildServiceProvider(repo, claimsProviders: [claimsProvider]);
        var sut = GetService(sp);

        var tokens = await sut.Login("alice", ValidPassword, null, CancellationToken.None);
        var decoded = tokens.DecodeAccessToken();

        decoded!.AsClaimsIdentity().Claims
            .ShouldContain(c => c.Type == "custom-claim" && c.Value == "claim-value");
    }

    [Fact]
    public async Task Login_MultipleClaimsProviders_AllClaimsMerged()
    {
        var repo = new FakeAuthenticationRepository();
        var user = FakeUser.WithPassword(ValidPassword) with { Name = "alice" };
        repo.AddUser(user);
        var cp1 = new FakeUserClaimsProvider();
        cp1.Claims.Add(("claim-one", "value-one"));
        var cp2 = new FakeUserClaimsProvider();
        cp2.Claims.Add(("claim-two", "value-two"));
        using var sp = BuildServiceProvider(repo, claimsProviders: [cp1, cp2]);
        var sut = GetService(sp);

        var tokens = await sut.Login("alice", ValidPassword, null, CancellationToken.None);
        var decoded = tokens.DecodeAccessToken();

        var claims = decoded!.AsClaimsIdentity().Claims.ToList();
        claims.ShouldContain(c => c.Type == "claim-one" && c.Value == "value-one");
        claims.ShouldContain(c => c.Type == "claim-two" && c.Value == "value-two");
    }
}
