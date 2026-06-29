# Upload Files With MetalNexus

Use this recipe when a Blazor WebAssembly client should upload files to ASP.NET Core handlers through MetalNexus request types.

## Install

Shared contracts:

```bash
dotnet add package RossWright.MetalNexus.Abstractions
```

Server:

```bash
dotnet add package RossWright.MetalNexus.Server
```

Blazor client:

```bash
dotnet add package RossWright.MetalNexus.Blazor
```

## Namespace

```csharp
using RossWright;
using RossWright.MetalNexus;
```

## Shared Request

```csharp
[ApiRequest]
[UploadLimit(10_000_000)]
public sealed class UploadAvatarRequest : MetalNexusFileRequest, IRequest
{
	public Guid CustomerId { get; set; }
}
```

## Blazor Component

```razor
<FileInput FilesPicked="UploadFiles" />
```

```csharp
private async Task UploadFiles(FileInput.IFilesPickedArgs args)
{
	await args.UploadFiles(new UploadAvatarRequest
	{
		CustomerId = CustomerId
	}, args.Files);
}
```

## Reach For This When

- You need Blazor uploads without hand-writing multipart endpoint clients.
- You want upload limits declared on request contracts.
- You want server handlers to stay in the MetalChain pattern.

## Notes For Agents

- Use `RossWright.MetalNexus.Blazor` for `FileInput` and browser file values.
- Keep file limits on the request type with upload attributes.
- Use server-side validation for content type and size even when the client constrains input.
