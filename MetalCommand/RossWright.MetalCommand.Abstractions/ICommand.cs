
namespace RossWright.MetalCommand;

/// <summary>
/// Legacy command contract. Implement this interface to use the original
/// descriptor-property / string-array-args style. Supported indefinitely but
/// new commands should implement <see cref="ICommand"/> instead.
/// </summary>
public interface ILegacyCommand
{
    CommandDescriptor Descriptor { get; }
    Task Execute(IConsole console, string[] args, CancellationToken cancellationToken);
}

/// <summary>
/// Attribute-driven command contract. Decorate the class with <see cref="CommandAttribute"/>
/// and properties with <see cref="ArgAttribute"/>. The framework binds argument values to
/// properties before calling <see cref="ExecuteAsync"/>.
/// </summary>
public interface ICommand
{
    Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken);
}

public class CommandDescriptor
{
    public string Name { get; set; } = null!;
    public string[] Invocations { get; set; } = null!;
    public ArgumentDescriptor[]? Args { get; set; } = null!;
    public string HelpBrief { get; set; } = null!;
    public string? HelpDetail { get; set; }
}

public class ArgumentDescriptor
{
    public static ArgumentDescriptor Required(string name, string? helpDetail = null) => new()
    {
        Name = name,
        HelpDetail = helpDetail,
        IsRequired = true
    };
    
    public static ArgumentDescriptor Optional(string name, string? defaultValue, string ? helpDetail = null) => new()
    {
        Name = name,
        HelpDetail = helpDetail,
        DefaultValue = defaultValue,
        IsRequired = false
    };

    public static ArgumentDescriptor RequiredWithContext(string name, string contextKey, string? helpDetail = null) => new()
    {
        Name = name,
        HelpDetail = helpDetail,
        UseContextKeyForDefault = contextKey,
        IsRequired = true
    };

    public static ArgumentDescriptor OptionalWithContext(string name, string contextKey, string? helpDetail = null) => new()
    {
        Name = name,
        HelpDetail = helpDetail,
        UseContextKeyForDefault = contextKey,
        IsRequired = false
    };

    public static ArgumentDescriptor RequiredWithValidValues(string name, string[] validValues, string? helpDetail = null) => new()
    {
        Name = name,
        HelpDetail = helpDetail,
        ValidValues = validValues,
        IsRequired = true
    };
    public static ArgumentDescriptor OptionalWithValidValues(string name, string[] validValues, string defaultValue, string? helpDetail = null) => new()
    {
        Name = name,
        HelpDetail = helpDetail,
        ValidValues = validValues,
        DefaultValue = defaultValue,
        IsRequired = false
    };

    private ArgumentDescriptor() { }

    public string Name { get; private set; } = null!;
    public string? HelpDetail { get; private set; }
    public bool IsRequired { get; private set; }
    public string? DefaultValue { get; private set; }
    public string? UseContextKeyForDefault { get; private set; }
    public string[]? ValidValues { get; set; }
}
