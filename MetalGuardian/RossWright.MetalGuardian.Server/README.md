# RossWright.MetalGuardian.Server

ASP.NET Core server library for MetalGuardian. Provides JWT issuance and refresh, login/logout/refresh MetalNexus handlers, one-time password (OTP) service, `ICurrentUser`, and the full server-side DI wiring via `AddMetalGuardianServer`.

## Installation

```powershell
dotnet add package RossWright.MetalGuardian.Server
```

## Quick Start

```csharp
// Program.cs — ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

// Required for OTP; also supports any IDistributedCache provider
builder.Services.AddDistributedMemoryCache();

builder.AddMetalGuardianServer(options =>
{
    options.UseMetalNexusAuthenticationEndpoints();

    options.UseJwtConfiguration(new MetalGuardianServerConfiguration
    {
        JwtIssuer = "https://myapp.example.com",
        JwtAudience = "myapp",
        JwtIssuerSigningKey = "your-secret-key",
    });

    options.MapDatabaseAuthentication<MyDbContext, MyUser>(
        identity => user => user.Email == identity);

    // Optional: one-time passwords
    options.UseOneTimePassword(otp =>
    {
        otp.NumberOfDigits = 6;
        otp.ExpirationInMinutes = 10;
    });

    // Optional: password validation
    options.UsePasswordValidator(req =>
    {
        req.MinimumLength = 10;
        req.RequireUppercase = true;
        req.RequireLowercase = true;
        req.RequireDigit = true;
        req.RequireSymbol = true;
    });
});
```

Your `DbContext` must implement `IMetalGuardianDbContext<TUser, TRefreshToken>` and your user entity must implement `IAuthenticationUser`.

### ICurrentUser

Inject `ICurrentUser` in any server-side handler to read the authenticated user's identity:

```csharp
public class MyHandler(ICurrentUser currentUser) : IRequestHandler<MyRequest, MyResponse>
{
    public Task<MyResponse> Handle(MyRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) throw new UnauthorizedAccessException();
        // currentUser.UserId, currentUser.UserName, currentUser.GetClaim("tenant"), ...
        return Task.FromResult(new MyResponse());
    }
}
```

### One-time passwords

```csharp
public class SendOtpHandler(IOtpService otp) : IRequestHandler<SendOtpRequest>
{
    public Task Handle(SendOtpRequest request, CancellationToken ct) =>
        otp.SendOtpViaEmail(request.Email,
            code => new MyOtpEmail(request.Email, code), ct);
}
```

## Documentation

Full documentation is available in the [MetalGuardian README](../../MetalGuardian/README.md).
