using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RossWright.MetalCommand;

/// <summary>
/// Abstraction over OS-specific terminal window launching for dupcon.
/// </summary>
public interface IDupConTerminalLauncher
{
    /// <summary>
    /// Launches the given executable in a new terminal window with the supplied arguments.
    /// Returns true if the launch was attempted; false if no suitable terminal was found.
    /// </summary>
    bool Launch(string executablePath, string args);
}

internal sealed class PlatformTerminalLauncher(WindowsTerminalLaunchMode mode = WindowsTerminalLaunchMode.Auto)
    : IDupConTerminalLauncher
{
    public bool Launch(string executablePath, string args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return LaunchWindows(executablePath, args);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return LaunchMacOs(executablePath, args);

        return LaunchLinux(executablePath, args);
    }

    private bool LaunchWindows(string executablePath, string args)
    {
        var effectiveMode = mode;
        if (effectiveMode == WindowsTerminalLaunchMode.Auto)
            effectiveMode = IsOnPath("wt.exe") ? WindowsTerminalLaunchMode.WindowsTerminalTab : WindowsTerminalLaunchMode.NewWindow;

        ProcessStartInfo psi = effectiveMode switch
        {
            WindowsTerminalLaunchMode.WindowsTerminalTab =>
                new ProcessStartInfo("wt.exe", $"--window last nt \"{executablePath}\" {args}")
                { UseShellExecute = false },
            WindowsTerminalLaunchMode.WindowsTerminalWindow =>
                new ProcessStartInfo("wt.exe", $"--window new \"{executablePath}\" {args}")
                { UseShellExecute = false },
            _ => // NewWindow
                new ProcessStartInfo("cmd.exe", $"/c start \"\" \"{executablePath}\" {args}")
                { UseShellExecute = false },
        };

        Process.Start(psi);
        return true;
    }

    private static bool LaunchMacOs(string executablePath, string args)
    {
        Process.Start(new ProcessStartInfo("open", $"-a Terminal \"{executablePath}\" {args}")
        { UseShellExecute = false });
        return true;
    }

    private static bool LaunchLinux(string executablePath, string args)
    {
        string[] candidates =
        [
            "x-terminal-emulator",
            "gnome-terminal",
            "xterm",
        ];

        foreach (var terminal in candidates)
        {
            if (!IsOnPath(terminal))
                continue;

            var arguments = terminal == "gnome-terminal"
                ? $"-- \"{executablePath}\" {args}"
                : $"-e \"{executablePath}\" {args}";

            // x-terminal-emulator uses the same -e convention as xterm
            Process.Start(new ProcessStartInfo(terminal, arguments) { UseShellExecute = false });
            return true;
        }

        return false;
    }

    private static bool IsOnPath(string executable)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var separator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';
        return path.Split(separator).Any(dir =>
            File.Exists(Path.Combine(dir, executable)));
    }
}
