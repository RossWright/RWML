namespace RossWright.MetalCommand;

/// <summary>
/// Declares a property on an <see cref="ICommand"/> class as the environment selector argument.
/// The framework binds the resolved environment name to the property before calling
/// <c>ExecuteAsync</c>, then the MetalCommand environment middleware enforces the declared
/// <see cref="EnvironmentPolicy"/> before execution proceeds.
/// </summary>
/// <remarks>
/// Exactly one <see cref="EnvironmentArgAttribute"/> is expected per command. When a command
/// references multiple independent environment sources, set
/// <see cref="EnvironmentSourceType"/> on each property to distinguish them.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class EnvironmentArgAttribute : Attribute
{
    /// <summary>How the framework behaves when this property resolves to a protected environment.</summary>
    public EnvironmentPolicy Policy { get; }

    /// <summary>Help text shown for this argument in help output.</summary>
    public string? HelpDetail { get; set; }

    /// <summary>
    /// The concrete IEnvironmentSource type to resolve from DI for this argument.
    /// When null, the single registered IEnvironmentSource is used.
    /// Required only when a command references multiple different factory types.
    /// </summary>
    public Type? EnvironmentSourceType { get; set; }

    /// <summary>
    /// Initializes a new <see cref="EnvironmentArgAttribute"/>.
    /// </summary>
    /// <param name="policy">The policy enforced when the resolved environment is protected. Defaults to <see cref="EnvironmentPolicy.Dangerous"/>.</param>
    public EnvironmentArgAttribute(EnvironmentPolicy policy = EnvironmentPolicy.Dangerous)
        => Policy = policy;
}
