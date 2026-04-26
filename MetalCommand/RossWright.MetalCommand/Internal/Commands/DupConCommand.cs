using System.Diagnostics;
using System.Text.Json;

namespace RossWright.MetalCommand.Internal.Commands;

[Command("DupCon", "dupcon", HelpBrief = "Opens a duplicate console in a new terminal window", Category = "Console")]
internal class DupConCommand(
    IDupConTerminalLauncher launcher,
    DupConCommandOptions opts,
    IDictionary<string, string> context) : ICommand
{
    [Arg(Name = "context", IsRequired = false)]
    public bool ForwardContext { get; set; }

    [Arg(Name = "cmd", IsRequired = false, AllowNamed = true)]
    public string? InitialCommand { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        var exePath = Environment.ProcessPath
            ?? Process.GetCurrentProcess().MainModule!.FileName;

        var argParts = new List<string>();

        var shouldForwardContext = ForwardContext || opts.DefaultForwardContext;
        if (shouldForwardContext)
        {
            var snapshot = new Dictionary<string, string>(context)
            {
                ["__dupcon_parent_pid"] = Environment.ProcessId.ToString()
            };
            var blob = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(snapshot));
            argParts.Add($"--ctx {blob}");
        }

        if (!string.IsNullOrWhiteSpace(InitialCommand))
            argParts.Add($"--cmd \"{InitialCommand}\"");

        var launched = launcher.Launch(exePath!, string.Join(' ', argParts));
        if (!launched)
            console.WriteErrorLine("No suitable terminal emulator found. Cannot open a new window.");

        return Task.FromResult(launched
            ? CommandResult.Ok()
            : CommandResult.Fail("No suitable terminal emulator found. Cannot open a new window."));
    }
}
