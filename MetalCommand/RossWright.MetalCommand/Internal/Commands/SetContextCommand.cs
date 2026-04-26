namespace RossWright.MetalCommand.Internal.Commands;

[Command("SetContext", "setcontext", HelpBrief = "Sets a key/value pair in the current session context", Category = "Console")]
internal class SetContextCommand(IDictionary<string, string> context) : ICommand
{
    [Arg(Name = "key", IsRequired = true)]
    public string Key { get; set; } = string.Empty;

    [Arg(Name = "value", IsRequired = true)]
    public string Value { get; set; } = string.Empty;

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        context[Key] = Value;
        console.WriteLine($"{Key} = {Value}");
        return Task.FromResult(CommandResult.Ok());
    }
}
