using System.Net.Http.Headers;
using RossWright.MetalChain;
using RossWright.MetalCommand;
using RossWright.MetalNexus.Testbed.Shared;
using System.Text.Json;

namespace RossWright.MetalNexus.Testbed.Console.Commands;

// ── 10. Add Note ─────────────────────────────────────────────────────────────

[Command("Add Note", "add-note",
    HelpBrief = "Adds a note to a customer via PATCH.",
    Category = "Notes")]
internal class AddNoteCommand(
    IMediator mediator,
    TokenService tokens,
    IHttpClientFactory httpClientFactory) : ICommand
{
    [Arg(Name = "customerId", DefaultValue = "1")]
    public int CustomerId { get; set; }

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        await AuthHelper.EnsureLoggedInAsync(tokens, httpClientFactory, ct: ct);
        console.WriteLine("Feature: PatchViaBody — PATCH method for partial update.");
        var text = $"Console note at {DateTime.Now:HH:mm:ss}";
        console.WriteLine($"Code: await Mediator.Send(new AddCustomerNoteRequest {{ CustomerId = {CustomerId}, Text = \"{text}\" }});");
        var result = await mediator.Send(new AddCustomerNoteRequest
        {
            CustomerId = CustomerId,
            Text = text
        }, ct);
        console.WriteLine($"Result: NoteId={result.Id} Text=\"{result.Text}\"");
        return CommandResult.Ok();
    }
}

// ── 11. Webhook ───────────────────────────────────────────────────────────────

[Command("Webhook", "webhook",
    HelpBrief = "Simulates a webhook POST with a raw request body (IMetalNexusRawRequest).",
    Category = "Notes")]
internal class WebhookCommand(IMediator mediator) : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        console.WriteLine("Feature: IMetalNexusRawRequest — server receives unparsed body for HMAC verification.");
        var payload = """{"event":"customer.updated","id":1,"sig":"abc123"}""";
        console.WriteLine($"Code: await Mediator.Send(new CustomerWebhookRequest {{ RawRequestBody = \"{payload}\" }});");
        var result = await mediator.Send(new CustomerWebhookRequest
        {
            RawRequestBody = payload
        }, ct);
        console.WriteLine($"Result: Accepted={result.Accepted} Message=\"{result.Message}\"");
        return CommandResult.Ok();
    }
}

// ── 12. Upload Avatar ─────────────────────────────────────────────────────────

[Command("Upload Avatar", "upload-avatar",
    HelpBrief = "Uploads a synthetic PNG avatar file (MetalNexusFileRequest, single file).",
    Category = "Files")]
internal class UploadAvatarCommand(
    IMetalNexusUrlHelper urlHelper,
    TokenService tokens,
    IHttpClientFactory httpClientFactory) : ICommand
{
    [Arg(Name = "customerId", DefaultValue = "1")]
    public int CustomerId { get; set; }

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        await AuthHelper.EnsureLoggedInAsync(tokens, httpClientFactory, ct: ct);
        console.WriteLine("Feature: MetalNexusFileRequest, [UploadLimit], [AllowedFileTypes], [MaxFileSize], single file.");
        console.WriteLine("The console synthesizes a minimal 1×1 PNG; in Blazor a <FileInput> is used.");

        var pngBytes = SyntheticFiles.MinimalPng();
        var url = urlHelper.GetUrlFor(new UploadCustomerAvatarRequest { CustomerId = CustomerId });
        console.WriteLine($"Upload URL: {url}");

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(pngBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        form.Add(fileContent, "files", "avatar.png");

        var http = httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Add("X-MetalNexus-Client", "true");
        var response = await http.PostAsync(url, form, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        var dto = JsonSerializer.Deserialize<CustomerDto>(body, JsonOptions.Default);
        console.WriteLine($"Result: Customer {dto?.Id} avatar set. Documents: {dto?.Documents.Count}");
        return CommandResult.Ok();
    }
}

// ── 13. Upload Documents ──────────────────────────────────────────────────────

[Command("Upload Documents", "upload-docs",
    HelpBrief = "Uploads multiple synthetic documents (MetalNexusFileRequest, multi-file, [MaxFileCount]).",
    Category = "Files")]
internal class UploadDocsCommand(
    IMetalNexusUrlHelper urlHelper,
    TokenService tokens,
    IHttpClientFactory httpClientFactory) : ICommand
{
    [Arg(Name = "customerId", DefaultValue = "1")]
    public int CustomerId { get; set; }

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        await AuthHelper.EnsureLoggedInAsync(tokens, httpClientFactory, ct: ct);
        console.WriteLine("Feature: MetalNexusFileRequest, [MaxFileCount(5)], [NoUploadLimit], multi-file.");

        var url = urlHelper.GetUrlFor(new UploadCustomerDocumentsRequest { CustomerId = CustomerId });
        console.WriteLine($"Upload URL: {url}");

        using var form = new MultipartFormDataContent();
        for (var i = 1; i <= 2; i++)
        {
            var pdfBytes = SyntheticFiles.MinimalPdf($"Document {i}");
            var fileContent = new ByteArrayContent(pdfBytes);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
            form.Add(fileContent, "files", $"document{i}.pdf");
        }

        var http = httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Add("X-MetalNexus-Client", "true");
        var response = await http.PostAsync(url, form, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        var dto = JsonSerializer.Deserialize<CustomerDto>(body, JsonOptions.Default);
        console.WriteLine($"Result: Customer {dto?.Id} now has {dto?.Documents.Count} document(s).");
        return CommandResult.Ok();
    }
}

// ── 14. Upload Profile Pack ───────────────────────────────────────────────────

[Command("Upload Profile Pack", "upload-profile",
    HelpBrief = "Uploads named file slots (avatar + document) using [FileSlot] overload.",
    Category = "Files")]
internal class UploadProfileCommand(
    IMetalNexusUrlHelper urlHelper,
    TokenService tokens,
    IHttpClientFactory httpClientFactory) : ICommand
{
    [Arg(Name = "customerId", DefaultValue = "1")]
    public int CustomerId { get; set; }

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        await AuthHelper.EnsureLoggedInAsync(tokens, httpClientFactory, ct: ct);
        console.WriteLine("Feature: [FileSlot] named slots — avatar (image) and document (PDF) with per-slot [AllowedFileTypes] override.");

        var url = urlHelper.GetUrlFor(new UploadProfilePackRequest { CustomerId = CustomerId });
        console.WriteLine($"Upload URL: {url}");

        using var form = new MultipartFormDataContent();

        var pngBytes = SyntheticFiles.MinimalPng();
        var avatarContent = new ByteArrayContent(pngBytes);
        avatarContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        form.Add(avatarContent, "avatar", "profile.png");

        var pdfBytes = SyntheticFiles.MinimalPdf("Profile Document");
        var docContent = new ByteArrayContent(pdfBytes);
        docContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
        form.Add(docContent, "document", "profile.pdf");

        var http = httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Add("X-MetalNexus-Client", "true");
        var response = await http.PostAsync(url, form, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        var dto = JsonSerializer.Deserialize<CustomerDto>(body, JsonOptions.Default);
        console.WriteLine($"Result: Customer {dto?.Id} profile pack uploaded. Documents: {dto?.Documents.Count}");
        return CommandResult.Ok();
    }
}

// ── 15. Download Document ─────────────────────────────────────────────────────

[Command("Download Document", "download-doc",
    HelpBrief = "Downloads a customer document and saves it to a temp file.",
    Category = "Files")]
internal class DownloadDocCommand(
    IMediator mediator,
    TokenService tokens,
    IHttpClientFactory httpClientFactory) : ICommand
{
    [Arg(Name = "customerId", DefaultValue = "1")]
    public int CustomerId { get; set; }

    [Arg(Name = "documentId", DefaultValue = "1")]
    public int DocumentId { get; set; }

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        await AuthHelper.EnsureLoggedInAsync(tokens, httpClientFactory, ct: ct);
        console.WriteLine("Feature: Handler returns MetalNexusFile — client receives typed file object.");
        console.WriteLine($"Code: await Mediator.Send(new DownloadCustomerDocumentRequest {{ CustomerId = {CustomerId}, DocumentId = {DocumentId} }});");
        var file = await mediator.Send(new DownloadCustomerDocumentRequest
        {
            CustomerId = CustomerId,
            DocumentId = DocumentId
        }, ct);
        var tempPath = Path.Combine(Path.GetTempPath(), file.FileName ?? "download.bin");
        await File.WriteAllBytesAsync(tempPath, file.Data!, ct);
        console.WriteLine($"Result: Downloaded \"{file.FileName}\" ({file.Data?.Length} bytes) → {tempPath}");
        return CommandResult.Ok();
    }
}

// ── Synthetic file helpers ────────────────────────────────────────────────────

internal static class SyntheticFiles
{
    /// <summary>Returns a minimal valid 1×1 PNG in memory.</summary>
    internal static byte[] MinimalPng() => Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

    /// <summary>Returns a minimal valid PDF in memory with a given title.</summary>
    internal static byte[] MinimalPdf(string title)
    {
        var content = $"%PDF-1.4\n1 0 obj<</Title({title})>>endobj\nxref\n0 2\n0000000000 65535 f\n0000000009 00000 n\ntrailer<</Size 2/Root 1 0 R>>\nstartxref\n9\n%%EOF";
        return System.Text.Encoding.Latin1.GetBytes(content);
    }
}

internal static class JsonOptions
{
    internal static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}
