namespace RossWright.MetalCommand.Data.Tests;

public class CommandResultTests
{
    [Fact]
    public void Exit_SetsSuccessTrueAndExitsApplication()
    {
        var result = CommandResult.Exit();

        result.Success.ShouldBeTrue();
        result.ExitApplication.ShouldBeTrue();
        result.Message.ShouldBeNull();
    }

    [Fact]
    public void FailAndExit_SetsSuccessFalseAndExitsApplication()
    {
        var result = CommandResult.FailAndExit();

        result.Success.ShouldBeFalse();
        result.ExitApplication.ShouldBeTrue();
        result.Message.ShouldBeNull();
    }

    [Fact]
    public void FailAndExit_WithMessage_SetsMessageAndExitsApplication()
    {
        var result = CommandResult.FailAndExit("fatal error");

        result.Success.ShouldBeFalse();
        result.ExitApplication.ShouldBeTrue();
        result.Message.ShouldBe("fatal error");
    }
}
