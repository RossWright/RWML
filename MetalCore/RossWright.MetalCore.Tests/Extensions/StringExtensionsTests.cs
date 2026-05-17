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
        var parts = txt.SplitAroundQuotes([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
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

    [Fact]
    public void SplitAroundQuotes_WithDefaultParameters_SplitsOnCommas()
    {
        string txt = "one,two,three";
        var parts = txt.SplitAroundQuotes();
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two"),
            _ => _.ShouldBe("three"));
    }

    [Fact]
    public void SplitAroundQuotes_WithCharArrayAndOptions_CallsMainOverload()
    {
        string txt = "one, two , three";
        var parts = txt.SplitAroundQuotes([','], StringSplitOptions.TrimEntries);
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two"),
            _ => _.ShouldBe("three"));
    }

    [Fact]
    public void SplitAroundQuotes_WithRemoveEmptyEntries_RemovesEmpty()
    {
        string txt = "one,,two,,,three";
        var parts = txt.SplitAroundQuotes([','], StringSplitOptions.RemoveEmptyEntries);
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two"),
            _ => _.ShouldBe("three"));
    }

    [Fact]
    public void SplitAroundQuotes_WithTrimEntries_TrimsWhitespace()
    {
        string txt = "one,  two  , three ";
        var parts = txt.SplitAroundQuotes([','], StringSplitOptions.TrimEntries);
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two"),
            _ => _.ShouldBe("three"));
    }

    [Fact]
    public void SplitAroundQuotes_WithBothOptions_RemovesAndTrims()
    {
        string txt = " one , , two ,  , three ";
        var parts = txt.SplitAroundQuotes([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two"),
            _ => _.ShouldBe("three"));
    }

    [Fact]
    public void Split_WithPredicateAndTrimEntries_TrimsWhitespace()
    {
        string txt = " one , two , three ";
        var parts = txt.Split(c => c == ',', StringSplitOptions.TrimEntries);
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two"),
            _ => _.ShouldBe("three"));
    }

    [Fact]
    public void Split_WithPredicateAndTrimEntriesAndRemoveEmpty_TrimsAndRemoves()
    {
        string txt = " one ,, two ,, three ";
        var parts = txt.Split(c => c == ',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two"),
            _ => _.ShouldBe("three"));
    }

    // RFC 4180 compliance tests

    [Fact]
    public void SplitAroundQuotes_ConsecutiveDelimiters_PreservesEmptyTokenWithNone()
    {
        // RFC 4180: empty fields between consecutive delimiters must be preserved
        var parts = "field1,,field3".SplitAroundQuotes(',', StringSplitOptions.None);
        Assert.Collection(parts,
            _ => _.ShouldBe("field1"),
            _ => _.ShouldBe(""),
            _ => _.ShouldBe("field3"));
    }

    [Fact]
    public void SplitAroundQuotes_EmptyQuotedField_StripsQuotesAndReturnsEmpty()
    {
        // RFC 4180 §2 rule 7: surrounding double-quotes are not part of the field value
        var parts = "field1,\"\",field3".SplitAroundQuotes(',', StringSplitOptions.None);
        Assert.Collection(parts,
            _ => _.ShouldBe("field1"),
            _ => _.ShouldBe(""),
            _ => _.ShouldBe("field3"));
    }

    [Fact]
    public void SplitAroundQuotes_EmptyInput_ReturnsSingleEmptyToken()
    {
        // Matches BCL string.Split(',') behaviour: empty string → one empty field
        var parts = string.Empty.SplitAroundQuotes(',', StringSplitOptions.None);
        Assert.Collection(parts,
            _ => _.ShouldBe(""));
    }

    // Null / empty input

    [Fact]
    public void SplitAroundQuotes_NullInput_ReturnsEmpty()
    {
        string? txt = null;
        var parts = txt.SplitAroundQuotes(',', StringSplitOptions.None);
        parts.ShouldBeEmpty();
    }

    [Fact]
    public void SplitAroundQuotes_EmptyInputWithRemoveEmptyEntries_ReturnsEmpty()
    {
        var parts = string.Empty.SplitAroundQuotes(',', StringSplitOptions.RemoveEmptyEntries);
        parts.ShouldBeEmpty();
    }

    // Boundary tokens — leading/trailing delimiters

    [Fact]
    public void SplitAroundQuotes_LeadingDelimiter_PreservesEmptyFirstToken()
    {
        var parts = ",one,two".SplitAroundQuotes(',', StringSplitOptions.None);
        Assert.Collection(parts,
            _ => _.ShouldBe(""),
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two"));
    }

    [Fact]
    public void SplitAroundQuotes_TrailingDelimiter_PreservesEmptyLastToken()
    {
        var parts = "one,two,".SplitAroundQuotes(',', StringSplitOptions.None);
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two"),
            _ => _.ShouldBe(""));
    }

    [Fact]
    public void SplitAroundQuotes_SingleToken_ReturnsSingleElement()
    {
        var parts = "hello".SplitAroundQuotes(',', StringSplitOptions.None);
        Assert.Collection(parts,
            _ => _.ShouldBe("hello"));
    }

    // Quoted field containing the delimiter character

    [Fact]
    public void SplitAroundQuotes_QuotedFieldContainsDelimiter_TreatsAsOneToken()
    {
        // The primary reason for this method's existence — delimiter inside quotes must not split
        var parts = "one,\"two,three\",four".SplitAroundQuotes(',', StringSplitOptions.None);
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two,three"),
            _ => _.ShouldBe("four"));
    }

    [Fact]
    public void SplitAroundQuotes_QuotedFieldAtStart_StripsQuotes()
    {
        var parts = "\"one,two\",three".SplitAroundQuotes(',', StringSplitOptions.None);
        Assert.Collection(parts,
            _ => _.ShouldBe("one,two"),
            _ => _.ShouldBe("three"));
    }

    [Fact]
    public void SplitAroundQuotes_QuotedFieldAtEnd_StripsQuotes()
    {
        var parts = "one,\"two,three\"".SplitAroundQuotes(',', StringSplitOptions.None);
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two,three"));
    }

    // TrimEntries interaction with whitespace-only fields

    [Fact]
    public void SplitAroundQuotes_WhitespaceField_TrimEntriesReducesToEmpty()
    {
        var parts = "one,   ,two".SplitAroundQuotes([','], StringSplitOptions.TrimEntries);
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe(""),
            _ => _.ShouldBe("two"));
    }

    [Fact]
    public void SplitAroundQuotes_WhitespaceField_TrimAndRemoveDropsIt()
    {
        // TrimEntries must run before RemoveEmptyEntries so whitespace-only fields are dropped
        var parts = "one,   ,two".SplitAroundQuotes([','], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        Assert.Collection(parts,
            _ => _.ShouldBe("one"),
            _ => _.ShouldBe("two"));
    }

    // Empty splitChars — no splitting should occur

    [Fact]
    public void SplitAroundQuotes_EmptySplitChars_ReturnsSingleToken()
    {
        var parts = "hello,world".SplitAroundQuotes([], StringSplitOptions.None);
        Assert.Collection(parts,
            _ => _.ShouldBe("hello,world"));
    }

    [Fact]
    public void SplitAroundQuotes_EmptySplitCharsAndEmptyInput_ReturnsSingleEmptyToken()
    {
        var parts = string.Empty.SplitAroundQuotes([], StringSplitOptions.None);
        Assert.Collection(parts,
            _ => _.ShouldBe(""));
    }
}
