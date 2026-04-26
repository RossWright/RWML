using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests;

public class IConsoleExtensionsTests
{
    // -----------------------------------------------------------------------
    // Announce (void)
    // -----------------------------------------------------------------------

    [Fact]
    public void Announce_VoidAction_ExecutesAction()
    {
        var console = new TestConsole();
        var actionCalled = false;

        console.Announce("Test", () => actionCalled = true);

        actionCalled.ShouldBeTrue();
    }

    [Fact]
    public void Announce_VoidActionWithConclusion_UsesConclusionMessage()
    {
        var console = new TestConsole();

        console.Announce("Test", () => { }, () => "Custom conclusion");

        console.Lines.ShouldContain(line => line.Contains("Custom conclusion"));
    }

    [Fact]
    public void Announce_VoidActionWithoutConclusion_UsesDefaultMessage()
    {
        var console = new TestConsole();

        console.Announce("Test", () => { });

        console.Lines.ShouldContain(line => line.Contains("Done!"));
    }

    // -----------------------------------------------------------------------
    // Announce<TResult> (Func<TResult>)
    // -----------------------------------------------------------------------

    [Fact]
    public void Announce_FuncReturnsValue_ReturnsValue()
    {
        var console = new TestConsole();

        var result = console.Announce("Test", () => 42);

        result.ShouldBe(42);
    }

    [Fact]
    public void Announce_FuncReturnsValue_ExecutesFunction()
    {
        var console = new TestConsole();
        var functionCalled = false;

        _ = console.Announce("Test", () =>
        {
            functionCalled = true;
            return 42;
        });

        functionCalled.ShouldBeTrue();
    }

    [Fact]
    public void Announce_FuncWithConclusion_UsesConclusionWithResult()
    {
        var console = new TestConsole();

        _ = console.Announce("Test", () => 42, x => $"Result: {x}");

        console.Lines.ShouldContain(line => line.Contains("Result: 42"));
    }

    [Fact]
    public void Announce_FuncWithoutConclusion_UsesDefaultMessage()
    {
        var console = new TestConsole();

        _ = console.Announce("Test", () => 42);

        console.Lines.ShouldContain(line => line.Contains("Done!"));
    }

    [Fact]
    public void Announce_FuncReturnsString_ReturnsCorrectValue()
    {
        var console = new TestConsole();

        var result = console.Announce("Test", () => "Hello");

        result.ShouldBe("Hello");
    }

    [Fact]
    public void Announce_FuncReturnsNull_ReturnsNull()
    {
        var console = new TestConsole();

        var result = console.Announce<string?>("Test", () => null);

        result.ShouldBeNull();
    }

    [Fact]
    public void Announce_FuncReturnsBool_ReturnsCorrectValue()
    {
        var console = new TestConsole();

        var result = console.Announce("Test", () => true);

        result.ShouldBeTrue();
    }

    [Fact]
    public void Announce_FuncReturnsComplexObject_ReturnsCorrectObject()
    {
        var console = new TestConsole();
        var expected = new { Name = "Test", Value = 42 };

        var result = console.Announce("Test", () => expected);

        result.ShouldBe(expected);
    }

    [Fact]
    public void Announce_FuncWithConclusionReturningNull_UsesNullConclusion()
    {
        var console = new TestConsole();

        _ = console.Announce("Test", () => 42, x => null!);

        console.Lines.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Announce_FuncWithConclusionReturningEmptyString_UsesEmptyConclusion()
    {
        var console = new TestConsole();

        _ = console.Announce("Test", () => 42, x => string.Empty);

        console.Lines.Count.ShouldBeGreaterThan(0);
    }

    // -----------------------------------------------------------------------
    // AnnounceAsync<TResult>
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AnnounceAsync_FuncTaskReturnsValue_ReturnsValue()
    {
        var console = new TestConsole();

        var result = await console.AnnounceAsync("Test", async () => await Task.FromResult(42));

        result.ShouldBe(42);
    }

    [Fact]
    public async Task AnnounceAsync_FuncTaskReturnsValue_ExecutesFunction()
    {
        var console = new TestConsole();
        var functionCalled = false;

        _ = await console.AnnounceAsync("Test", async () =>
        {
            functionCalled = true;
            return await Task.FromResult(42);
        });

        functionCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task AnnounceAsync_FuncTaskWithConclusion_UsesConclusionWithResult()
    {
        var console = new TestConsole();

        _ = await console.AnnounceAsync("Test", async () => await Task.FromResult(42), x => $"Result: {x}");

        console.Lines.ShouldContain(line => line.Contains("Result: 42"));
    }

    [Fact]
    public async Task AnnounceAsync_FuncTaskWithoutConclusion_UsesDefaultMessage()
    {
        var console = new TestConsole();

        _ = await console.AnnounceAsync("Test", async () => await Task.FromResult(42));

        console.Lines.ShouldContain(line => line.Contains("Done!"));
    }

    [Fact]
    public async Task AnnounceAsync_FuncTaskReturnsString_ReturnsCorrectValue()
    {
        var console = new TestConsole();

        var result = await console.AnnounceAsync("Test", async () => await Task.FromResult("Hello"));

        result.ShouldBe("Hello");
    }

    [Fact]
    public async Task AnnounceAsync_FuncTaskReturnsNull_ReturnsNull()
    {
        var console = new TestConsole();

        var result = await console.AnnounceAsync<string?>("Test", async () => await Task.FromResult<string?>(null));

        result.ShouldBeNull();
    }

    // -----------------------------------------------------------------------
    // DumpJson (string)
    // -----------------------------------------------------------------------

    [Fact]
    public void DumpJson_StringIsNull_DoesNotCallWriteLine()
    {
        var console = Substitute.For<IConsole>();

        console.DumpJson((string?)null);

        console.DidNotReceive().WriteLine(Arg.Any<string?>(), Arg.Any<ConsoleColor?>(), Arg.Any<ConsoleColor?>());
    }

    [Fact]
    public void DumpJson_StringIsNotNull_CallsWriteLine()
    {
        var console = new TestConsole();
        var json = "{\"key\":\"value\"}";

        console.DumpJson(json);

        console.Lines.ShouldNotBeEmpty();
    }

    [Fact]
    public void DumpJson_StringIsNotNull_FormatsJson()
    {
        var console = new TestConsole();
        var json = "{\"key\":\"value\"}";

        console.DumpJson(json);

        console.Lines.Count.ShouldBeGreaterThan(0);
    }

    // -----------------------------------------------------------------------
    // DumpJson (object)
    // -----------------------------------------------------------------------

    [Fact]
    public void DumpJson_ObjectIsNull_DoesNotCallWriteLine()
    {
        var console = Substitute.For<IConsole>();

        console.DumpJson((object?)null);

        console.DidNotReceive().WriteLine(Arg.Any<string?>(), Arg.Any<ConsoleColor?>(), Arg.Any<ConsoleColor?>());
    }

    [Fact]
    public void DumpJson_ObjectIsNotNull_CallsDumpJsonWithSerializedJson()
    {
        var console = new TestConsole();
        var obj = new { Name = "Test", Value = 123 };

        console.DumpJson(obj);

        console.Lines.ShouldNotBeEmpty();
    }

    [Fact]
    public void DumpJson_ObjectIsString_SerializesCorrectly()
    {
        var console = new TestConsole();
        var obj = new { Text = "test" };

        console.DumpJson(obj);

        console.Lines.ShouldNotBeEmpty();
    }

    [Fact]
    public void DumpJson_ObjectIsEmptyObject_CallsDumpJson()
    {
        var console = new TestConsole();
        var obj = new { };

        console.DumpJson(obj);

        console.Lines.ShouldNotBeEmpty();
    }

    // -----------------------------------------------------------------------
    // WriteLineIndented
    // -----------------------------------------------------------------------

    [Fact]
    public void WriteLineIndented_SingleLine_WritesOneLine()
    {
        var console = new TestConsole();

        console.WriteLineIndented("test");

        console.Lines.Count.ShouldBe(1);
        console.Lines[0].ShouldBe("test");
    }

    [Fact]
    public void WriteLineIndented_MultipleLines_WritesMultipleLines()
    {
        var console = new TestConsole();
        var text = $"line1{Environment.NewLine}line2{Environment.NewLine}line3";

        console.WriteLineIndented(text);

        console.Lines.Count.ShouldBe(3);
        console.Lines[0].ShouldBe("line1");
        console.Lines[1].ShouldBe("line2");
        console.Lines[2].ShouldBe("line3");
    }

    [Fact]
    public void WriteLineIndented_WithColors_PassesColorsToWriteLine()
    {
        var console = Substitute.For<IConsole>();

        console.WriteLineIndented("test", ConsoleColor.Red, ConsoleColor.Blue);

        console.Received(1).WriteLine("test", ConsoleColor.Red, ConsoleColor.Blue);
    }

    [Fact]
    public void WriteLineIndented_MultipleLinesWithColors_PassesColorsToEachWriteLine()
    {
        var console = Substitute.For<IConsole>();
        var text = $"line1{Environment.NewLine}line2";

        console.WriteLineIndented(text, ConsoleColor.Green, ConsoleColor.Yellow);

        console.Received(1).WriteLine("line1", ConsoleColor.Green, ConsoleColor.Yellow);
        console.Received(1).WriteLine("line2", ConsoleColor.Green, ConsoleColor.Yellow);
    }

    // -----------------------------------------------------------------------
    // Confirm
    // -----------------------------------------------------------------------

    [Fact]
    public void Confirm_EmptyInputWithDefaultYesTrue_ReturnsTrue()
    {
        var console = new TestConsole("");

        var result = console.Confirm("Test", defaultYes: true);

        result.ShouldBeTrue();
    }

    [Fact]
    public void Confirm_EmptyInputWithDefaultYesFalse_ReturnsFalse()
    {
        var console = new TestConsole("");

        var result = console.Confirm("Test", defaultYes: false);

        result.ShouldBeFalse();
    }

    [Fact]
    public void Confirm_InputY_ReturnsTrue()
    {
        var console = new TestConsole("y");

        var result = console.Confirm("Test");

        result.ShouldBeTrue();
    }

    [Fact]
    public void Confirm_InputYes_ReturnsTrue()
    {
        var console = new TestConsole("yes");

        var result = console.Confirm("Test");

        result.ShouldBeTrue();
    }

    [Fact]
    public void Confirm_InputYUppercase_ReturnsTrue()
    {
        var console = new TestConsole("Y");

        var result = console.Confirm("Test");

        result.ShouldBeTrue();
    }

    [Fact]
    public void Confirm_InputYesUppercase_ReturnsTrue()
    {
        var console = new TestConsole("YES");

        var result = console.Confirm("Test");

        result.ShouldBeTrue();
    }

    [Fact]
    public void Confirm_InputYesMixedCase_ReturnsTrue()
    {
        var console = new TestConsole("YeS");

        var result = console.Confirm("Test");

        result.ShouldBeTrue();
    }

    [Fact]
    public void Confirm_InputN_ReturnsFalse()
    {
        var console = new TestConsole("n");

        var result = console.Confirm("Test");

        result.ShouldBeFalse();
    }

    [Fact]
    public void Confirm_InputNo_ReturnsFalse()
    {
        var console = new TestConsole("no");

        var result = console.Confirm("Test");

        result.ShouldBeFalse();
    }

    [Fact]
    public void Confirm_InputNUppercase_ReturnsFalse()
    {
        var console = new TestConsole("N");

        var result = console.Confirm("Test");

        result.ShouldBeFalse();
    }

    [Fact]
    public void Confirm_InputNoUppercase_ReturnsFalse()
    {
        var console = new TestConsole("NO");

        var result = console.Confirm("Test");

        result.ShouldBeFalse();
    }

    [Fact]
    public void Confirm_InputNoMixedCase_ReturnsFalse()
    {
        var console = new TestConsole("nO");

        var result = console.Confirm("Test");

        result.ShouldBeFalse();
    }

    [Fact]
    public void Confirm_InvalidInputThenValid_RepromptsAndReturnsTrue()
    {
        var console = new TestConsole("invalid", "y");

        var result = console.Confirm("Test");

        result.ShouldBeTrue();
        console.ErrorLines.Count.ShouldBe(1);
        console.ErrorLines[0].ShouldBe("Please enter y or n.");
    }

    [Fact]
    public void Confirm_MultipleInvalidInputsThenValid_RepromptsAndReturnsFalse()
    {
        var console = new TestConsole("bad", "wrong", "n");

        var result = console.Confirm("Test");

        result.ShouldBeFalse();
        console.ErrorLines.Count.ShouldBe(2);
        console.ErrorLines[0].ShouldBe("Please enter y or n.");
        console.ErrorLines[1].ShouldBe("Please enter y or n.");
    }

    [Fact]
    public void Confirm_DefaultYesTrue_ShowsYUppercaseInHint()
    {
        var console = new TestConsole("y");

        _ = console.Confirm("Test", defaultYes: true);

        console.Lines.ShouldContain(line => line.Contains("[Y/n]"));
    }

    [Fact]
    public void Confirm_DefaultYesFalse_ShowsNUppercaseInHint()
    {
        var console = new TestConsole("n");

        _ = console.Confirm("Test", defaultYes: false);

        console.Lines.ShouldContain(line => line.Contains("[y/N]"));
    }

    [Fact]
    public void Confirm_WithWhitespace_TrimsAndReturnsTrue()
    {
        var console = new TestConsole("  yes  ");

        var result = console.Confirm("Test");

        result.ShouldBeTrue();
    }

    [Fact]
    public void Confirm_NullReadLine_TreatsAsEmpty()
    {
        var console = new TestConsole();

        var result = console.Confirm("Test", defaultYes: true);

        result.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Prompt
    // -----------------------------------------------------------------------

    [Fact]
    public void Prompt_WithInput_ReturnsInput()
    {
        var console = new TestConsole("user input");

        var result = console.Prompt("Test");

        result.ShouldBe("user input");
    }

    [Fact]
    public void Prompt_WithEmptyInput_ReturnsDefaultValue()
    {
        var console = new TestConsole("");

        var result = console.Prompt("Test", "default");

        result.ShouldBe("default");
    }

    [Fact]
    public void Prompt_WithNullInput_ReturnsDefaultValue()
    {
        var console = new TestConsole();

        var result = console.Prompt("Test", "default");

        result.ShouldBe("default");
    }

    [Fact]
    public void Prompt_WithWhitespaceInput_ReturnsDefaultValue()
    {
        var console = new TestConsole("   ");

        var result = console.Prompt("Test", "default");

        result.ShouldBe("default");
    }

    [Fact]
    public void Prompt_WithInputAndDefaultValue_ReturnsInput()
    {
        var console = new TestConsole("user input");

        var result = console.Prompt("Test", "default");

        result.ShouldBe("user input");
    }

    [Fact]
    public void Prompt_WithDefaultValueNull_NoHintShown()
    {
        var console = new TestConsole("input");

        _ = console.Prompt("Test", null);

        console.Lines.ShouldContain(line => line == "Test: ");
    }

    [Fact]
    public void Prompt_WithDefaultValueNotNull_ShowsHint()
    {
        var console = new TestConsole("input");

        _ = console.Prompt("Test", "default");

        console.Lines.ShouldContain(line => line.Contains("[default]"));
    }

    [Fact]
    public void Prompt_WithInputTrimsWhitespace_ReturnsTrimmedInput()
    {
        var console = new TestConsole("  input  ");

        var result = console.Prompt("Test");

        result.ShouldBe("input");
    }

    [Fact]
    public void Prompt_WithNoDefaultAndEmptyInput_ReturnsNull()
    {
        var console = new TestConsole("");

        var result = console.Prompt("Test");

        result.ShouldBeNull();
    }

    // -----------------------------------------------------------------------
    // Choose
    // -----------------------------------------------------------------------

    [Fact]
    public void Choose_EmptyOptions_ThrowsArgumentException()
    {
        var console = new TestConsole();
        var options = Array.Empty<string>();

        var exception = Should.Throw<ArgumentException>(() => console.Choose("Test", options));

        exception.ParamName.ShouldBe("options");
        exception.Message.ShouldContain("At least one option must be provided.");
    }

    [Fact]
    public void Choose_ValidChoice_ReturnsSelectedOption()
    {
        var console = new TestConsole("2");
        var options = new[] { "Option1", "Option2", "Option3" };

        var result = console.Choose("Test", options);

        result.ShouldBe("Option2");
    }

    [Fact]
    public void Choose_FirstOption_ReturnsFirstOption()
    {
        var console = new TestConsole("1");
        var options = new[] { "Option1", "Option2", "Option3" };

        var result = console.Choose("Test", options);

        result.ShouldBe("Option1");
    }

    [Fact]
    public void Choose_LastOption_ReturnsLastOption()
    {
        var console = new TestConsole("3");
        var options = new[] { "Option1", "Option2", "Option3" };

        var result = console.Choose("Test", options);

        result.ShouldBe("Option3");
    }

    [Fact]
    public void Choose_SingleOption_ReturnsThatOption()
    {
        var console = new TestConsole("1");
        var options = new[] { "OnlyOption" };

        var result = console.Choose("Test", options);

        result.ShouldBe("OnlyOption");
    }

    [Fact]
    public void Choose_InvalidInputThenValid_RepromptsAndReturnsOption()
    {
        var console = new TestConsole("invalid", "2");
        var options = new[] { "Option1", "Option2" };

        var result = console.Choose("Test", options);

        result.ShouldBe("Option2");
        console.ErrorLines.Count.ShouldBe(1);
        console.ErrorLines[0].ShouldContain("Please enter a number between 1 and 2.");
    }

    [Fact]
    public void Choose_ZeroInput_RepromptsAndReturnsOption()
    {
        var console = new TestConsole("0", "1");
        var options = new[] { "Option1", "Option2" };

        var result = console.Choose("Test", options);

        result.ShouldBe("Option1");
        console.ErrorLines.Count.ShouldBe(1);
    }

    [Fact]
    public void Choose_NegativeInput_RepromptsAndReturnsOption()
    {
        var console = new TestConsole("-1", "2");
        var options = new[] { "Option1", "Option2" };

        var result = console.Choose("Test", options);

        result.ShouldBe("Option2");
        console.ErrorLines.Count.ShouldBe(1);
    }

    [Fact]
    public void Choose_OutOfRangeInput_RepromptsAndReturnsOption()
    {
        var console = new TestConsole("5", "1");
        var options = new[] { "Option1", "Option2" };

        var result = console.Choose("Test", options);

        result.ShouldBe("Option1");
        console.ErrorLines.Count.ShouldBe(1);
        console.ErrorLines[0].ShouldContain("Please enter a number between 1 and 2.");
    }

    [Fact]
    public void Choose_EmptyInput_RepromptsAndReturnsOption()
    {
        var console = new TestConsole("", "1");
        var options = new[] { "Option1", "Option2" };

        var result = console.Choose("Test", options);

        result.ShouldBe("Option1");
        console.ErrorLines.Count.ShouldBe(1);
    }

    [Fact]
    public void Choose_NullReadLine_RepromptsAndReturnsOption()
    {
        var console = new TestConsole(null, "1");
        var options = new[] { "Option1", "Option2" };

        var result = console.Choose("Test", options);

        result.ShouldBe("Option1");
        console.ErrorLines.Count.ShouldBe(1);
    }

    [Fact]
    public void Choose_WithWhitespace_TrimsAndReturnsOption()
    {
        var console = new TestConsole("  2  ");
        var options = new[] { "Option1", "Option2", "Option3" };

        var result = console.Choose("Test", options);

        result.ShouldBe("Option2");
    }

    [Fact]
    public void Choose_DisplaysPrompt_WritesPromptToConsole()
    {
        var console = new TestConsole("1");
        var options = new[] { "Option1" };

        _ = console.Choose("Choose an option", options);

        console.Lines.ShouldContain("Choose an option");
    }

    [Fact]
    public void Choose_DisplaysOptions_WritesNumberedOptionsToConsole()
    {
        var console = new TestConsole("1");
        var options = new[] { "First", "Second", "Third" };

        _ = console.Choose("Test", options);

        console.Lines.ShouldContain(line => line.Contains("1. First"));
        console.Lines.ShouldContain(line => line.Contains("2. Second"));
        console.Lines.ShouldContain(line => line.Contains("3. Third"));
    }

    [Fact]
    public void Choose_DisplaysRangePrompt_ShowsCorrectRange()
    {
        var console = new TestConsole("1");
        var options = new[] { "Option1", "Option2", "Option3" };

        _ = console.Choose("Test", options);

        console.Lines.ShouldContain(line => line.Contains("Enter 1-3: "));
    }

    [Fact]
    public void Choose_WithIntOptions_ReturnsCorrectInt()
    {
        var console = new TestConsole("2");
        var options = new[] { 10, 20, 30 };

        var result = console.Choose("Test", options);

        result.ShouldBe(20);
    }

    [Fact]
    public void Choose_MultipleInvalidInputsThenValid_RepromptsMultipleTimes()
    {
        var console = new TestConsole("abc", "100", "-5", "2");
        var options = new[] { "Option1", "Option2" };

        var result = console.Choose("Test", options);

        result.ShouldBe("Option2");
        console.ErrorLines.Count.ShouldBe(3);
    }

    [Fact]
    public void Choose_WithBoolOptions_ReturnsCorrectBool()
    {
        var console = new TestConsole("1");
        var options = new[] { true, false };

        var result = console.Choose("Test", options);

        result.ShouldBeTrue();
    }

    [Fact]
    public void Choose_VeryLargeNumberInput_RepromptsAndReturnsOption()
    {
        var console = new TestConsole("999999", "1");
        var options = new[] { "Option1", "Option2" };

        var result = console.Choose("Test", options);

        result.ShouldBe("Option1");
        console.ErrorLines.Count.ShouldBe(1);
    }

    [Fact]
    public void Choose_SingleOptionWithOne_ReturnsThatOption()
    {
        var console = new TestConsole("1");
        var options = new[] { 42 };

        var result = console.Choose("Test", options);

        result.ShouldBe(42);
    }

    [Fact]
    public void Choose_TenOptions_DisplaysAllOptionsCorrectly()
    {
        var console = new TestConsole("10");
        var options = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        var result = console.Choose("Test", options);

        result.ShouldBe(10);
        console.Lines.ShouldContain(line => line.Contains("10. 10"));
    }

    [Fact]
    public void Choose_WhitespaceOnlyInput_RepromptsAndReturnsOption()
    {
        var console = new TestConsole("   ", "2");
        var options = new[] { "Option1", "Option2" };

        var result = console.Choose("Test", options);

        result.ShouldBe("Option2");
        console.ErrorLines.Count.ShouldBe(1);
    }
}
