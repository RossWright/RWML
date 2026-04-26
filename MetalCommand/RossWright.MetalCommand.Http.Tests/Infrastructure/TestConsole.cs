namespace RossWright.MetalCommand.Http.Tests.Infrastructure;

internal sealed class TestConsole : IConsole
{
    public List<string> Lines { get; } = [];
    public List<string> ErrorLines { get; } = [];

    public void Write(string message, ConsoleColor? textColor = null, ConsoleColor? backgroundColor = null)
        => Lines.Add(message);

    public void WriteLine(string? message = null, ConsoleColor? textColor = null, ConsoleColor? backgroundColor = null)
        => Lines.Add(message ?? string.Empty);

    public void WriteError(string message)
        => ErrorLines.Add(message);

    public void WriteErrorLine(string? message = null)
        => ErrorLines.Add(message ?? string.Empty);

    public string? ReadLine() => null;

    public IDisposable Indent() => NoOpDisposable.Instance;

    public IDisposable HideCursor() => NoOpDisposable.Instance;

    public void ResetLine() { }

    public void ResetIndent() { }

    private sealed class NoOpDisposable : IDisposable
    {
        public static readonly NoOpDisposable Instance = new();
        public void Dispose() { }
    }
}
