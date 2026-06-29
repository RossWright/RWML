using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace RossWright;

/// <summary>
/// An <see cref="ILoggerProvider"/> that creates color-coded, scope-indented console loggers.
/// Use <see cref="MetalConsoleLoggerProvider.Default"/> to register with an <see cref="ILoggerFactory"/>.
/// </summary>
public sealed class MetalConsoleLoggerProvider : ILoggerProvider
{
    /// <summary>
    /// A pre-configured provider instance using default colors and minimum level.
    /// </summary>
    public static readonly MetalConsoleLoggerProvider Default = new();

    private readonly AsyncLocal<int> _indent = new();
    private readonly ConcurrentDictionary<string, MetalConsoleLogger> _loggers = new();
    private readonly LogLevel _minLogLevel;
    private readonly ConsoleColor _traceColor;
    private readonly ConsoleColor _warningColor;
    private readonly ConsoleColor _errorColor;

    /// <summary>
    /// Initializes a new <see cref="MetalConsoleLoggerProvider"/>.
    /// </summary>
    /// <param name="minLogLevel">
    /// The minimum level to emit. Defaults to <see cref="LogLevel.Trace"/> in
    /// Debug builds and <see cref="LogLevel.Warning"/> in Release builds.
    /// </param>
    /// <param name="traceColor">Console color for trace/info/debug messages.</param>
    /// <param name="warningColor">Console color for warning messages.</param>
    /// <param name="errorColor">Console color for error and critical messages.</param>
    public MetalConsoleLoggerProvider(
        LogLevel minLogLevel =
#if DEBUG
        LogLevel.Debug,
#else
        LogLevel.Warning,
#endif
        ConsoleColor traceColor = ConsoleColor.DarkBlue,
        ConsoleColor warningColor = ConsoleColor.Yellow,
        ConsoleColor errorColor = ConsoleColor.Red)
    {
        _minLogLevel = minLogLevel;
        _traceColor = traceColor;
        _warningColor = warningColor;
        _errorColor = errorColor;
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName,
            name => new MetalConsoleLogger(name, _indent, _minLogLevel, _traceColor, _warningColor, _errorColor));

    /// <inheritdoc/>
    public void Dispose() => _loggers.Clear();
}

/// <summary>
/// Extension methods for registering <see cref="MetalConsoleLoggerProvider"/> with an <see cref="ILoggingBuilder"/>.
/// </summary>
public static class MetalConsoleLoggerExtensions
{
    /// <summary>
    /// Adds <see cref="MetalConsoleLoggerProvider"/> to the logging pipeline.
    /// </summary>
    public static ILoggingBuilder AddMetalConsoleLogger(
        this ILoggingBuilder builder,
        LogLevel minLogLevel =
#if DEBUG
        LogLevel.Debug,
#else
        LogLevel.Warning,
#endif
        ConsoleColor traceColor = ConsoleColor.DarkBlue,
        ConsoleColor warningColor = ConsoleColor.Yellow,
        ConsoleColor errorColor = ConsoleColor.Red)
    {
        builder.AddProvider(new MetalConsoleLoggerProvider(minLogLevel, traceColor, warningColor, errorColor));
        return builder;
    }
}

/// <summary>
/// An <see cref="ILogger"/> implementation that writes color-coded, scope-indented messages
/// to the console. Created by <see cref="MetalConsoleLoggerProvider"/>.
/// </summary>
public sealed class MetalConsoleLogger(
    string _categoryName,
    AsyncLocal<int> _indent,
    LogLevel _minLogLevel,
    ConsoleColor _traceColor,
    ConsoleColor _warningColor,
    ConsoleColor _errorColor) : ILogger
{
    private static readonly bool _useColor =
        RuntimeInformation.ProcessArchitecture != Architecture.Wasm &&
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) =>
        logLevel != LogLevel.None && logLevel >= _minLogLevel;

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        if (exception != null)
            message += $"\n{exception}";

        var color = logLevel switch
        {
            LogLevel.Warning => _warningColor,
            LogLevel.Error or LogLevel.Critical => _errorColor,
            _ => _traceColor
        };

        if (_useColor) Console.ForegroundColor = color;
        Console.WriteLine(new string('\t', _indent.Value) + $"{_categoryName}: " + message);
        if (_useColor) Console.ResetColor();
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        var message = state.ToString();
        if (!string.IsNullOrWhiteSpace(message))
            Log(LogLevel.Trace, default, message, null, (s, _) => s);
        _indent.Value++;
        return new OnDispose(() => _indent.Value--);
    }
}
