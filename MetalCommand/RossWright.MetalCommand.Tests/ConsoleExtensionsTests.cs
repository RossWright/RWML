using RossWright.MetalCommand.Tests.Infrastructure;

namespace RossWright.MetalCommand.Tests;

public class ConsoleExtensionsTests
{
    // -----------------------------------------------------------------------
    // Confirm
    // -----------------------------------------------------------------------

    [Fact]
    public void Confirm_Y_ReturnsTrue()
    {
        var console = new TestConsole("y");
        console.Confirm("Continue?").ShouldBeTrue();
    }

    [Fact]
    public void Confirm_Yes_ReturnsTrue_CaseInsensitive()
    {
        var console = new TestConsole("YES");
        console.Confirm("Continue?").ShouldBeTrue();
    }

    [Fact]
    public void Confirm_N_ReturnsFalse()
    {
        var console = new TestConsole("n");
        console.Confirm("Continue?").ShouldBeFalse();
    }

    [Fact]
    public void Confirm_No_ReturnsFalse_CaseInsensitive()
    {
        var console = new TestConsole("NO");
        console.Confirm("Continue?").ShouldBeFalse();
    }

    [Fact]
    public void Confirm_Enter_DefaultYesTrue_ReturnsTrue()
    {
        var console = new TestConsole("");
        console.Confirm("Continue?", defaultYes: true).ShouldBeTrue();
    }

    [Fact]
    public void Confirm_Enter_DefaultYesFalse_ReturnsFalse()
    {
        var console = new TestConsole("");
        console.Confirm("Continue?", defaultYes: false).ShouldBeFalse();
    }

    [Fact]
    public void Confirm_InvalidThenY_RePromptsAndReturnsTrue()
    {
        var console = new TestConsole("maybe", "y");

        var result = console.Confirm("Continue?");

        result.ShouldBeTrue();
        // The error line "Please enter y or n." should have been written once.
        console.ErrorLines.Count.ShouldBe(1);
        console.ErrorLines[0].ShouldContain("y or n");
        // The prompt should appear twice — once for each attempt.
        console.Lines.Count(_ => _.Contains("Continue?")).ShouldBe(2);
    }

    // -----------------------------------------------------------------------
    // AnnounceAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AnnounceAsync_WritesAnnouncementAndDoneByDefault()
    {
        var console = new TestConsole();

        await console.AnnounceAsync("Processing", () => Task.CompletedTask);

        console.Lines.ShouldContain(l => l.Contains("Processing"));
        console.Lines.ShouldContain("Done!");
    }

    [Fact]
    public async Task AnnounceAsync_WritesCustomConclusion()
    {
        var console = new TestConsole();

        await console.AnnounceAsync("Processing", () => Task.CompletedTask, () => "All set!");

        console.Lines.ShouldContain("All set!");
        console.Lines.ShouldNotContain("Done!");
    }

    [Fact]
    public async Task AnnounceAsync_CallsAction()
    {
        var console = new TestConsole();
        var called = false;

        await console.AnnounceAsync("Processing", () =>
        {
            called = true;
            return Task.CompletedTask;
        });

        called.ShouldBeTrue();
    }

    [Fact]
    public async Task AnnounceAsync_BodyException_Propagates()
    {
        var console = new TestConsole();

        await Should.ThrowAsync<InvalidOperationException>(
            () => console.AnnounceAsync("Processing", () => throw new InvalidOperationException("boom")));
    }

    // -----------------------------------------------------------------------
    // HideCursor
    // -----------------------------------------------------------------------

    [Fact]
    public void HideCursor_ReturnsDisposable_DoesNotThrow()
    {
        var console = new TestConsole();

        using var _ = console.HideCursor();

        // No exception — IDisposable contract satisfied by TestConsole.
    }

    // -----------------------------------------------------------------------
    // ShowProgress
    // -----------------------------------------------------------------------

    [Fact]
    public void ShowProgress_UpdateCallback_CanBeCalledMultipleTimes()
    {
        var console = new TestConsole();

        Should.NotThrow(() =>
            console.ShowProgress(update =>
            {
                update(0.0);
                update(0.5);
                update(1.0);
            }));
    }
}
