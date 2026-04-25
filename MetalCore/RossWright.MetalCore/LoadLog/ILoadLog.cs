namespace RossWright;

/// <summary>
/// Diagnostic logging contract used during application startup, before the
/// standard <c>ILogger</c> pipeline is available. Supports scoped indentation
/// and three severity levels.
/// </summary>
public interface ILoadLog
{
    /// <summary>
    /// Optional module or component name prepended to each log message.
    /// </summary>
    string? ModuleName { get; set; }

    /// <summary>
    /// Begins an indented scope. Dispose the returned handle to close the scope.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> that exits the scope on disposal.</returns>
    IDisposable BeginScope();

    /// <summary>
    /// Writes a message at the specified severity level.
    /// </summary>
    /// <param name="level">The severity level of the message.</param>
    /// <param name="message">The message text to log.</param>
    void Log(LogLevel level, string message);
}

/// <summary>
/// Null-safe extension helpers for <see cref="ILoadLog"/>.
/// All methods are no-ops when the log is <see langword="null"/>.
/// </summary>
public static class ILoadLogExtensions
{
    /// <summary>Logs a trace-level message, or does nothing if <paramref name="log"/> is <see langword="null"/>.</summary>
    /// <param name="log">The load log instance, or <see langword="null"/>.</param>
    /// <param name="message">The message text to log.</param>
    public static void LogTrace(this ILoadLog? log, string message) => log?.Log(LogLevel.Trace, message);

    /// <summary>Logs a warning-level message, or does nothing if <paramref name="log"/> is <see langword="null"/>.</summary>
    /// <param name="log">The load log instance, or <see langword="null"/>.</param>
    /// <param name="message">The message text to log.</param>
    public static void LogWarning(this ILoadLog? log, string message) => log?.Log(LogLevel.Warning, message);

    /// <summary>Logs an error-level message, or does nothing if <paramref name="log"/> is <see langword="null"/>.</summary>
    /// <param name="log">The load log instance, or <see langword="null"/>.</param>
    /// <param name="message">The message text to log.</param>
    public static void LogError(this ILoadLog? log, string message) => log?.Log(LogLevel.Error, message);
}

/// <summary>Severity levels for <see cref="ILoadLog"/> messages.</summary>
public enum LogLevel
{
    /// <summary>Detailed diagnostic information.</summary>
    Trace,
    /// <summary>Potentially harmful situations that do not prevent startup.</summary>
    Warning,
    /// <summary>Errors that indicate a failure during startup configuration.</summary>
    Error
}