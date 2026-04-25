namespace RossWright.MetalInjection;

/// <summary>
/// Specifies that the decorated class should be bound to a configuration section and registered
/// as a singleton in the MetalInjection container. The class is discovered and registered
/// automatically during assembly scanning.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class ConfigSectionAttribute : Attribute
{
    /// <summary>
    /// Specifies that an instance of this class should be injected as a singleton and bound the specific section of the app configuration
    /// </summary>
    /// <param name="sectionTitle">The configuration section to be bound to an instance of this class</param>
    /// <param name="registerAs">The type used for dependency injection if different than the decorated type</param>
    public ConfigSectionAttribute(string sectionTitle, Type? registerAs = null)
    {
        SectionTitle = sectionTitle;
        RegisterAs = registerAs;
    }
    /// <summary>The configuration section key used to bind this class to app configuration.</summary>
    public string SectionTitle { get; private set; }
    /// <summary>
    /// The interface or base type to register the bound instance under in the DI container,
    /// or <see langword="null"/> to register under the concrete type.
    /// </summary>
    public Type? RegisterAs { get; private set; }
}

/// <summary>
/// Shorthand for <c>[ConfigSection(sectionTitle, typeof(<typeparamref name="T"/>))]</c>.
/// Binds the decorated class to a configuration section and registers it as a singleton under <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The interface or base type to register the bound instance under in the DI container.</typeparam>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class ConfigSectionAttribute<T> : ConfigSectionAttribute
{
    /// <param name="sectionTitle">The configuration section to bind to an instance of this class.</param>
    public ConfigSectionAttribute(string sectionTitle) : base(sectionTitle, typeof(T)) { }
}