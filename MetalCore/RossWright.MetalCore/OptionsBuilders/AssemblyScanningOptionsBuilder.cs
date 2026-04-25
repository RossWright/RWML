using System.Reflection;
using System.Runtime.InteropServices;

namespace RossWright;

/// <summary>
/// Extends <see cref="IUsesLoggerOptionsBuilder"/> with the ability to specify
/// which assemblies should be scanned for auto-registration.
/// </summary>
public interface IAssemblyScanningOptionsBuilder : IUsesLoggerOptionsBuilder
{
    /// <summary>
    /// Adds a single assembly to the scan list.
    /// </summary>
    /// <param name="assembly">The assembly to include in the scan.</param>
    void ScanAssembly(Assembly assembly);
}

/// <summary>
/// Implementation of <see cref="IAssemblyScanningOptionsBuilder"/> that collects
/// assemblies and lazily enumerates their concrete (non-abstract, non-interface)
/// types on first access.
/// </summary>
public class AssemblyScanningOptionsBuilder(string moduleName)
    : UsesLoggerOptionsBuilder(moduleName), 
    IAssemblyScanningOptionsBuilder    
{
    /// <summary>The list of assemblies queued for scanning.</summary>
    public List<Assembly> Assemblies { get; set; } = new();

    private Type[]? _discoveredConcreteTypes;

    /// <summary>
    /// All non-abstract, non-interface types found across the scanned assemblies.
    /// The list is built lazily on first access and cached for subsequent reads.
    /// </summary>
    public Type[] DiscoveredConcreteTypes
    {
        get
        {
            if (_discoveredConcreteTypes == null)
            {
                List<Type> types = new();
                foreach (var assembly in Assemblies)
                {
                    try
                    {
                        types.AddRange(assembly.GetTypes()
                            .Where(_ => _.IsConcrete()));
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        types.AddRange(ex.Types
                            .Where(_ => _ != null && _!.IsConcrete())
                            .Cast<Type>());
                    }
                    catch (Exception ex)
                    {
                        LoadLog?.LogWarning($"Could not load types from assembly '{assembly.FullName}': {ex.Message}");
                    }
                }
                _discoveredConcreteTypes = types.ToArray();
            }
            return _discoveredConcreteTypes;
        }
        internal set => _discoveredConcreteTypes = value;
    }

    /// <inheritdoc/>
    public void ScanAssembly(Assembly assembly)
    {
        if (!Assemblies.Contains(assembly))
        {
            _discoveredConcreteTypes = null;
            Assemblies.Add(assembly);
        }
    }
}

/// <summary>
/// Extension methods for <see cref="IAssemblyScanningOptionsBuilder"/> that
/// provide higher-level assembly discovery strategies.
/// </summary>
public static class IAssemblyScanningBuilderExtensions
{
    /// <summary>Adds multiple assemblies to the scan list.</summary>
    /// <param name="builder">The options builder.</param>
    /// <param name="assemblies">The assemblies to include.</param>
    public static void ScanAssemblies(this IAssemblyScanningOptionsBuilder builder, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
            builder.ScanAssembly(assembly);
    }

    /// <summary>
    /// Adds the calling assembly to the scan list. Use this in the project that
    /// defines the types you want auto-registered.
    /// </summary>
    /// <param name="builder">The options builder.</param>
    public static void ScanThisAssembly(this IAssemblyScanningOptionsBuilder builder) =>
        builder.ScanAssembly(Assembly.GetCallingAssembly());

    /// <summary>
    /// Discovers and adds all loaded assemblies (file-system on non-WASM,
    /// reference walk on WASM).
    /// </summary>
    /// <param name="builder">The options builder.</param>
    public static void ScanAllAssemblies(this IAssemblyScanningOptionsBuilder builder) =>
        builder.ScanAssemblies(FindAssemblies(null, builder.LoadLog));

    /// <summary>
    /// Discovers assemblies by walking loaded assembly references and adds them
    /// to the scan list.
    /// </summary>
    /// <param name="builder">The options builder.</param>
    public static void ScanAllAssembliesViaReference(this IAssemblyScanningOptionsBuilder builder) =>
        builder.ScanAssemblies(FindAssembliesViaReference(null, builder.LoadLog));

    /// <summary>
    /// Discovers assemblies by scanning the application base directory on disk
    /// and adds them to the scan list.
    /// </summary>
    /// <param name="builder">The options builder.</param>
    public static void ScanAllAssembliesViaFileSystem(this IAssemblyScanningOptionsBuilder builder) =>
        builder.ScanAssemblies(FindAssembliesViaFileSystem(null, builder.LoadLog));

    /// <summary>
    /// Adds only assemblies whose names begin with one of the specified prefixes.
    /// Useful in multi-project solutions with a shared name prefix.
    /// </summary>
    /// <param name="builder">The options builder.</param>
    /// <param name="startingWith">One or more assembly name prefixes to match.</param>
    public static void ScanAssembliesStartingWith(this IAssemblyScanningOptionsBuilder builder,
        params string[] startingWith) =>
        builder.ScanAssemblies(FindAssemblies(startingWith, builder.LoadLog));

    /// <summary>
    /// Discovers assemblies via reference walk whose names begin with one of the
    /// specified prefixes.
    /// </summary>
    /// <param name="builder">The options builder.</param>
    /// <param name="startingWith">One or more assembly name prefixes to match.</param>
    public static void ScanReferencedAssembliesStartingWith(this IAssemblyScanningOptionsBuilder builder,
        params string[] startingWith) =>
        builder.ScanAssemblies(FindAssembliesViaReference(startingWith, builder.LoadLog));

    /// <summary>
    /// Discovers assemblies via file-system scan whose names begin with one of
    /// the specified prefixes.
    /// </summary>
    /// <param name="builder">The options builder.</param>
    /// <param name="startingWith">One or more assembly name prefixes to match.</param>
    public static void ScanAssembliesInFolderStartingWith(this IAssemblyScanningOptionsBuilder builder,
        params string[] startingWith) =>
        builder.ScanAssemblies(FindAssembliesViaFileSystem(startingWith, builder.LoadLog));

    /// <summary>
    /// Adds the assemblies that contain the specified types to the scan list.
    /// Use this to include types from referenced projects.
    /// </summary>
    /// <param name="builder">The options builder.</param>
    /// <param name="types">
    /// One or more types whose containing assemblies should be scanned.
    /// </param>
    public static void ScanAssemblyContaining(this IAssemblyScanningOptionsBuilder builder,
        params Type[] types)
    {
        foreach (var type in types)
        {
            builder.ScanAssembly(type.Assembly);
        }
    }

    /// <summary>
    /// Adds the assembly that contains <typeparamref name="T"/> to the scan list.
    /// Use this to include types from a referenced project.
    /// </summary>
    /// <typeparam name="T">A type in the assembly to scan.</typeparam>
    /// <param name="config">The options builder.</param>
    public static void ScanAssemblyContaining<T>(
        this IAssemblyScanningOptionsBuilder config) =>
            config.ScanAssembly(typeof(T).Assembly);

    private static Assembly[] FindAssemblies(string[]? prefixes, ILoadLog? log)
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.Wasm)
            return FindAssembliesViaReference(prefixes, log);
        else
            return FindAssembliesViaFileSystem(prefixes, log);
    }

    private static Assembly[] FindAssembliesViaFileSystem(string[]? prefixes, ILoadLog? log)
    {
        log?.LogTrace($"Loading local assemblies");
        using var topLogScope = log?.BeginScope();
        var asmFiles = prefixes?.SelectMany(prefix =>
            Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, $"{prefix}*.dll"))
            ?? Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, $"*.dll");
        log?.LogTrace($"Found {asmFiles.Count()} assemblies:");
        List<Assembly> assemblies = new List<Assembly>();
        foreach (var asmFile in asmFiles.OrderBy(_ => _))
        {
            try
            {
                assemblies.Add(Assembly.Load(AssemblyName.GetAssemblyName(asmFile)));
                log?.LogTrace($"{asmFile} Loaded");
            }
            catch
            {
                log?.LogWarning($"{asmFile} FAILED");
            }
        }
        return assemblies.ToArray();
    }

    private static Assembly[] FindAssembliesViaReference(string[]? prefixes, ILoadLog? log)
    {
        log?.LogTrace($"Loading referenced assemblies");
        using var topLogScope = log?.BeginScope();
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .WhereIf(prefixes?.Any() == true, _ => prefixes!.Any(prefix => _.FullName?.StartsWith(prefix) ?? false))
            .DistinctBy(_ => _.FullName)
            .ToDictionary(_ => _.FullName!, _ => _);
        if (log != null)
        {
            log.LogTrace($"Found {loadedAssemblies.Count()} pre-loaded assemblies:");
            using var logScope = log.BeginScope();
            foreach (var assembly in loadedAssemblies.Values.OrderBy(_ => _.FullName))
                log.LogTrace(assembly.FullName!);
        }

        var referencedAssemblies = loadedAssemblies.Values
            .SelectMany(_ => _.GetReferencedAssemblies())
            .WhereIf(prefixes?.Any() == true, _ => prefixes!.Any(prefix => _.FullName.StartsWith(prefix)))
            .Where(_ => !loadedAssemblies.ContainsKey(_.FullName))
            .DistinctBy(_ => _?.FullName)
            .Select(name =>
            {
                try { return Assembly.Load(name); }
                catch { return null; }
            })
            .Where(_ => _ != null)
            .Cast<Assembly>()
            .ToArray();
        if (log != null)
        {
            log.LogTrace($"Found {referencedAssemblies.Count()} more referenced assemblies that are now loaded:");
            using var logScope = log.BeginScope();
            foreach (var assembly in referencedAssemblies.OrderBy(_ => _.FullName))
                log.LogTrace(assembly.FullName!);
        }
        return loadedAssemblies?.Values.Concat(referencedAssemblies)?.ToArray() ?? Array.Empty<Assembly>();
    }
}