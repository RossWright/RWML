namespace RossWright.MetalCommand;

/// <summary>
/// Configuration options for the <c>dupcon</c> command.
/// </summary>
public class DupConCommandOptions : CommandOptions
{
    /// <summary>
    /// When true, the current session context is forwarded to the new terminal automatically,
    /// without the user needing to pass the <c>--context</c> flag.
    /// </summary>
    public bool DefaultForwardContext { get; set; } = false;

    /// <summary>
    /// Controls how the new terminal window is opened on Windows.
    /// </summary>
    public WindowsTerminalLaunchMode WindowsLaunchMode { get; set; } = WindowsTerminalLaunchMode.Auto;

    /// <summary>
    /// Full override of the terminal launcher. When null the built-in
    /// <see cref="PlatformTerminalLauncher"/> is used.
    /// </summary>
    public IDupConTerminalLauncher? TerminalLauncher { get; set; }
}
