namespace RossWright.MetalCommand;

/// <summary>
/// Returned by <see cref="ICommand.ExecuteAsync"/> to signal the outcome of a command.
/// </summary>
/// <remarks>
/// <para>
/// <b>Exit behaviour</b><br/>
/// <see cref="ExitApplication"/> = <see langword="false"/> (default) — the interactive loop
/// continues normally after the command completes.<br/>
/// <see cref="ExitApplication"/> = <see langword="true"/> — the loop exits cleanly, equivalent
/// to the user typing <c>exit</c>.
/// </para>
/// <para>
/// <see cref="Success"/> is informational: it appears in completion messages and is available
/// to middleware. It does <b>not</b> by itself exit the application — only
/// <see cref="ExitApplication"/> controls that.
/// </para>
/// </remarks>
public readonly record struct CommandResult
{
    /// <summary>
    /// Whether the command completed successfully.
    /// Informational only — does not control whether the application exits.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// When <see langword="true"/>, the interactive loop exits cleanly after this command
    /// completes, equivalent to the user typing <c>exit</c>.
    /// Use <see cref="Exit"/> or <see cref="FailAndExit"/> to set this.
    /// </summary>
    public bool ExitApplication { get; init; }

    /// <summary>
    /// Optional message associated with the result. On <see cref="FailAndExit"/>, this is
    /// written as an error line before the goodbye message.
    /// </summary>
    public string? Message { get; init; }

    // -------------------------------------------------------------------------
    // Convenience factory members
    // -------------------------------------------------------------------------

    /// <summary>Command succeeded; the interactive loop continues.</summary>
    public static CommandResult Ok() => new() { Success = true };

    /// <summary>
    /// Command failed with an optional message; the interactive loop continues.
    /// </summary>
    /// <param name="message">
    /// Optional message describing the failure. When non-null, the caller or middleware
    /// is responsible for writing it to the console.
    /// </param>
    public static CommandResult Fail(string? message = null) =>
        new() { Success = false, Message = message };

    /// <summary>
    /// Command succeeded and requests the application exit cleanly.
    /// The loop exits after the completion message is written, exactly as if the user
    /// typed <c>exit</c>.
    /// </summary>
    public static CommandResult Exit() =>
        new() { Success = true, ExitApplication = true };

    /// <summary>
    /// Command failed and requests the application exit.
    /// <see cref="Message"/> (when non-null) is written as an error line before the
    /// goodbye message.
    /// Useful for non-interactive / CI scenarios where a bad argument should terminate
    /// the process rather than return to the prompt.
    /// </summary>
    /// <param name="message">Optional error message written before the process exits.</param>
    public static CommandResult FailAndExit(string? message = null) =>
        new() { Success = false, ExitApplication = true, Message = message };

    /// <summary>
    /// Implicitly converts a <see langword="bool"/> to a <see cref="CommandResult"/>:
    /// <see langword="true"/> maps to <see cref="Ok"/>, <see langword="false"/> to
    /// <see cref="Fail()"/>.
    /// </summary>
    public static implicit operator CommandResult(bool success) =>
        success ? Ok() : Fail();
}
