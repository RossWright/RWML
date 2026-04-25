namespace RossWright.MetalCore.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void BasicSplitTest()
    {
        string txt = "one two three";
        var parts = txt.SplitAroundQuotes(' ');
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two"),
            _ => _.ShouldBe("three"));
    }

    [Fact]
    public void SingleQuotesSplitTest()
    {
        string txt = "one 'two three' four";
        var parts = txt.SplitAroundQuotes([' '], ['\'']);
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two three"),
            _ => _.ShouldBe("four"));
    }

    [Fact]
    public void DoubleQuotesSplitTest()
    {
        string txt = "one \"two three\" four";
        var parts = txt.SplitAroundQuotes(' ');
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two three"),
            _ => _.ShouldBe("four"));
    }

    [Fact]
    public void IgnoreNonQuoteSplitTest()
    {
        string txt = "one's \"two's three\" four";
        var parts = txt.SplitAroundQuotes(' ');
        Assert.Collection(parts,
            _ => _.ShouldBe("one's"),
            _ => _.ShouldBe("two's three"),
            _ => _.ShouldBe("four"));
    }

    [Fact]
    public void RepeatQuotesSplitTest()
    {
        string txt = "one\t\"two \"\"three\"\"\" four";
        var parts = txt.SplitAroundQuotes(' ', '\t');
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two \"three\""),
            _ => _.ShouldBe("four"));
    }

    [Fact]
    public void RepeatDelimitersSplitTest()
    {
        string txt = "one  two\t three";
        var parts = txt.SplitAroundQuotes(' ', '\t');
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two"),
            _ => _.ShouldBe("three"));
    }


    [Fact]
    public void CommaListJoinTest()
    {
        List<string>? input = null;
        input.CommaListJoin().ShouldBeNull();
        input = new();
        input.CommaListJoin().ShouldBeNull();
        input.Add("First");
        input.CommaListJoin().ShouldBe("First");
        input.Add("Second");
        input.CommaListJoin().ShouldBe("First and Second");
        input.Add("Third");
        input.CommaListJoin().ShouldBe("First, Second and Third");
    }
}
