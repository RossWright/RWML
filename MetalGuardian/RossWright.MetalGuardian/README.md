# RossWright.MetalGuardian

Client library for non-Blazor .NET applications (console, WPF, MAUI, etc.). Provides `IMetalGuardianAuthenticationClient`, authenticated HTTP client helpers, device fingerprinting, and password validation. For Blazor WebAssembly use `RossWright.MetalGuardian.Blazor` instead.

## Installation

```powershell
dotnet add package RossWright.MetalGuardian
```

## Quick Start

### Console App (MetalCommand)

```csharp
ConsoleApplication.CreateBuilder(args)
    .AddMetalGuardianClient(options =>
    {
        options.UseMetalNexusAuthenticationEndpoints();
        options.AddAuthenticatedHttpClient("https://api.example.com");
    })
    .Build()
    .Run();
```

### Generic .NET host

```csharp
services.AddMetalGuardianClient(options =>
{
    options.UseMetalNexusAuthenticationEndpoints();
    options.AddAuthenticatedHttpClient("https://api.example.com");
});
```

### Authenticating

```csharp
public class LoginHandler(IMetalGuardianAuthenticationClient authClient)
{
    public async Task LoginAsync(string email, string password, CancellationToken ct)
    {
        IAuthenticationInformation? info = await authClient.Login(email, password, ct);
        if (info is null) throw new Exception("Invalid credentials.");
        Console.WriteLine($"Logged in as {info.UserName}");
    }
}
```

### Password validation

```csharp
// Registration (Program.cs / DI setup)
options.UsePasswordValidator(req =>
{
    req.MinimumLength = 10;
    req.RequireUppercase = true;
    req.RequireLowercase = true;
    req.RequireDigit = true;
    req.RequireSymbol = true;
});

// Usage
public class RegisterHandler(IPasswordValidator passwordValidator)
{
    public Task Handle(RegisterRequest request, CancellationToken ct)
    {
        if (!passwordValidator.Validate(request.Password, request.Email))
            throw new ArgumentException(passwordValidator.GetPasswordRequirementsMessage());
        // ... create user
        return Task.CompletedTask;
    }
}
```

### Device fingerprinting (non-browser)

Register `MachineDeviceFingerprintService` to enable trusted-device MFA skip on Windows/Linux/macOS clients:

```csharp
services.AddSingleton<IDeviceFingerprintService, MachineDeviceFingerprintService>();
```

## Documentation

Full documentation is available in the [MetalGuardian README](../../MetalGuardian/README.md).
