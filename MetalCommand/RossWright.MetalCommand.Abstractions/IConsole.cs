namespace RossWright.MetalCommand;

public interface IConsole
{
    void Write(string message, ConsoleColor? textColor = null, ConsoleColor? backgroundColor = null);
    void WriteError(string message);
    void WriteLine(string? message = null, ConsoleColor? textColor = null, ConsoleColor? backgroundColor = null);
    void WriteErrorLine(string? message = null);
    IDisposable Indent();
    void ResetLine();
    void ResetIndent();
    string?  ReadLine();
    IDisposable HideCursor();
}