namespace RossWright.MetalCommand.Tests;

public class PlatformTerminalLauncherTests
{
    [Fact]
    public void PlatformTerminalLauncher_ImplementsInterface()
    {
        IDupConTerminalLauncher launcher = new PlatformTerminalLauncher(WindowsTerminalLaunchMode.Auto);
        launcher.ShouldNotBeNull();
    }

    [Fact]
    public void PlatformTerminalLauncher_AllModes_CanBeInstantiated()
    {
        foreach (WindowsTerminalLaunchMode m in Enum.GetValues<WindowsTerminalLaunchMode>())
        {
            var launcher = new PlatformTerminalLauncher(m);
            launcher.ShouldBeAssignableTo<IDupConTerminalLauncher>();
        }
    }

    [Fact]
    public void WindowsTerminalLaunchMode_HasExpectedValues()
    {
        var values = Enum.GetNames<WindowsTerminalLaunchMode>();
        values.ShouldContain("Auto");
        values.ShouldContain("NewWindow");
        values.ShouldContain("WindowsTerminalTab");
        values.ShouldContain("WindowsTerminalWindow");
    }
}
