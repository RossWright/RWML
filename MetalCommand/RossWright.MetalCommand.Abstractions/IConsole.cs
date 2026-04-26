namespace RossWright.MetalCommand;

/// <summary>
/// Abstraction over the terminal for all command I/O. Provides color output, indentation,
/// cursor control, and user input. Use this instead of <see cref="System.Console"/> directly
/// so commands remain testable without a real terminal.
/// </summary>
public interface IConsole
{
    /// <summary>Writes <paramref name="message"/> at the current position without a trailing newline.</summary>
    /// <param name="message">The text to write.</param>
    /// <param name="textColor">Optional foreground color override.</param>
    /// <param name="backgroundColor">Optional background color override.</param>
    void Write(string message, ConsoleColor? textColor = null, ConsoleColor? backgroundColor = null);

    /// <summary>Writes <paramref name="message"/> in error color without a trailing newline.</summary>
    /// <param name="message">The error text to write.</param>
    void WriteError(string message);

    /// <summary>Writes <paramref name="message"/> followed by a newline. Pass <see langword="null"/> to write a blank line.</summary>
    /// <param name="message">The text to write, or <see langword="null"/> for a blank line.</param>
    /// <param name="textColor">Optional foreground color override.</param>
    /// <param name="backgroundColor">Optional background color override.</param>
    void WriteLine(string? message = null, ConsoleColor? textColor = null, ConsoleColor? backgroundColor = null);

    /// <summary>Writes <paramref name="message"/> in error color followed by a newline.</summary>
    /// <param name="message">The error text to write, or <see langword="null"/> for a blank line.</param>
    void WriteErrorLine(string? message = null);

    /// <summary>
    /// Increases the indent level and returns a scope that restores it when disposed.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> that decrements the indent level on disposal.</returns>
    IDisposable Indent();

    /// <summary>If the cursor is not at the start of a line, emits a newline to move it there.</summary>
    void ResetLine();

    /// <summary>Resets the indent level to zero.</summary>
    void ResetIndent();

    /// <summary>Reads a line of input from the user. Returns <see langword="null"/> on end-of-stream.</summary>
    /// <returns>The line entered by the user, or <see langword="null"/>.</returns>
    string? ReadLine();

    /// <summary>
    /// Hides the terminal cursor and returns a scope that restores it when disposed.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> that makes the cursor visible again on disposal.</returns>
    IDisposable HideCursor();
}