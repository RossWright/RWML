using RossWright.MetalChain;
using RossWright.MetalCommand;
using RossWright.MetalNexus.Testbed.Shared;
using System.Net.Http.Json;

namespace RossWright.MetalNexus.Testbed.Console.Commands;

// ── 0. Login helper ──────────────────────────────────────────────────────────

/// <summary>Shared helper for ensuring the session has a valid token.</summary>
internal static class AuthHelper
{
    internal static async Task EnsureLoggedInAsync(
        TokenService tokens,
        IHttpClientFactory httpClientFactory,
        string username = "admin",
        string password = "admin",
        bool provisional = false,
        CancellationToken ct = default)
    {
        if (tokens.Token == null)
        {
            var http = httpClientFactory.CreateClient();
            await tokens.LoginAsync(http, username, password, provisional, ct);
        }
    }
}

// ── 1. List Customers ────────────────────────────────────────────────────────

[Command("List Customers", "list-customers",
    HelpBrief = "Lists all customers (Anonymous, Auto GET).",
    Category = "Basics")]
internal class ListCustomersCommand(IMediator mediator) : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        console.WriteLine("Feature: [Anonymous], Auto GET — no token required.");
        console.WriteLine("Code: var result = await Mediator.Send(new GetCustomersRequest());");
        var result = await mediator.Send(new GetCustomersRequest(), ct);
        console.WriteLine($"Result: {result.Customers.Count} customer(s) returned.");
        foreach (var c in result.Customers)
            console.WriteLine($"  [{c.Id}] {c.Name} <{c.Email}>");
        return CommandResult.Ok();
    }
}

// ── 2. Get Customer ──────────────────────────────────────────────────────────

[Command("Get Customer", "get-customer",
    HelpBrief = "Gets a single customer by ID (Anonymous, Auto GET with query param).",
    Category = "Basics")]
internal class GetCustomerCommand(IMediator mediator) : ICommand
{
    [Arg(Name = "id", DefaultValue = "1")]
    public int Id { get; set; }

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        console.WriteLine($"Feature: [Anonymous], Auto GET — single property sent as query param.");
        console.WriteLine($"Code: await Mediator.Send(new GetCustomerRequest {{ Id = {Id} }});");
        var result = await mediator.Send(new GetCustomerRequest { Id = Id }, ct);
        console.WriteLine($"Result: {result.Name} | {result.Email} | {result.Phone}");
        return CommandResult.Ok();
    }
}

// ── 3. Create Customer ───────────────────────────────────────────────────────

[Command("Create Customer", "create-customer",
    HelpBrief = "Creates a new customer (Authenticated, PostViaBody, 201 Created).",
    Category = "Basics")]
internal class CreateCustomerCommand(
    IMediator mediator,
    TokenService tokens,
    IHttpClientFactory httpClientFactory) : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        await AuthHelper.EnsureLoggedInAsync(tokens, httpClientFactory, ct: ct);
        console.WriteLine("Feature: [Authenticated], PostViaBody, SuccessStatusCode = 201 Created.");
        var request = new CreateCustomerRequest
        {
            Name = $"Test Customer {DateTime.Now:HHmmss}",
            Email = $"test{DateTime.Now:HHmmss}@example.com",
            Phone = "555-0100"
        };
        console.WriteLine($"Code: await Mediator.Send(new CreateCustomerRequest {{ Name = \"{request.Name}\", ... }});");
        var result = await mediator.Send(request, ct);
        console.WriteLine($"Result: Created customer Id={result.Id} Name={result.Name}");
        return CommandResult.Ok();
    }
}

// ── 4. Update Customer ───────────────────────────────────────────────────────

[Command("Update Customer", "update-customer",
    HelpBrief = "Updates a customer (Authenticated Admin/Manager, PutViaBody, Location header).",
    Category = "Basics")]
internal class UpdateCustomerCommand(
    IMediator mediator,
    TokenService tokens,
    IHttpClientFactory httpClientFactory) : ICommand
{
    [Arg(Name = "id", DefaultValue = "1")]
    public int Id { get; set; }

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        await AuthHelper.EnsureLoggedInAsync(tokens, httpClientFactory, ct: ct);
        console.WriteLine("Feature: [Authenticated(Admin, Manager)], PutViaBody, IMetalNexusResponseContext Location header.");
        var request = new UpdateCustomerRequest
        {
            Id = Id,
            Name = $"Updated Name {DateTime.Now:HHmmss}",
            Email = $"updated{DateTime.Now:HHmmss}@example.com",
            Phone = "555-0200"
        };
        console.WriteLine($"Code: await Mediator.Send(new UpdateCustomerRequest {{ Id = {Id}, ... }});");
        var result = await mediator.Send(request, ct);
        console.WriteLine($"Result: Updated customer Id={result.Id} Name={result.Name}");
        return CommandResult.Ok();
    }
}

// ── 5. Delete Customer ───────────────────────────────────────────────────────

[Command("Delete Customer", "delete-customer",
    HelpBrief = "Deletes a customer (Admin only, DELETE).",
    Category = "Basics")]
internal class DeleteCustomerCommand(
    IMediator mediator,
    TokenService tokens,
    IHttpClientFactory httpClientFactory) : ICommand
{
    [Arg(Name = "id", DefaultValue = "5")]
    public int Id { get; set; }

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        await AuthHelper.EnsureLoggedInAsync(tokens, httpClientFactory, ct: ct);
        console.WriteLine("Feature: [Authenticated(Admin)], HttpProtocol.Delete.");
        console.WriteLine($"Code: await Mediator.Send(new DeleteCustomerRequest {{ Id = {Id} }});");
        await mediator.Send(new DeleteCustomerRequest { Id = Id }, ct);
        console.WriteLine($"Result: Customer {Id} deleted successfully.");
        return CommandResult.Ok();
    }
}

// ── 6. Not Found Error ───────────────────────────────────────────────────────

[Command("Not Found Error", "not-found-error",
    HelpBrief = "Triggers CustomerNotFoundException by requesting a nonexistent customer.",
    Category = "Errors")]
internal class NotFoundErrorCommand(IMediator mediator) : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        console.WriteLine("Feature: Exception marshalling — CustomerNotFoundException reconstructed on client.");
        console.WriteLine("Code: await Mediator.Send(new GetCustomerRequest { Id = 99999 });");
        try
        {
            await mediator.Send(new GetCustomerRequest { Id = 99999 }, ct);
            console.WriteErrorLine("Expected CustomerNotFoundException but none was thrown.");
            return CommandResult.Fail();
        }
        catch (CustomerNotFoundException ex)
        {
            console.WriteLine($"Caught (exact type): {ex.GetType().Name}: {ex.Message}");
            return CommandResult.Ok();
        }
    }
}

// ── 7. Validation Error ──────────────────────────────────────────────────────

[Command("Validation Error", "validation-error",
    HelpBrief = "Triggers DuplicateEmailException by creating a customer with a duplicate email.",
    Category = "Errors")]
internal class ValidationErrorCommand(
    IMediator mediator,
    TokenService tokens,
    IHttpClientFactory httpClientFactory) : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        await AuthHelper.EnsureLoggedInAsync(tokens, httpClientFactory, ct: ct);
        // Use bob@example.com (customer 2) — customer 1's email is overwritten by update-customer
        console.WriteLine("Feature: [ProducesError<DuplicateEmailException>], 422 mapping.");
        console.WriteLine("Code: await Mediator.Send(new CreateCustomerRequest { Email = \"bob@example.com\", ... });");
        try
        {
            await mediator.Send(new CreateCustomerRequest
            {
                Name = "Duplicate Bob",
                Email = "bob@example.com",
                Phone = "555-0000"
            }, ct);
            console.WriteErrorLine("Expected DuplicateEmailException but none was thrown.");
            return CommandResult.Fail();
        }
        catch (DuplicateEmailException ex)
        {
            console.WriteLine($"Caught (exact type): {ex.GetType().Name}: {ex.Message}");
            return CommandResult.Ok();
        }
    }
}

// ── 8. Authorization Error ───────────────────────────────────────────────────

[Command("Auth Error", "auth-error",
    HelpBrief = "Triggers NotAuthorizedException by calling an Admin-only endpoint as Manager.",
    Category = "Errors")]
internal class AuthErrorCommand(
    IMediator mediator,
    TokenService tokens,
    IHttpClientFactory httpClientFactory) : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        // Re-login as manager
        tokens.ClearToken();
        var http = httpClientFactory.CreateClient();
        await tokens.LoginAsync(http, "manager", "manager", ct: ct);
        console.WriteLine("Feature: 403 marshalling, NotAuthorizedException.");
        console.WriteLine("Logged in as: manager. Attempting DeleteCustomerRequest (Admin only)...");
        console.WriteLine("Code: await Mediator.Send(new DeleteCustomerRequest { Id = 1 });");
        try
        {
            await mediator.Send(new DeleteCustomerRequest { Id = 1 }, ct);
            console.WriteErrorLine("Expected NotAuthorizedException but none was thrown.");
            return CommandResult.Fail();
        }
        catch (NotAuthorizedException ex)
        {
            console.WriteLine($"Caught (exact type): {ex.GetType().Name}: {ex.Message}");
            return CommandResult.Ok();
        }
        finally
        {
            // Restore admin token for subsequent commands
            tokens.ClearToken();
            await tokens.LoginAsync(httpClientFactory.CreateClient(), "admin", "admin", ct: ct);
        }
    }
}

// ── 9. Correlation ID ────────────────────────────────────────────────────────

[Command("Correlation ID", "correlation-id",
    HelpBrief = "Sends a request with X-Correlation-Id header via [FromHeader].",
    Category = "Headers")]
internal class CorrelationIdCommand(IMediator mediator) : ICommand
{
    [Arg(Name = "id", DefaultValue = "1")]
    public int Id { get; set; }

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid().ToString();
        console.WriteLine("Feature: [FromHeader] sends property as X-Correlation-Id HTTP header.");
        console.WriteLine($"CorrelationId: {correlationId}");
        console.WriteLine("Code: await Mediator.Send(new GetCustomerWithCorrelationRequest { Id = ..., CorrelationId = ... });");
        var result = await mediator.Send(new GetCustomerWithCorrelationRequest
        {
            Id = Id,
            CorrelationId = correlationId
        }, ct);
        console.WriteLine($"Result: {result.Name} | Echoed CorrelationId={result.CorrelationId}");
        return CommandResult.Ok();
    }
}
