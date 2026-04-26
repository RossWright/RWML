namespace RossWright.MetalCommand.Tests;

public class ProgressBarTests
{
    [Fact]
    public void ZeroProgress_OutputStartsAndEndsWithBrackets()
    {
        var bar = new ProgressBar(showPercent: false);
        var output = bar.Output(0.0);

        output.ShouldStartWith("[");
        output.ShouldEndWith("]");
    }

    [Fact]
    public void FullProgress_OutputContainsFullBlockCharacter()
    {
        var bar = new ProgressBar(showPercent: false);
        var output = bar.Output(1.0);

        output.ShouldContain("\u2588");
    }

    [Fact]
    public void OutputLength_MatchesWidth()
    {
        var bar = new ProgressBar(showPercent: false);
        var output = bar.Output(0.5);

        output.Length.ShouldBe(bar.Width);
    }

    [Fact]
    public void NegativeProgress_ClampedToZero()
    {
        var bar = new ProgressBar(showPercent: false);

        var atZero    = bar.Output(0.0);
        var belowZero = bar.Output(-0.5);

        belowZero.ShouldBe(atZero);
    }

    [Fact]
    public void OverOneProgress_ClampedToFull()
    {
        var bar = new ProgressBar();

        var atFull   = bar.Output(1.0);
        var overFull = bar.Output(1.5);

        // The bar fill characters (█) should be identical — the fill is clamped to 100 %.
        // (The percent label rendered inside the bar is a cosmetic overlay and is separate.)
        var fillAtFull   = atFull.Count(c => c == '\u2588');
        var fillOverFull = overFull.Count(c => c == '\u2588');
        fillOverFull.ShouldBe(fillAtFull);
    }

    [Fact]
    public void ShowPercent_True_OutputContainsPercentSign()
    {
        var bar = new ProgressBar(showPercent: true);
        var output = bar.Output(0.5);

        output.ShouldContain("%");
    }

    [Fact]
    public void ShowPercent_False_OutputDoesNotContainPercentSign()
    {
        var bar = new ProgressBar(showPercent: false);
        var output = bar.Output(0.5);

        output.ShouldNotContain("%");
    }

    [Fact]
    public void CustomLength_WidthMatchesLength()
    {
        const int length = 22;
        var bar = new ProgressBar(length: length);

        bar.Width.ShouldBe(length);
    }
}
