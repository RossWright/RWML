namespace RossWright.MetalCommand;

/// <summary>
/// Invocation customization options for the commands that are built into every
/// <see cref="ConsoleApplication"/>. All five commands are registered automatically;
/// use <see cref="ConsoleApplicationExtensions.CustomizeBuiltInCommands"/> to
/// override their invocation tokens.
/// </summary>
public sealed class BuiltInCommandOptions
{
    /// <summary>
    /// Invocation tokens for the <c>listctx</c> command.
    /// Defaults to <c>["listctx"]</c>.
    /// </summary>
    public string[] ListContextInvocations { get; set; } = ["listctx"];

    /// <summary>
    /// Invocation tokens for the <c>setctx</c> command.
    /// Defaults to <c>["setctx"]</c>.
    /// </summary>
    public string[] SetContextInvocations { get; set; } = ["setctx"];

    /// <summary>
    /// Invocation tokens for the <c>savectx</c> command.
    /// Defaults to <c>["savectx"]</c>.
    /// </summary>
    public string[] SaveContextInvocations { get; set; } = ["savectx"];

    /// <summary>
    /// Invocation tokens for the <c>loadctx</c> command.
    /// Defaults to <c>["loadctx"]</c>.
    /// </summary>
    public string[] LoadContextInvocations { get; set; } = ["loadctx"];

    /// <summary>
    /// Invocation tokens for the <c>dupcon</c> command.
    /// Defaults to <c>["dupcon"]</c>.
    /// </summary>
    public string[] DupConInvocations { get; set; } = ["dupcon"];

    /// <summary>
    /// When <see langword="true"/>, the current session context is forwarded to the
    /// new terminal automatically without the user needing to pass the <c>--context</c> flag.
    /// </summary>
    public bool DupConDefaultForwardContext { get; set; } = true;

    /// <summary>
    /// Controls how the new terminal window is opened on Windows.
    /// </summary>
    public WindowsTerminalLaunchMode DupConWindowsLaunchMode { get; set; } = WindowsTerminalLaunchMode.Auto;

    /// <summary>
    /// Full override of the terminal launcher used by <c>dupcon</c>.
    /// When <see langword="null"/> the built-in <see cref="PlatformTerminalLauncher"/> is used.
    /// </summary>
    public IDupConTerminalLauncher? DupConTerminalLauncher { get; set; }
}
