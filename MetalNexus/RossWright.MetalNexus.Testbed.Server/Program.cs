using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RossWright.MetalNexus.Testbed.Server;
using RossWright.MetalNexus.Testbed.Shared;
using RossWright;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RossWright.MetalInjection;
using RossWright.MetalNexus;

var builder = WebApplication.CreateBuilder(args);

// ── Runtime logging ───────────────────────────────────────────────────────────
builder.Logging
    .ClearProviders()
    .AddMetalConsoleLogger()
    .SetMinimumLevel(LogLevel.Information);

// ── JWT Auth ─────────────────────────────────────────────────────────────────

const string jwtSecret = "MetalNexusTestbedSuperSecretKey!!";
const string jwtIssuer  = "MetalNexusTestbed";
const string jwtAudience = "MetalNexusTestbedClient";

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuer              = jwtIssuer,
            ValidateAudience         = true,
            ValidAudience            = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = signingKey,
            ValidateLifetime         = true,
        };
    });

// ── MetalInjection — replaces the default service provider and auto-registers  ─
// classes decorated with [Singleton], [ScopedService], or [TransientService]
builder.AddMetalInjection(options =>
{
    options.ScanAssemblyContaining<Program>();
    // Bootstrap logging captures discovery/registration diagnostics during startup
    options.UseBootstrapLogger(logging =>
    {
        logging.ClearProviders();
        logging.AddDebug();
        logging.AddMetalConsoleLogger();
        logging.SetMinimumLevel(LogLevel.Debug);
    });
});

// ── MetalNexus server

builder.AddMetalNexusServer(options =>
{
    options.ScanAssemblyContaining(typeof(Program)); // Server — handler types; request types reached via handlers
    options.UseBootstrapLogger(logging =>
    {
        logging.ClearProviders();
        logging.AddDebug();
        logging.AddMetalConsoleLogger();
        logging.SetMinimumLevel(LogLevel.Debug);
    });
    options.IncludeServerStackTraceOnExceptions();
    options.ConfigureEndpointSchema(schema =>
    {
        schema.ApiPathPrefix = "api";
        schema.ApiPathToLower = true;
        schema.RequestSuffixesToTrim = ["Request"];
        schema.RequiresAuthenticationByDefault = true;
    });
});

// ── Swagger ───────────────────────────────────────────────────────────────────

builder.Services.AddSwaggerGen(options =>
{
    options.UseMetalNexus();
    options.SwaggerDoc("v1", new() { Title = "MetalNexus Testbed", Version = "v1" });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();

// ── Late-registered endpoint ──────────────────────────────────────────────────
// Demonstrates AddMetalNexusEndpoints after the main AddMetalNexusServer call.
builder.Services.AddMetalNexusEndpoints(typeof(LateRegisteredRequest));

// ── Build app ─────────────────────────────────────────────────────────────────

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MetalNexus Testbed v1"));

app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// ── /auth/token ───────────────────────────────────────────────────────────────

app.MapPost("/auth/token", (AuthTokenRequest body) =>
{
    var (role, authLevel) = body.Username switch
    {
        "admin"    => ("Admin",     (string?)null),
        "manager"  => ("Manager",   null),
        "readonly" => ("ReadOnly",  null),
        "provisional" => ("ReadOnly", "provisional"),
        _ => (null, null)
    };

    var validPassword = body.Username switch
    {
        "admin"    => "admin",
        "manager"  => "manager",
        "readonly" => "readonly",
        "provisional" => "provisional",
        _ => null
    };

    if (role is null || validPassword is null || body.Password != validPassword)
        return Results.Unauthorized();

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, body.Username),
        new(ClaimTypes.Role, role),
    };
    if (authLevel is not null)
        claims.Add(new Claim("auth_level", authLevel));

    var token = new JwtSecurityToken(
        issuer:   jwtIssuer,
        audience: jwtAudience,
        claims:   claims,
        expires:  DateTime.UtcNow.AddHours(8),
        signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new { token = tokenString, username = body.Username, role });
}).AllowAnonymous();

// ── MetalNexus middleware ─────────────────────────────────────────────────────

app.UseMetalNexusServer();

app.Run();

// ── Auth token request model ──────────────────────────────────────────────────

record AuthTokenRequest(string Username, string Password);
