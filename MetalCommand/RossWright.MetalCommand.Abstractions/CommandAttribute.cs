namespace RossWright.MetalCommand;

/// <summary>
/// Declares a class as an attribute-driven MetalCommand command.
/// Replaces the <see cref="CommandDescriptor"/> property required by <see cref="ILegacyCommand"/>.
/// </summary>
/// <param name="name">Display name shown in help and run/completion messages.</param>
/// <param name="invocations">
/// One or more strings the user can type to invoke the command (case-insensitive).
/// When empty, the runtime defaults to a single invocation equal to <paramref name="name"/> lowercased.
/// </param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class CommandAttribute(string name, params string[] invocations) : Attribute
{
    /// <summary>Display name shown in help and run/completion messages.</summary>
    public string Name { get; } = name;

    /// <summary>
    /// One or more strings the user can type to invoke the command (case-insensitive).
    /// When empty, the runtime defaults to <see cref="Name"/> lowercased.
    /// </summary>
    public string[] Invocations { get; } = invocations;

    /// <summary>Short one-line description shown in the command list.</summary>
    public string HelpBrief { get; set; } = "";

    /// <summary>Optional longer description shown when <c>help &lt;command&gt;</c> is called.</summary>
    public string? HelpDetail { get; set; }
}
