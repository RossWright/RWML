using System.Runtime.InteropServices;

namespace RossWright.MetalCommand;

internal class Console : IConsole
{
    public interface IConsole
    {
        void Write(string? text);
        void WriteLine();
        string? ReadLine();
        ConsoleColor BackgroundColor { get; set; }
        ConsoleColor ForegroundColor { get; set; }
        void ResetColor();
        bool CursorVisible { get; set; }
    }
    private class SystemConsole : IConsole
    {
        public void Write(string? text) => System.Console.Write(text);
        public void WriteLine() => System.Console.WriteLine();
        public string? ReadLine() => System.Console.ReadLine();
        public ConsoleColor BackgroundColor 
        { 
            get => System.Console.BackgroundColor;
            set => System.Console.BackgroundColor = value;
        }
        public ConsoleColor ForegroundColor 
        { 
            get => System.Console.ForegroundColor;
            set => System.Console.ForegroundColor = value;
        }
        public void ResetColor() => System.Console.ResetColor();
        public bool CursorVisible
        {
            get => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                ? System.Console.CursorVisible 
                : true;
            set
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    System.Console.CursorVisible = value;
                }
            }
        }
    }

    public Console(IConsole? console = null) =>
        _console = console ?? new SystemConsole();
    private readonly IConsole _console;

    public ConsoleColor ErrorTextColor {get; set;} = ConsoleColor.White;
    public ConsoleColor ErrorBackgroundColor { get; set; } = ConsoleColor.Red;

    private bool _atStartOfLine = true;
    private int _indent = 0;
    public int TabWidth { get; set; } = 5;

    private void WriteIndent() => _console.Write(new string(' ', _indent * TabWidth));

    public void Write(string message, ConsoleColor? textColor = null, ConsoleColor? backgroundColor = null)
    {
        var lines = message.Split(Environment.NewLine);
        for (var i = 0; i< lines.Length; i++)
        {
            if (_atStartOfLine) WriteIndent();
            if (backgroundColor.HasValue) _console.BackgroundColor = backgroundColor.Value;
            if (textColor.HasValue) _console.ForegroundColor = textColor.Value;
            _console.Write(lines[i]);
            _console.ResetColor();
            if (i < lines.Length - 1 && lines.Length > 1) _console.WriteLine();
        }
        _atStartOfLine = lines.Length > 1;
    }

    public void WriteError(string message)
    {
        if (_atStartOfLine) Write($"ERROR: ", ErrorTextColor, ErrorBackgroundColor);
        Write(message, ErrorTextColor, ErrorBackgroundColor);
    }

    public void WriteLine(string? message = null, ConsoleColor? textColor = null, ConsoleColor? backgroundColor = null)
    {
        if (!string.IsNullOrWhiteSpace(message)) Write(message, textColor, backgroundColor);
        _console.WriteLine();
        _atStartOfLine = true;
    }

    public void WriteErrorLine(string? message = null)
    {
        if (_atStartOfLine && string.IsNullOrWhiteSpace(message))
        {
            message = "Unspecified Failure";
        }
        if (!string.IsNullOrWhiteSpace(message)) WriteError(message);
        WriteLine();
    }

    public IDisposable Indent()
    {
        _indent++;
        return new OnDispose(() => _indent = Math.Max(_indent - 1, 0));
    }

    public void ResetIndent() => _indent = 0;

    public void ResetLine()
    {
        if (!_atStartOfLine) WriteLine();
    }

    public string? ReadLine()
    {
        var value = _console.ReadLine();
        if (value != null) _atStartOfLine = true;
        return value;
    }

    public IDisposable HideCursor()
    {
        var oldValue = _console.CursorVisible;
        _console.CursorVisible = false;
        return new OnDispose(() => _console.CursorVisible = oldValue);
    }
}
