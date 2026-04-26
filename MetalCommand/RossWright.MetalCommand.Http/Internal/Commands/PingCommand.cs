using RossWright.MetalCommand.Http;
using System.Diagnostics;

namespace RossWright.MetalCommand.Http.Commands;

/// <summary>
/// Built-in command that sends a GET request to the active HTTP environment and
/// reports the status code and latency.
/// </summary>
[Command("Ping", "ping",
    HelpBrief = "Ping the HTTP endpoint for the active environment",
    Category = "HTTP")]
internal sealed class PingCommand(IHttpConnectionResolver resolver) : ICommand
{
    /// <summary>The HTTP environment to ping. Defaults to the configured default environment.</summary>
    [EnvironmentArg(EnvironmentPolicy.Benign,
        HelpDetail = "The HTTP environment to ping")]
    public string? Environment { get; set; }

    /// <summary>Request timeout in seconds.</summary>
    [Arg]
    public int TimeoutSeconds { get; set; } = 10;

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(TimeoutSeconds));

        string clientName;
        HttpClient client;
        try
        {
            clientName = resolver.GetClientName(Environment, null);
            client = resolver.GetClient(Environment, null);
        }
        catch (InvalidOperationException ex)
        {
            console.WriteErrorLine(ex.Message);
            return CommandResult.Fail(ex.Message);
        }

        console.WriteLine($"Pinging [{clientName}] ({client.BaseAddress})...");

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await client.GetAsync("/", cts.Token);
            sw.Stop();
            var color = response.IsSuccessStatusCode ? ConsoleColor.Green : ConsoleColor.Yellow;
            console.WriteLine($"  Status : {(int)response.StatusCode} {response.ReasonPhrase}", color);
            console.WriteLine($"  Latency: {sw.ElapsedMilliseconds} ms");
            return response.IsSuccessStatusCode ? CommandResult.Ok() : CommandResult.Fail();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            sw.Stop();
            console.WriteErrorLine($"  Timed out after {TimeoutSeconds}s.");
            return CommandResult.Fail("Timeout");
        }
    }
}
