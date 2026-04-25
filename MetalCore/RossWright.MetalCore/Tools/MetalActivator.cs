using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Remoting;

namespace System.Reflection;

/// <summary>
/// Drop-in replacement for <see cref="System.Activator"/> that lives in the
/// <c>System.Reflection</c> namespace so existing <c>using</c> directives continue
/// to work. In Release builds all construction methods are marked
/// <see cref="System.Diagnostics.DebuggerStepThroughAttribute"/>, keeping the
/// debugger out of framework-level object creation.
/// </summary>
public static class MetalActivator
{
    private const BindingFlags ConstructorDefault = BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;

    /// <summary>
    /// Creates an instance of the specified <paramref name="type"/> using the given binding flags.
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="bindingAttr">Optional binding flags. Defaults to public instance constructors.</param>
    /// <param name="binder">Optional custom binder.</param>
    /// <param name="args">Arguments for the constructor.</param>
    /// <param name="culture">Culture information for conversion. Pass <see langword="null"/> for the current culture.</param>
    /// <returns>The newly created object, or <see langword="null"/>.</returns>

#if !DEBUG
    [DebuggerHidden]
    [DebuggerStepThrough]
#endif
    public static object? CreateInstance([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicConstructors)] Type type,
    BindingFlags? bindingAttr = null, Binder? binder = null, object?[]? args = null, CultureInfo? culture = null) =>
        InnerCreateInstance(type, bindingAttr, binder, args, culture, null);

    /// <summary>
    /// Creates an instance of the specified <paramref name="type"/>, passing
    /// <paramref name="args"/> to the matching public constructor.
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="args">Arguments matched to a public constructor.</param>
    /// <returns>The newly created object, or <see langword="null"/>.</returns>
#if !DEBUG
    [DebuggerHidden]
    [DebuggerStepThrough]
#endif
    public static object? CreateInstance([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type, params object?[]? args) =>
        InnerCreateInstance(type, ConstructorDefault, null, args, null, null);

    /// <summary>
    /// Creates an instance of the specified <paramref name="type"/> with constructor
    /// arguments and activation attributes.
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="args">Arguments matched to a public constructor.</param>
    /// <param name="activationAttributes">Activation attributes passed to the constructor.</param>
    /// <returns>The newly created object, or <see langword="null"/>.</returns>
#if !DEBUG
    [DebuggerHidden]
    [DebuggerStepThrough]
#endif
    public static object? CreateInstance([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type, object?[]? args, object?[]? activationAttributes) =>
        InnerCreateInstance(type, ConstructorDefault, null, args, null, activationAttributes);

    /// <summary>
    /// Loads an assembly from <paramref name="assemblyFile"/> and creates an instance
    /// of the named type, wrapped in an <see cref="ObjectHandle"/>.
    /// </summary>
    /// <param name="assemblyFile">Path to the assembly file.</param>
    /// <param name="typeName">Fully qualified name of the type to instantiate.</param>
    /// <returns>An <see cref="ObjectHandle"/> wrapping the new instance, or <see langword="null"/>.</returns>
    [RequiresUnreferencedCode("Type and its constructor could be removed")]
    public static ObjectHandle? CreateInstanceFrom(string assemblyFile, string typeName) =>
        CreateInstanceFrom(assemblyFile, typeName, false, ConstructorDefault, null, null, null, null);

    /// <summary>
    /// Loads an assembly from <paramref name="assemblyFile"/> and creates an instance of the named type
    /// with optional activation attributes, wrapped in an <see cref="ObjectHandle"/>.
    /// </summary>
    /// <param name="assemblyFile">Path to the assembly file.</param>
    /// <param name="typeName">Fully qualified name of the type to instantiate.</param>
    /// <param name="activationAttributes">Activation attributes passed to the constructor.</param>
    /// <returns>An <see cref="ObjectHandle"/> wrapping the new instance, or <see langword="null"/>.</returns>
    [RequiresUnreferencedCode("Type and its constructor could be removed")]
    public static ObjectHandle? CreateInstanceFrom(string assemblyFile, string typeName, object?[]? activationAttributes) =>
        CreateInstanceFrom(assemblyFile, typeName, false, ConstructorDefault, null, null, null, activationAttributes);

    /// <summary>
    /// Loads an assembly from <paramref name="assemblyFile"/> and creates an instance of the named type
    /// with full control over binding and activation, wrapped in an <see cref="ObjectHandle"/>.
    /// </summary>
    /// <param name="assemblyFile">Path to the assembly file.</param>
    /// <param name="typeName">Fully qualified name of the type to instantiate.</param>
    /// <param name="ignoreCase">Whether to ignore case when matching <paramref name="typeName"/>.</param>
    /// <param name="bindingAttr">Binding flags controlling constructor lookup.</param>
    /// <param name="binder">Optional custom binder.</param>
    /// <param name="args">Arguments matched to a public constructor.</param>
    /// <param name="culture">Culture used for coercing arguments.</param>
    /// <param name="activationAttributes">Activation attributes passed to the constructor.</param>
    /// <returns>An <see cref="ObjectHandle"/> wrapping the new instance, or <see langword="null"/>.</returns>
    [RequiresUnreferencedCode("Type and its constructor could be removed")]
    public static ObjectHandle? CreateInstanceFrom(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder? binder, object?[]? args, CultureInfo? culture, object?[]? activationAttributes)
    {
        Assembly assembly = Assembly.LoadFrom(assemblyFile);
        Type t = assembly.GetType(typeName, throwOnError: true, ignoreCase)!;

        object? o = InnerCreateInstance(t, bindingAttr, binder, args, culture, activationAttributes);

        return o != null ? new ObjectHandle(o) : null;
    }

    /// <summary>
    /// Creates an instance of <typeparamref name="T"/> using the given binding flags.
    /// </summary>
    /// <typeparam name="T">The type to instantiate.</typeparam>
    /// <param name="bindingAttr">Optional binding flags.</param>
    /// <param name="binder">Optional custom binder.</param>
    /// <param name="args">Arguments for the constructor.</param>
    /// <param name="culture">Culture for conversion, or <see langword="null"/> for the current culture.</param>
    /// <returns>The new instance, or <see langword="null"/>.</returns>
#if !DEBUG
    [DebuggerHidden]
    [DebuggerStepThrough]
#endif
    public static T? CreateInstance<T>(BindingFlags? bindingAttr = null, Binder? binder = null, object?[]? args = null, CultureInfo? culture = null)
        where T : class => (T?)InnerCreateInstance(typeof(T), bindingAttr, binder, args, culture, null);

    /// <summary>
    /// Creates an instance of <typeparamref name="T"/>, passing
    /// <paramref name="args"/> to the matching public constructor.
    /// </summary>
    /// <typeparam name="T">The type to instantiate.</typeparam>
    /// <param name="args">Arguments matched to a public constructor.</param>
    /// <returns>The new instance, or <see langword="null"/>.</returns>
#if !DEBUG
    [DebuggerHidden]
    [DebuggerStepThrough]
#endif
    public static T? CreateInstance<T>(params object?[]? args)
         where T : class =>
        (T?)InnerCreateInstance(typeof(T), ConstructorDefault, null, args, null, null);

    /// <summary>
    /// Creates an instance of <typeparamref name="T"/> with constructor arguments
    /// and activation attributes.
    /// </summary>
    /// <typeparam name="T">The type to instantiate.</typeparam>
    /// <param name="args">Arguments matched to a public constructor.</param>
    /// <param name="activationAttributes">Activation attributes passed to the constructor.</param>
    /// <returns>The new instance, or <see langword="null"/>.</returns>
#if !DEBUG
    [DebuggerHidden]
    [DebuggerStepThrough]
#endif
    public static T? CreateInstance<T>(object?[]? args, object?[]? activationAttributes)
        where T : class =>
        (T?)InnerCreateInstance(typeof(T), ConstructorDefault, null, args, null, activationAttributes);

    private static object? InnerCreateInstance(
        Type type, 
        BindingFlags? bindingAttr = null, 
        Binder? binder = null, 
        object?[]? args = null, 
        CultureInfo? culture = null,
        object?[]? activationAttributes = null)
    {
        if (args == null || args.Length == 0)
        {
            return Activator.CreateInstance(
                type,
                bindingAttr ?? ConstructorDefault,
                binder,
                null,
                culture, 
                activationAttributes);
        }

        object?[]? bestConstructorArguments = null;
        foreach (var constructor in type.GetConstructors())
        {
            var constructorParameters = constructor.GetParameters();
            List<object?> constructorArguments = new();
            for (var i = 0; i < constructorParameters.Length; i++)
            {
                if (args.Length > i &&
                    (constructorParameters[i].ParameterType.IsByRef ||
                    (args[i] is not null && constructorParameters[i].ParameterType.IsAssignableFrom(args[i]?.GetType()))))
                {
                    constructorArguments.Add(args[i]);
                }
                else if (constructorParameters[i].HasDefaultValue)
                {
                    constructorArguments.Add(constructorParameters[i].DefaultValue);
                }
                else
                {
                    break;
                }
            }
            if (constructorArguments.Count >= args.Length &&
                constructorArguments.Count == constructorParameters.Length &&
                (bestConstructorArguments?.Length ?? int.MaxValue) > constructorArguments.Count)
            {
                bestConstructorArguments = constructorArguments.ToArray();
            }
        }
        
        return bestConstructorArguments == null ? null
            : Activator.CreateInstance(
                type,
                bindingAttr ?? ConstructorDefault,
                binder,
                bestConstructorArguments,
                culture,
                activationAttributes);
    }
}
