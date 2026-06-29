using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RossWright.MetalNexus.Integration.Tests.Infrastructure;

/// <summary>
/// Options that control what the <see cref="TestAuthHandler"/> asserts about the
/// current request.  Registered as a singleton so each test factory instance can
/// inject its own values before the host starts.
/// </summary>
internal class TestAuthOptions
{
    /// <summary>When <c>true</c> the handler returns an authenticated principal.</summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>Roles to include in the principal's claims.</summary>
    public string[] Roles { get; set; } = [];

    /// <summary>When <c>true</c> a <c>Provisional=true</c> claim is added.</summary>
    public bool IsProvisional { get; set; }
}

/// <summary>
/// Minimal ASP.NET Core authentication handler used exclusively by the integration
/// test host.  It builds a <see cref="ClaimsPrincipal"/> from the values held in
/// <see cref="TestAuthOptions"/> — no HTTP header is parsed.
/// </summary>
internal class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    TestAuthOptions authOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    internal const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!authOptions.IsAuthenticated)
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "test-user"),
            new(ClaimTypes.NameIdentifier, "test-user-id")
        };

        foreach (var role in authOptions.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        if (authOptions.IsProvisional)
            claims.Add(new Claim("Provisional", "true"));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
