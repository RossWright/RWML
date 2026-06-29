# MetalNexus Testbed

An interactive testbed for [MetalNexus](../README.md) that demonstrates every feature of the library through a working customer management system.

## Projects

| Project | Description |
|---|---|
| `MetalNexus.Testbed.Shared` | Shared request/response types, DTOs, and exceptions |
| `MetalNexus.Testbed.Server` | ASP.NET Core server with in-memory repository |
| `MetalNexus.Testbed.Blazor` | Blazor WebAssembly interactive testbed UI |
| `MetalNexus.Testbed.Console` | MetalCommand console client |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022+ or VS Code with C# extension (optional)

---

## Running the Testbed

### 1 — Start the Server

```powershell
cd MetalNexus\RossWright.MetalNexus.Testbed.Server
dotnet run
```

The server starts at:
- **HTTPS:** `https://localhost:58386`
- **HTTP:** `http://localhost:58387`
- **Swagger UI:** `https://localhost:58386/swagger`

> The Swagger UI shows all 24 demo endpoints, including the deprecated `GetCustomersV1` endpoint marked with `deprecated: true`.

Hardcoded credentials for the `/auth/token` endpoint:

| Username | Password | Role |
|---|---|---|
| `admin` | `admin` | Admin |
| `manager` | `manager` | Manager |
| `readonly` | `readonly` | ReadOnly |

Obtain a token:

```powershell
Invoke-RestMethod -Method Post -Uri "https://localhost:58386/auth/token" `
    -ContentType "application/json" `
    -Body '{"username":"admin","password":"admin"}'
```

---

### 2 — Run the Blazor Client

The Blazor app requires the server to be running first.

```powershell
cd MetalNexus\RossWright.MetalNexus.Testbed.Blazor
dotnet run
```

The Blazor app starts at:
- **HTTPS:** `https://localhost:58379`
- **HTTP:** `http://localhost:58380`

Open `https://localhost:58379` in your browser. The two-panel UI shows:
- **Left panel** — grouped list of all 24 test functions plus a **Test All** button
- **Right panel** — live tutorial log with narrative, code snippet, and result for each function

Use the auth bar (top-right) to switch between `admin`, `manager`, `readonly`, and unauthenticated. This controls which endpoints will succeed vs. produce auth errors.

---

### 3 — Run the Console Client

The console client requires the server to be running first.

```powershell
cd MetalNexus\RossWright.MetalNexus.Testbed.Console
dotnet run -- test-all
```

This runs every command in sequence and prints verbose tutorial output. Individual commands are also available:

```powershell
dotnet run -- list-customers
dotnet run -- get-customer
dotnet run -- create-customer
dotnet run -- update-customer
dotnet run -- delete-customer
dotnet run -- not-found-error
dotnet run -- validation-error
dotnet run -- auth-error
dotnet run -- correlation-id
dotnet run -- add-note
dotnet run -- webhook
dotnet run -- upload-avatar
dotnet run -- upload-docs
dotnet run -- upload-profile
dotnet run -- download-doc
dotnet run -- csv-export
dotnet run -- content-neg
dotnet run -- slow-request
dotnet run -- deprecated
dotnet run -- no-content
dotnet run -- mfa
dotnet run -- send-via
dotnet run -- adv-register
```

The console automatically authenticates as `admin` before running each command. The `auth-error` command intentionally uses a `manager` token to demonstrate 403 handling.

---

## Feature Coverage

| # | Feature | MetalNexus Capability Demonstrated |
|---|---|---|
| 1 | List Customers | `[Anonymous]`, Auto GET (zero properties) |
| 2 | Get Customer | Auto GET with query parameter |
| 3 | Create Customer | `PostViaBody`, `[Authenticated]`, `SuccessStatusCode = Created` |
| 4 | Update Customer | `PutViaBody`, `IMetalNexusResponseContext`, Location header |
| 5 | Delete Customer | `Delete`, Admin-only auth |
| 6 | Not Found Error | Exception marshalling — `CustomerNotFoundException` reconstructed on client |
| 7 | Validation Error | `DuplicateEmailException` (subclass of `ValidationException`), 422 |
| 8 | Auth Error | `NotAuthorizedException`, 403 marshalling |
| 9 | Correlation ID | `[FromHeader]` sends property as HTTP request header |
| 10 | Add Note | `PatchViaBody` |
| 11 | Webhook | `IMetalNexusRawRequest`, raw body access for HMAC verification |
| 12 | Upload Avatar | `MetalNexusFileRequest`, `[UploadLimit]`, `[AllowedFileTypes]`, `[MaxFileSize]` |
| 13 | Upload Documents | `[MaxFileCount]`, `[NoUploadLimit]`, multi-file upload |
| 14 | Upload Profile Pack | `[FileSlot]` named slots, per-slot attribute override |
| 15 | Download Document | Handler returns `MetalNexusFile`, `IsAttachment = true` |
| 16 | Direct Download Link | `IMetalNexusUrlHelper.GetUrlFor` — URL only, no HTTP call |
| 17 | CSV Export | `IMetalNexusRawResponse`, non-JSON response body |
| 18 | Content Negotiation | `IMetalNexusRequestContext.AcceptHeader`, JSON or CSV based on `Accept` |
| 19 | Custom Timeout | `[HttpClientTimeout]`, `TaskCanceledException` on timeout |
| 20 | Deprecated Endpoint | `[Obsolete]` → `deprecated: true` in Swagger |
| 21 | NoContent Response | `IMetalNexusResponseContext.StatusCode = NoContent`, 204 |
| 22 | AllowProvisional Auth | `[Authenticated(AllowProvisional = true)]`, MFA simulation |
| 23 | Multiple Connections | Named `HttpClient`, `SendVia("connection-b", ...)` |
| 24 | Late Registration | `AddMetalNexusEndpoints(typeof(LateRegisteredRequest))` |

---

## Build

```powershell
dotnet build MetalNexus\RossWright.MetalNexus.Testbed.sln
```
