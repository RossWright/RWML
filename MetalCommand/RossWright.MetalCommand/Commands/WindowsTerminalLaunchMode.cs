namespace RossWright.MetalCommand;

/// <summary>
/// Controls how dupcon opens a new terminal window on Windows.
/// </summary>
public enum WindowsTerminalLaunchMode
{
    /// <summary>Use Windows Terminal tab if wt.exe is on PATH, otherwise open a new cmd window.</summary>
    Auto,
    /// <summary>Always open a new cmd.exe window.</summary>
    NewWindow,
    /// <summary>Open a new tab in the last active Windows Terminal window.</summary>
    WindowsTerminalTab,
    /// <summary>Open a new Windows Terminal window.</summary>
    WindowsTerminalWindow,
}
