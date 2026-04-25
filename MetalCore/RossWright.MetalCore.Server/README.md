# Ross Wright's Metal Core Server Library
Copyright (c) 2023-2026 Pross Co.

## Table of Contents

- [Messaging](#messaging)
  - [Messaging Contracts](#messaging-contracts)
  - [SMTP Email](#smtp-email)
- [App Builder Fluent Syntax](#app-builder-fluent-syntax)
  - [`WebApplicationBuilder`](#webapplicationbuilder)
  - [`WebApplication`](#webapplication)
- [Installation](#installation)
- [See Also](#see-also)
- [License](#license)

---

## Messaging

The Server package provides an **abstraction framework** for messaging that decouples application code from delivery implementations. Application code depends only on `IEmailService` or `ISmsService`; switching to a different delivery provider requires only a registration change in `Program.cs` — no changes to handlers or services.

Currently provided: **SMTP** via `AddSmtpEmailService()`. SendGrid and Twilio integrations were previously available as separate packages and may return in a future release.

```csharp
// Program.cs
builder.AddSmtpEmailService();

// Handler
await emailService.Send(new AddressedEmail {
    Subject = "Welcome",
    ToRecipients = [new EmailRecipient { Email = user.Email }],
    TextBody = "Thanks for signing up."
});
```

### Messaging Contracts

`RossWright.Messaging` namespace.

| Type | Description |
|---|---|
| `IMessageContent` | Base content contract; exposes `TextBody` |
| `IEmailContent` | Extends `IMessageContent` with `Subject` and optional `HtmlBody` |
| `IAddressedEmail` | Extends `IEmailContent` with `ToRecipients`, `CcRecipients`, `BccRecipients` |
| `IEmailRecipient` | Contract for a single email address: `Name?` and `Email` |
| `EmailRecipient` | Concrete `IEmailRecipient` |
| `AddressedEmail` | Concrete `IAddressedEmail` |
| `IEmailService` | `Send(IAddressedEmail)` — abstracts email delivery |
| `IAddressedSmsMessage` | Extends `IMessageContent` with `Recipients` |
| `ISmsRecipient` | Contract for a single SMS recipient: `PhoneNumber` |
| `ISmsService` | `Send(IAddressedSmsMessage)` — abstracts SMS delivery |
| `ISmsServiceExtensions` | Convenience overloads for sending to a phone number, single recipient, or from `IMessageContent` |

### SMTP Email

`RossWright.Messaging.Smtp` namespace.

Bind configuration from `appsettings.json` under the `"MetalCore.Smtp"` section.

**Authenticated relay** (Gmail, SendGrid SMTP, AWS SES SMTP, etc.):

```json
"MetalCore.Smtp": {
  "Host": "smtp.example.com",
  "Port": 587,
  "EnableSsl": true,
  "FromEmail": "no-reply@example.com",
  "FromName": "My App",
  "Username": "smtp-user",
  "Password": "smtp-password"
}
```

**Unauthenticated relay** (internal mail relay, local dev SMTP server, etc.) — omit `Username` and `Password`:

```json
"MetalCore.Smtp": {
  "Host": "relay.internal",
  "Port": 25,
  "EnableSsl": false,
  "FromEmail": "no-reply@example.com",
  "FromName": "My App"
}
```

When `Username` and `Password` are both present and non-empty, the client sets `SmtpClient.Credentials` to a `NetworkCredential` before sending. When either is absent, the client sends without credentials.

| Type / Method | Description |
|---|---|
| `SmtpConfig` | Configuration model: `Host`, `Port`, `FromEmail`, `FromName`, `EnableSsl`, `Username?`, `Password?` |
| `AddSmtpEmailService(builder, configSection?)` | Registers `IEmailService` as a singleton SMTP service, binding config from `appsettings` |
| `AddSmtpEmailService(builder, SmtpConfig)` | Registers `IEmailService` from a pre-built `SmtpConfig` |
| `AddSmtpEmailService(builder, Action<SmtpConfig>, configSection?)` | Registers `IEmailService` after binding config and applying a post-bind delegate |

> **SMS:** No concrete `ISmsService` implementation is provided in this package. Register your own implementation against `ISmsService`.

---

## App Builder Fluent Syntax

MetalCore provides extension methods on `WebApplicationBuilder` and `WebApplication` that enable a single fluent chain from `CreateBuilder(args)` through to `Run()`, eliminating the intermediate variable assignments that standard ASP.NET Core startup requires.

```csharp
WebApplication.CreateBuilder(args)
    .AddMetalInjection(_ => _.ScanThisAssembly())
    .AddSmtpEmailService()
    .AddServices((services, config) => services
        .AddCors(...)
        .AddSwaggerGen())
    .Build()
    .UseApp((app, config) => {
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.MapFallbackToFile("index.html");
    })
    .Run();
```

### `WebApplicationBuilder`

| Method | Description | Example |
|---|---|---|
| `AddServices(Action<IServiceCollection>)` | Fluent wrapper for registering DI services | `builder.AddServices(s => s.AddScoped<IFoo, Foo>())` |
| `AddServices(Action<IServiceCollection, IConfiguration>)` | Same, with access to `IConfiguration` | `builder.AddServices((s, cfg) => s.Configure<MyOpts>(cfg))` |

### `WebApplication`

| Method | Description | Example |
|---|---|---|
| `UseApp(Action<WebApplication>)` | Fluent pipeline setup callback on a built `WebApplication` | `app.UseApp(a => a.UseHttpsRedirection())` |
| `UseApp(Action<WebApplication, IConfiguration>)` | Pipeline setup with access to `IConfiguration` | `app.UseApp((a, cfg) => ...)` |

---

## Installation

```powershell
dotnet add package RossWright.MetalCore.Server
```

Or add directly to your project file:

```xml
<PackageReference Include="RossWright.MetalCore.Server" Version="*" />
```

---

## See Also

| Package | Purpose |
|---|---|
| [`RossWright.MetalCore`](https://www.nuget.org/packages/RossWright.MetalCore) | Core extensions, utilities, options builders, load logging, exceptions, signing |
| [`RossWright.MetalCore.Data`](https://www.nuget.org/packages/RossWright.MetalCore.Data) | Entity Framework extensions, GeoCoder, database timing interceptor |
| [`RossWright.MetalCore.Blazor`](https://www.nuget.org/packages/RossWright.MetalCore.Blazor) | Blazor WASM utilities: local storage, JS script loader, host builder extensions |
| [`RossWright.MetalCore.Populi`](https://www.nuget.org/packages/RossWright.MetalCore.Populi) | Zero-dependency test-data generator: names, addresses, emails, coordinates, dates, prices, and lorem ipsum |

---

## License

All **Ross Wright Metal Libraries** including this one are licensed under **Apache License 2.0 with Commons Clause**.

**You are free to**:
- Use the libraries in any project (personal or commercial)
- Modify them
- Include them in products or services you sell

**You may not**:
- Sell the libraries themselves (or any product/service whose *primary* value comes from the libraries)
- Repackage them with minimal changes and sell them as your own standalone product

Full legal text: [LICENSE.md](./LICENSE.md)
