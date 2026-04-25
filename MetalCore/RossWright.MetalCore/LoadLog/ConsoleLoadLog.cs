using System.Runtime.InteropServices;

namespace RossWright;

/// <summary>
/// An <see cref="ILoadLog"/> implementation that writes color-coded messages to
/// the console. In Debug builds all levels are shown; in Release builds only
/// warnings and errors are shown by default.
/// </summary>
public class ConsoleLoadLog : ILoadLog
{
    /// <summary>
    /// A shared, pre-configured <see cref="ConsoleLoadLog"/> instance ready for
    /// immediate use without additional setup.
    /// </summary>
    public static ILoadLog Default { get; } = new ConsoleLoadLog(LogLevel.Trace);

    /// <summary>
    /// Initializes a new <see cref="ConsoleLoadLog"/>.
    /// </summary>
    /// <param name="minLogLevel">
    /// The minimum level to emit. Defaults to <see cref="LogLevel.Trace"/> in
    /// Debug builds and <see cref="LogLevel.Warning"/> in Release builds.
    /// </param>
    /// <param name="traceColor">Console color for trace messages.</param>
    /// <param name="warningColor">Console color for warning messages.</param>
    /// <param name="errorColor">Console color for error messages.</param>
    public ConsoleLoadLog(
        LogLevel minLogLevel =
#if DEBUG
        LogLevel.Trace,
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
        _useColor = RuntimeInformation.ProcessArchitecture != Architecture.Wasm &&
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
    private readonly LogLevel _minLogLevel;
    private readonly ConsoleColor _traceColor;
    private readonly ConsoleColor _warningColor;
    private readonly ConsoleColor _errorColor;
    private readonly bool _useColor;
    private int indent = 0;

    /// <inheritdoc/>
    public string? ModuleName { get; set; }

    void WriteLine(string message, ConsoleColor color)
    {
        if (_useColor) Console.ForegroundColor = color;
        Console.WriteLine(new string('\t', indent) + 
            (ModuleName != null ? $"{ModuleName}: " : string.Empty) + 
            message);
        if (_useColor) Console.ResetColor();
    }

    /// <inheritdoc/>
    public IDisposable BeginScope()
    {
        indent++;
        return new OnDispose(() => indent--);
    }

    /// <inheritdoc/>
    public void Log(LogLevel level, string message)
    { 
        if (_minLogLevel <= level) 
            WriteLine(message, level switch
            {
                LogLevel.Trace => _traceColor,
                LogLevel.Warning => _warningColor,
                LogLevel.Error => _errorColor,
                _ => _traceColor
            }); 
    }
}