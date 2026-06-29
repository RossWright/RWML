using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalGuardian;
using RossWright.MetalGuardian.Authentication;
using RossWright.MetalGuardian.Server.Tests.AuthenticationServiceTests;

namespace RossWright.MetalGuardian.Server.Tests.Authentication.Internal;

public class MetalGuardianAuthenticationServiceTests
{
    private static ServiceProvider BuildServiceProvider(
        FakeAuthenticationRepository repo,
        IEnumerable<IUserClaimsProvider>? claimsProviders = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MetalGuardian:JwtIssuer"] = "test-issuer",
                ["MetalGuardian:JwtAudience"] = "test-audience",
                ["MetalGuardian:JwtIssuerSigningKey"] = "this-is-a-32-char-test-signing-key!",
                ["MetalGuardian:JwtAccessTokenExpireMins"] = "60",
                ["MetalGuardian:RefreshTokenExpireMins"] = "10080",
            })
            .Build();

        var services = new ServiceCollection();

        var jwtConfig = new MetalGuardianServerConfiguration();
        configuration.Bind("MetalGuardian", jwtConfig);
        services.AddSingleton<IMetalGuardianServerConfiguration>(jwtConfig);
        services.AddSingleton<IAccessTokenFactory>(_ => new AccessTokenFactory(jwtConfig));

        services.AddScoped<IAuthenticationRepository>(_ => repo);

        foreach (var cp in claimsProviders ?? [])
            services.AddScoped<IUserClaimsProvider>(_ => cp);

        services.AddScoped<IMetalGuardianAuthenticationService, MetalGuardianAuthenticationService>();

        return services.BuildServiceProvider();
    }

    private static IMetalGuardianAuthenticationService GetService(ServiceProvider sp) =>
        sp.CreateScope().ServiceProvider.GetRequiredService<IMetalGuardianAuthenticationService>();

    private const string ValidPassword = "Test@1234";

    [Fact]
    public async Task Refresh_RefreshTokenNotFoundInDatabase_ThrowsNotAuthenticatedException()
    {
        var repo = new FakeAuthenticationRepository();
        var user = FakeUser.WithPassword(ValidPassword) with { Name = "alice" };
        repo.AddUser(user);
        using var sp = BuildServiceProvider(repo);
        var sut = GetService(sp);

        var initial = await sut.Login("alice", ValidPassword, null, CancellationToken.None);
        
        // Use a different refresh token that doesn't exist in the database
        var tokensWithWrongRefreshToken = initial with { RefreshToken = "nonexistent-token" };

        await Should.ThrowAsync<NotAuthenticatedException>(
            () => sut.Refresh(tokensWithWrongRefreshToken, CancellationToken.None));
    }

    [Fact]
    public async Task Refresh_EmptyRefreshToken_ThrowsNotAuthenticatedException()
    {
        var repo = new FakeAuthenticationRepository();
        var user = FakeUser.WithPassword(ValidPassword) with { Name = "alice" };
        repo.AddUser(user);
        using var sp = BuildServiceProvider(repo);
        var sut = GetService(sp);

        var initial = await sut.Login("alice", ValidPassword, null, CancellationToken.None);
        
        var tokensWithEmptyRefreshToken = initial with { RefreshToken = string.Empty };

        await Should.ThrowAsync<NotAuthenticatedException>(
            () => sut.Refresh(tokensWithEmptyRefreshToken, CancellationToken.None));
    }

    [Fact]
    public async Task Refresh_WhitespaceRefreshToken_ThrowsNotAuthenticatedException()
    {
        var repo = new FakeAuthenticationRepository();
        var user = FakeUser.WithPassword(ValidPassword) with { Name = "alice" };
        repo.AddUser(user);
        using var sp = BuildServiceProvider(repo);
        var sut = GetService(sp);

        var initial = await sut.Login("alice", ValidPassword, null, CancellationToken.None);
        
        var tokensWithWhitespaceRefreshToken = initial with { RefreshToken = "   " };

        await Should.ThrowAsync<NotAuthenticatedException>(
            () => sut.Refresh(tokensWithWhitespaceRefreshToken, CancellationToken.None));
    }
}
