using System.Text.Json;
using RossWright.MetalCommand.Tests.Infrastructure;

namespace RossWright.MetalCommand.Tests;

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
    public void WriteLineIndented_SingleLine_CallsWriteLineOnce()
    {
        var console = new TestConsole();

        console.WriteLineIndented("Single line");

        console.Lines.Count.ShouldBe(1);
        console.Lines[0].ShouldBe("Single line");
    }

    [Fact]
    public void WriteLineIndented_MultipleLines_CallsWriteLineForEachLine()
    {
        var console = new TestConsole();
        var text = $"Line 1{Environment.NewLine}Line 2{Environment.NewLine}Line 3";

        console.WriteLineIndented(text);

        console.Lines.Count.ShouldBe(3);
        console.Lines[0].ShouldBe("Line 1");
        console.Lines[1].ShouldBe("Line 2");
        console.Lines[2].ShouldBe("Line 3");
    }

    [Fact]
    public void WriteLineIndented_EmptyString_CallsWriteLineOnce()
    {
        var console = new TestConsole();

        console.WriteLineIndented("");

        console.Lines.Count.ShouldBe(1);
        console.Lines[0].ShouldBe("");
    }

    [Fact]
    public void WriteLineIndented_WithColors_PassesColorsToWriteLine()
    {
        var console = Substitute.For<IConsole>();

        console.WriteLineIndented("test", ConsoleColor.Red, ConsoleColor.Blue);

        console.Received(1).WriteLine("test", ConsoleColor.Red, ConsoleColor.Blue);
    }

    // -----------------------------------------------------------------------
    // Prompt
    // -----------------------------------------------------------------------

    [Fact]
    public void Prompt_WithDefaultValueAndNoInput_ReturnsDefaultValue()
    {
        var console = new TestConsole("");

        var result = console.Prompt("Enter name", "DefaultName");

        result.ShouldBe("DefaultName");
    }

    [Fact]
    public void Prompt_WithDefaultValueAndInput_ReturnsInput()
    {
        var console = new TestConsole("UserInput");

        var result = console.Prompt("Enter name", "DefaultName");

        result.ShouldBe("UserInput");
    }

    [Fact]
    public void Prompt_WithoutDefaultValueAndInput_ReturnsInput()
    {
        var console = new TestConsole("UserInput");

        var result = console.Prompt("Enter name");

        result.ShouldBe("UserInput");
    }

    [Fact]
    public void Prompt_WithoutDefaultValueAndNoInput_ReturnsNull()
    {
        var console = new TestConsole((string?)null);

        var result = console.Prompt("Enter name");

        result.ShouldBeNull();
    }

    [Fact]
    public void Prompt_WithDefaultValueDisplaysHint_WritesPromptWithHint()
    {
        var console = new TestConsole("test");

        _ = console.Prompt("Enter name", "DefaultName");

        console.Lines.ShouldContain("Enter name [DefaultName]: ");
    }

    [Fact]
    public void Prompt_WithoutDefaultValueDisplaysNoHint_WritesPromptWithoutHint()
    {
        var console = new TestConsole("test");

        _ = console.Prompt("Enter name");

        console.Lines.ShouldContain("Enter name: ");
    }

    [Fact]
    public void Prompt_WithWhitespaceInput_ReturnsDefaultValue()
    {
        var console = new TestConsole("   ");

        var result = console.Prompt("Enter name", "DefaultName");

        result.ShouldBe("DefaultName");
    }

    [Fact]
    public void Prompt_WithInputHavingWhitespace_TrimsInput()
    {
        var console = new TestConsole("  UserInput  ");

        var result = console.Prompt("Enter name");

        result.ShouldBe("UserInput");
    }

    [Fact]
    public void Prompt_WithEmptyInputAndNoDefault_ReturnsNull()
    {
        var console = new TestConsole("");

        var result = console.Prompt("Enter name");

        result.ShouldBeNull();
    }

    // -----------------------------------------------------------------------
    // Choose<T>
    // -----------------------------------------------------------------------

    [Fact]
    public void Choose_EmptyOptionsArray_ThrowsArgumentException()
    {
        var console = new TestConsole();
        var options = Array.Empty<string>();

        Should.Throw<ArgumentException>(() => console.Choose("Select", options))
            .ParamName.ShouldBe("options");
    }

    [Fact]
    public void Choose_ValidChoice_ReturnsSelectedOption()
    {
        var console = new TestConsole("2");
        var options = new[] { "Option1", "Option2", "Option3" };

        var result = console.Choose("Select", options);

        result.ShouldBe("Option2");
    }

    [Fact]
    public void Choose_ValidChoiceFirstOption_ReturnsFirstOption()
    {
        var console = new TestConsole("1");
        var options = new[] { "First", "Second" };

        var result = console.Choose("Select", options);

        result.ShouldBe("First");
    }

    [Fact]
    public void Choose_ValidChoiceLastOption_ReturnsLastOption()
    {
        var console = new TestConsole("3");
        var options = new[] { "First", "Second", "Third" };

        var result = console.Choose("Select", options);

        result.ShouldBe("Third");
    }

    [Fact]
    public void Choose_InvalidChoiceThenValid_ReturnsSelectedOption()
    {
        var console = new TestConsole("invalid", "2");
        var options = new[] { "Option1", "Option2" };

        var result = console.Choose("Select", options);

        result.ShouldBe("Option2");
    }

    [Fact]
    public void Choose_OutOfRangeChoiceThenValid_ReturnsSelectedOption()
    {
        var console = new TestConsole("5", "1");
        var options = new[] { "Option1", "Option2" };

        var result = console.Choose("Select", options);

        result.ShouldBe("Option1");
    }

    [Fact]
    public void Choose_ZeroChoiceThenValid_ReturnsSelectedOption()
    {
        var console = new TestConsole("0", "1");
        var options = new[] { "Option1", "Option2" };

        var result = console.Choose("Select", options);

        result.ShouldBe("Option1");
    }

    [Fact]
    public void Choose_NegativeChoiceThenValid_ReturnsSelectedOption()
    {
        var console = new TestConsole("-1", "1");
        var options = new[] { "Option1", "Option2" };

        var result = console.Choose("Select", options);

        result.ShouldBe("Option1");
    }

    [Fact]
    public void Choose_InvalidChoice_WritesErrorMessage()
    {
        var console = new TestConsole("invalid", "1");
        var options = new[] { "Option1", "Option2" };

        _ = console.Choose("Select", options);

        console.ErrorLines.ShouldContain("Please enter a number between 1 and 2.");
    }

    [Fact]
    public void Choose_DisplaysPromptAndOptions_WritesExpectedOutput()
    {
        var console = new TestConsole("1");
        var options = new[] { "Option1", "Option2" };

        _ = console.Choose("Select an option", options);

        console.Lines.ShouldContain("Select an option");
        console.Lines.ShouldContain("1. Option1");
        console.Lines.ShouldContain("2. Option2");
        console.Lines.ShouldContain("Enter 1-2: ");
    }

    [Fact]
    public void Choose_WithEnumOptions_ReturnsSelectedEnum()
    {
        var console = new TestConsole("2");
        var options = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday };

        var result = console.Choose("Select day", options);

        result.ShouldBe(DayOfWeek.Tuesday);
    }

    [Fact]
    public void Choose_WithIntOptions_ReturnsSelectedInt()
    {
        var console = new TestConsole("1");
        var options = new[] { 10, 20, 30 };

        var result = console.Choose("Select number", options);

        result.ShouldBe(10);
    }

    [Fact]
    public void Choose_NullInputThenValid_ReturnsSelectedOption()
    {
        var console = new TestConsole(null, "1");
        var options = new[] { "Option1", "Option2" };

        var result = console.Choose("Select", options);

        result.ShouldBe("Option1");
    }
}
