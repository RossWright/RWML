namespace RossWright.MetalCommand;

/// <summary>
/// Declares a public settable property on an <see cref="ICommand"/> class as a command argument.
/// The framework reads these attributes at startup (no instantiation required) and binds
/// resolved values to the properties before calling <c>ExecuteAsync</c>.
/// </summary>
/// <remarks>
/// Supported property types: <see langword="string"/>, <see langword="int"/>,
/// <see langword="double"/>, <see langword="bool"/>, <see cref="System.Guid"/>,
/// <see cref="System.DateTime"/>, and any <see langword="enum"/> type.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class ArgAttribute : Attribute
{
    /// <summary>
    /// Display name used in help text and error messages.
    /// Defaults to the property name when <see langword="null"/>.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Zero-based positional order. When <c>-1</c> (default), declaration order is used,
    /// which is stable in .NET 6 and later via <see cref="System.Reflection.MemberInfo.MetadataToken"/>.
    /// </summary>
    public int Order { get; set; } = -1;

    /// <summary>
    /// When <see langword="true"/>, execution is aborted if the argument is not supplied
    /// and has no context-key or default fallback.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Literal string default used when the argument is not supplied and no context-key
    /// fallback resolves a value. Converted to the property type by the binder.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Key into the session context dictionary (<see cref="ICommandExecutor.Context"/>).
    /// When the argument is not supplied, the context value is used as a fallback and
    /// echoed to the console before execution.
    /// </summary>
    public string? ContextKey { get; set; }

    /// <summary>
    /// Restricts accepted values to this explicit set (case-insensitive comparison).
    /// Applies to <see langword="string"/> and <see langword="enum"/> properties.
    /// Supplying a value outside the set aborts execution with an error message.
    /// </summary>
    public string[]? ValidValues { get; set; }

    /// <summary>Help detail shown when <c>help &lt;command&gt;</c> is called.</summary>
    public string? HelpDetail { get; set; }

    /// <summary>
    /// When <see langword="true"/>, the argument can be supplied by name in addition to its
    /// positional slot: <c>--PropertyName value</c> (case-insensitive, hyphens normalised).
    /// For <see langword="bool"/> properties, the bare flag <c>--PropertyName</c> (with no
    /// following value token) is interpreted as <see langword="true"/>.
    /// </summary>
    public bool AllowNamed { get; set; }
}
