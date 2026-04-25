namespace RossWright;

/// <summary>
/// Extends <see cref="IOptionsBuilder"/> with the ability to attach an
/// <see cref="ILoadLog"/> for diagnostic output during options setup.
/// </summary>
public interface IUsesLoggerOptionsBuilder : IOptionsBuilder
{
    /// <summary>
    /// Attaches a load log that receives diagnostic messages during configuration.
    /// Pass <see langword="null"/> to disable all diagnostic output.
    /// </summary>
    /// <param name="loadLog">
    /// The <see cref="ILoadLog"/> to use, or <see langword="null"/> to suppress output.
    /// </param>
    void UseLogger(ILoadLog? loadLog);
    internal ILoadLog? LoadLog { get; }
}

/// <summary>
/// Extension helpers for <see cref="IUsesLoggerOptionsBuilder"/>.
/// </summary>
public static class IUsesLoggerOptionsBuilderExtensions
{
    /// <summary>Disables all diagnostic load-log output for this options builder.</summary>
    /// <param name="builder">The options builder to silence.</param>
    public static void DoNotUseLogger(this IUsesLoggerOptionsBuilder builder) => 
        builder.UseLogger(null);
}

/// <summary>
/// Base implementation of <see cref="IUsesLoggerOptionsBuilder"/> that stores an
/// optional <see cref="ILoadLog"/> and stamps each message with a module name.
/// In Debug builds a <see cref="ConsoleLoadLog"/> is attached by default.
/// </summary>
public class UsesLoggerOptionsBuilder(string moduleName)
    : OptionsBuilder,
    IUsesLoggerOptionsBuilder
{
    /// <inheritdoc/>
    public void UseLogger(ILoadLog? loadLog) => _loadLog = loadLog;

    /// <inheritdoc/>
    public ILoadLog? LoadLog
    {
        get
        {
            if (_loadLog != null) _loadLog.ModuleName = moduleName;
            return _loadLog;
        }
    }

    private ILoadLog? _loadLog =
#if DEBUG
        new ConsoleLoadLog();
#else
        null;
#endif
}
