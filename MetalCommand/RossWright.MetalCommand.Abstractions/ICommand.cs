
namespace RossWright.MetalCommand;

/// <summary>
/// Attribute-driven command contract. Decorate the class with <see cref="CommandAttribute"/>
/// and properties with <see cref="ArgAttribute"/>. The framework binds argument values to
/// properties before calling <see cref="ExecuteAsync"/>.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Executes the command. Called by the framework after argument values have been bound
    /// to the command's <c>[Arg]</c> properties.
    /// </summary>
    /// <param name="console">The console for the current session.</param>
    /// <param name="cancellationToken">Cancellation token that is signalled when the user presses Ctrl-C.</param>
    /// <returns>
    /// A <see cref="CommandResult"/> that signals success or failure and controls whether
    /// the interactive loop continues or exits.
    /// </returns>
    Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken);
}

/// <summary>
/// Describes a command — its display name, invocation tokens, arguments, and help text.
/// Used by <c>ILegacyCommand</c> and built by the framework for attribute-driven <see cref="ICommand"/> commands.
/// </summary>
public class CommandDescriptor
{
    /// <summary>Display name shown in help and run/completion messages.</summary>
    public string Name { get; set; } = null!;
    /// <summary>One or more tokens the user can type to invoke the command (case-insensitive).</summary>
    public string[] Invocations { get; set; } = null!;
    /// <summary>Ordered argument descriptors. <see langword="null"/> when the command has no arguments.</summary>
    public ArgumentDescriptor[]? Args { get; set; } = null!;
    /// <summary>Short one-line description shown in the command list.</summary>
    public string HelpBrief { get; set; } = null!;
    /// <summary>Optional longer description shown when <c>help &lt;command&gt;</c> is called.</summary>
    public string? HelpDetail { get; set; }
    /// <summary>Category group used to cluster commands in help output.</summary>
    public string? Category { get; set; }
}

/// <summary>
/// Describes a single argument on a command. Used by <c>ILegacyCommand</c> implementations
/// and constructed from <see cref="ArgAttribute"/> for attribute-driven commands.
/// </summary>
public class ArgumentDescriptor
{
    // Internal constructor used by CommandDescriptorFactory for attribute-driven commands.
    internal ArgumentDescriptor(
        string name,
        bool isRequired,
        string? defaultValue,
        string? contextKey,
        string[]? validValues,
        string? helpDetail,
        bool allowNamed,
        string propertyName,
        Type propertyType)
    {
        Name = name;
        IsRequired = isRequired;
        DefaultValue = defaultValue;
        UseContextKeyForDefault = contextKey;
        ValidValues = validValues;
        HelpDetail = helpDetail;
        AllowNamed = allowNamed;
        PropertyName = propertyName;
        PropertyType = propertyType;
    }

    /// <summary>Display name used in help text and error messages.</summary>
    public string Name { get; private set; } = null!;
    /// <summary>Detail text shown when <c>help &lt;command&gt;</c> is called.</summary>
    public string? HelpDetail { get; private set; }
    /// <summary>When <see langword="true"/>, execution is aborted if no value is resolved for this argument.</summary>
    public bool IsRequired { get; private set; }
    /// <summary>Literal string default used when the argument is not supplied and no context-key fallback resolves a value.</summary>
    public string? DefaultValue { get; private set; }
    /// <summary>Session context key used as a fallback when the argument is not supplied.</summary>
    public string? UseContextKeyForDefault { get; private set; }
    /// <summary>Restricts accepted values to this explicit set (case-insensitive). <see langword="null"/> means any value is accepted.</summary>
    public string[]? ValidValues { get; set; }
    /// <summary>When <see langword="true"/>, the argument can be supplied by name (<c>--PropertyName value</c>) in addition to its positional slot.</summary>
    public bool AllowNamed { get; private set; }

    // Internal — used by ArgBinder for attribute-driven commands; not part of the public API.
    internal string? PropertyName { get; set; }
    internal Type? PropertyType { get; set; }
}
