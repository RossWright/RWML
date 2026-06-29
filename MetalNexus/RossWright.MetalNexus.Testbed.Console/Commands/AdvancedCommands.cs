using RossWright.MetalChain;
using RossWright.MetalCommand;
using RossWright.MetalNexus;
using RossWright.MetalNexus.Testbed.Shared;
using System.Net.Http.Headers;
using System.Text;

namespace RossWright.MetalNexus.Testbed.Console.Commands;

// ── 17. CSV Export ────────────────────────────────────────────────────────────

[Command("CSV Export", "csv-export",
    HelpBrief = "Returns raw CSV bytes via IMetalNexusRawResponse.",
    Category = "Advanced")]
internal class CsvExportCommand(IMediator mediator) : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        console.WriteLine("Feature: IMetalNexusRawResponse — non-JSON response; client receives typed wrapper.");
        console.WriteLine("Code: await Mediator.Send(new ExportCustomersCsvRequest());");
        var result = await mediator.Send(new ExportCustomersCsvRequest(), ct);
        var text = result.Data != null ? Encoding.UTF8.GetString(result.Data) : "(no data)";
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        console.WriteLine($"Result: ContentType={result.ContentType} Rows={lines.Length}");
        if (lines.Length > 0) console.WriteLine($"  Header: {lines[0]}");
        return CommandResult.Ok();
    }
}

// ── 18. Content Negotiation ───────────────────────────────────────────────────

[Command("Content Negotiation", "content-neg",
    HelpBrief = "Demonstrates IMetalNexusRequestContext Accept-header inspection.",
    Category = "Advanced")]
internal class ContentNegCommand(IMediator mediator) : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        console.WriteLine("Feature: Server inspects Accept header via IMetalNexusRequestContext; returns CSV or JSON.");
        // Note: Accept header injection is Blazor/HttpClient-level; IMediator.Send() uses JSON by default.
        // We demonstrate by invoking the endpoint which defaults to JSON without an Accept override.
        console.WriteLine("Code: await Mediator.Send(new ExportCustomersNegotiatedRequest());");
        var result = await mediator.Send(new ExportCustomersNegotiatedRequest(), ct);
        if (result?.Data != null)
        {
            console.WriteLine($"Result: ContentType={result.ContentType} Bytes={result.Data.Length}");
        }
        else if (result?.DataStream != null)
        {
            console.WriteLine($"Result: ContentType={result.ContentType} (streamed)");
        }
        else
        {
            console.WriteLine("Result: (empty body)");
        }
        return CommandResult.Ok();
    }
}

// ── 19. Slow Request ──────────────────────────────────────────────────────────

[Command("Slow Request", "slow-request",
    HelpBrief = "Demonstrates per-request [HttpClientTimeout(5)] — runs fast then forces timeout.",
    Category = "Advanced")]
internal class SlowRequestCommand(
    IMediator mediator,
    TokenService tokens,
    IHttpClientFactory httpClientFactory) : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        await AuthHelper.EnsureLoggedInAsync(tokens, httpClientFactory, ct: ct);
        console.WriteLine("Feature: [HttpClientTimeout(5)] — 5-second per-request timeout on the client.");
        console.WriteLine();
        console.WriteLine("--- Run 1: fast request (no timeout) ---");
        console.WriteLine("Code: await Mediator.Send(new SlowReportRequest { ForceTimeout = false });");
        var result = await mediator.Send(new SlowReportRequest { ForceTimeout = false }, ct);
        console.WriteLine($"Result: {result.CustomerCount} customers in {result.ElapsedSeconds:F2}s");
        console.WriteLine();
        console.WriteLine("--- Run 2: forced timeout ---");
        console.WriteLine("Code: await Mediator.Send(new SlowReportRequest { ForceTimeout = true });");
        try
        {
            await mediator.Send(new SlowReportRequest { ForceTimeout = true }, ct);
            console.WriteErrorLine("Expected timeout but none occurred.");
        }
        catch (TimeoutException)
        {
            console.WriteLine("Caught: TimeoutException — client-side timeout fired as expected.");
        }
        return CommandResult.Ok();
    }
}

// ── 20. Deprecated ────────────────────────────────────────────────────────────

[Command("Deprecated Endpoint", "deprecated",
    HelpBrief = "Calls the [Obsolete] GetCustomersV1Request — deprecated in Swagger.",
    Category = "Advanced")]
internal class DeprecatedCommand(IMediator mediator) : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        console.WriteLine("Feature: [Obsolete] marks endpoint as deprecated: true in OpenAPI spec.");
#pragma warning disable CS0618
        var result = await mediator.Send(new GetCustomersV1Request(), ct);
#pragma warning restore CS0618
        console.WriteLine($"Result: {result.Customers.Count} customers returned via deprecated endpoint.");
        console.WriteLine("Note: Swagger shows deprecated: true for this endpoint.");
        return CommandResult.Ok();
    }
}

// ── 21. NoContent ─────────────────────────────────────────────────────────────

[Command("NoContent", "no-content",
    HelpBrief = "Calls PurgeAuditLogRequest — expects 204 No Content (Admin only).",
    Category = "Advanced")]
internal class NoContentCommand(
    IMediator mediator,
    TokenService tokens,
    IHttpClientFactory httpClientFactory) : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        await AuthHelper.EnsureLoggedInAsync(tokens, httpClientFactory, ct: ct);
        console.WriteLine("Feature: IRequest (no response), 204 NoContent set via IMetalNexusResponseContext.");
        console.WriteLine("Code: await Mediator.Send(new PurgeAuditLogRequest());");
        await mediator.Send(new PurgeAuditLogRequest(), ct);
        console.WriteLine("Result: Completed silently (204 No Content).");
        return CommandResult.Ok();
    }
}

// ── 22. AllowProvisional / MFA ────────────────────────────────────────────────

[Command("MFA", "mfa",
    HelpBrief = "Demonstrates AllowProvisional = true with a provisional JWT token.",
    Category = "Advanced")]
internal class MfaCommand(
    IMediator mediator,
    TokenService tokens,
    IHttpClientFactory httpClientFactory) : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        console.WriteLine("Feature: [Authenticated(AllowProvisional = true)] — provisional token accepted.");
        console.WriteLine("Step 1: Obtain a provisional token for admin.");
        tokens.ClearToken();
        var http = httpClientFactory.CreateClient();
        await tokens.LoginAsync(http, "admin", "admin", provisional: true, ct: ct);
        console.WriteLine($"Logged in as: {tokens.Username}");
        console.WriteLine("Step 2: Call ConfirmMfaRequest — only allowed with provisional or full token.");
        console.WriteLine("Code: await Mediator.Send(new ConfirmMfaRequest { Code = \"123456\" });");
        var result = await mediator.Send(new ConfirmMfaRequest { Code = "123456" }, ct);
        console.WriteLine($"Result: Success={result.Success} Message=\"{result.Message}\"");
        // Restore normal token
        tokens.ClearToken();
        await tokens.LoginAsync(httpClientFactory.CreateClient(), "admin", "admin", ct: ct);
        return CommandResult.Ok();
    }
}

// ── 23. SendVia ───────────────────────────────────────────────────────────────

[Command("SendVia", "send-via",
    HelpBrief = "Demonstrates named connections + SendVia for routing to a specific HttpClient.",
    Category = "Advanced")]
internal class SendViaCommand(IMediator mediator) : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        console.WriteLine("Feature: Named HttpClient connections + IMediator.SendVia(connectionName, request).");
        console.WriteLine("Code (default): await Mediator.Send(new GetCustomersRequest());");
        var defaultResult = await mediator.Send(new GetCustomersRequest(), ct);
        console.WriteLine($"Default connection: {defaultResult.Customers.Count} customers.");
        console.WriteLine("Code (connection-b): await Mediator.SendVia(\"connection-b\", new GetCustomersRequest(), ct);");
        var connBResult = await mediator.SendVia<GetCustomersResponse>("connection-b", new GetCustomersRequest(), ct);
        console.WriteLine($"connection-b: {connBResult?.Customers.Count} customers (same server, demonstrates routing).");
        return CommandResult.Ok();
    }
}

// ── 24. Advanced Registration ─────────────────────────────────────────────────

[Command("Advanced Registration", "adv-register",
    HelpBrief = "Calls LateRegisteredRequest — registered via AddMetalNexusEndpoints after main setup.",
    Category = "Advanced")]
internal class AdvRegisterCommand(IMediator mediator) : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        console.WriteLine("Feature: AddMetalNexusEndpoints registers request types after the main AddMetalNexusServer call.");
        console.WriteLine("Code: await Mediator.Send(new LateRegisteredRequest());");
        var result = await mediator.Send(new LateRegisteredRequest(), ct);
        console.WriteLine($"Result: \"{result.Message}\"");
        return CommandResult.Ok();
    }
}

// ── Test All ──────────────────────────────────────────────────────────────────

[Command("Test All", "test-all",
    HelpBrief = "Runs every testbed command in sequence with verbose tutorial output.",
    Category = "Console")]
internal class TestAllCommand(ICommandExecutor executor) : ICommand
{
    // null command = intentional skip (e.g. requires interactive arg)
    private static readonly (string? Command, string Description)[] _steps =
    [
        ("list-customers",  "1. List Customers — Anonymous, Auto GET"),
        ("get-customer",    "2. Get Customer — Anonymous, Auto GET with query param"),
        ("create-customer", "3. Create Customer — Authenticated, PostViaBody, 201"),
        ("update-customer", "4. Update Customer — Authenticated Admin/Manager, PutViaBody"),
        ("delete-customer", "5. Delete Customer — Authenticated, Admin only, DELETE"),
        ("not-found-error", "6. Not Found Error — CustomerNotFoundException"),
        ("validation-error","7. Validation Error — DuplicateEmailException"),
        ("auth-error",      "8. Auth Error — NotAuthorizedException as Manager"),
        ("correlation-id",  "9. Correlation ID — [FromHeader]"),
        ("add-note",        "10. Add Note — PatchViaBody"),
        ("webhook",         "11. Webhook — IMetalNexusRawRequest"),
        ("upload-avatar",   "12. Upload Avatar — single file, [AllowedFileTypes]"),
        ("upload-docs",     "13. Upload Documents — multi-file, [MaxFileCount]"),
        ("upload-profile",  "14. Upload Profile Pack — [FileSlot] named slots"),
        ("download-doc",    "15. Download Document — returns MetalNexusFile"),
        ("csv-export",      "17. CSV Export — IMetalNexusRawResponse"),
        ("content-neg",     "18. Content Negotiation — IMetalNexusRequestContext"),
        ("slow-request",    "19. Slow Request — [HttpClientTimeout]"),
        ("deprecated",      "20. Deprecated Endpoint — [Obsolete]"),
        ("no-content",      "21. NoContent — 204 via IMetalNexusResponseContext"),
        ("mfa",             "22. AllowProvisional — provisional JWT"),
        ("send-via",        "23. SendVia — named connections"),
        ("adv-register",    "24. Advanced Registration — AddMetalNexusEndpoints"),
    ];

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        console.WriteLine("=== MetalNexus Testbed — Test All ===");
        console.WriteLine($"Running {_steps.Length} commands in sequence.");
        console.WriteLine();

        int passed = 0, failed = 0, skipped = 0;
        foreach (var (command, description) in _steps)
        {
            console.WriteLine($"──── {description} ────", ConsoleColor.Cyan);
            if (command is null)
            {
                console.WriteLine("Skipped.", ConsoleColor.DarkGray);
                skipped++;
            }
            else
            {
                try
                {
                    await executor.Execute(command);
                    passed++;
                }
                catch (Exception ex)
                {
                    console.WriteErrorLine($"  FAILED: {ex.Message}");
                    failed++;
                }
            }
            console.WriteLine();
        }

        var summary = skipped > 0
            ? $"=== Complete: {passed} passed, {failed} failed, {skipped} skipped ==="
            : $"=== Complete: {passed} passed, {failed} failed ===";
        console.WriteLine(summary, failed == 0 ? ConsoleColor.Green : ConsoleColor.Yellow);
        return failed == 0 ? CommandResult.Ok() : CommandResult.Fail($"{failed} command(s) failed.");
    }
}
