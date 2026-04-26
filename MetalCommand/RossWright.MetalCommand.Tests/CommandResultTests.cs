namespace RossWright.MetalCommand.Tests;

public class CommandResultTests
{
    [Fact]
    public void Ok_SetsSuccessTrueAndDoesNotExit()
    {
        var result = CommandResult.Ok();

        result.Success.ShouldBeTrue();
        result.ExitApplication.ShouldBeFalse();
        result.Message.ShouldBeNull();
    }

    [Fact]
    public void Fail_SetsSuccessFalse()
    {
        var result = CommandResult.Fail();

        result.Success.ShouldBeFalse();
        result.ExitApplication.ShouldBeFalse();
        result.Message.ShouldBeNull();
    }

    [Fact]
    public void Fail_WithMessage_SetsMessage()
    {
        var result = CommandResult.Fail("oops");

        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("oops");
    }

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
    }

    [Fact]
    public void FailAndExit_WithMessage_SetsMessage()
    {
        var result = CommandResult.FailAndExit("fatal");

        result.Success.ShouldBeFalse();
        result.ExitApplication.ShouldBeTrue();
        result.Message.ShouldBe("fatal");
    }

    [Fact]
    public void ImplicitBoolTrue_ProducesOk()
    {
        CommandResult result = true;

        result.ShouldBe(CommandResult.Ok());
    }

    [Fact]
    public void ImplicitBoolFalse_ProducesFail()
    {
        CommandResult result = false;

        result.ShouldBe(CommandResult.Fail());
    }
}
