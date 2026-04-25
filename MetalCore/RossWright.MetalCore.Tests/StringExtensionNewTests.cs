namespace RossWright;

public class SpaceOutTests
{
    [Fact] public void PascalCase_InsertsSpacesBeforeUppercase()
        => "HelloWorld".SpaceOut().ShouldBe("Hello World");

    [Fact] public void CamelCase_InsertsSpaceBeforeUppercase()
        => "helloWorld".SpaceOut().ShouldBe("hello World");

    [Fact] public void SnakeCase_ReplacesUnderscoresWithSpaces()
        => "hello_world".SpaceOut().ShouldBe("hello world");

    [Fact] public void AlreadySpaced_ReturnsUnchanged()
        => "Hello World".SpaceOut().ShouldBe("Hello World");

    [Fact] public void EmptyString_ReturnsEmpty()
        => string.Empty.SpaceOut().ShouldBe(string.Empty);
}

public class CapFirstTests
{
    [Fact] public void LowercaseFirstChar_IsUppercased()
        => "hello world".CapFirst().ShouldBe("Hello world");

    [Fact] public void AlreadyUppercase_ReturnsUnchanged()
        => "Hello".CapFirst().ShouldBe("Hello");

    [Fact] public void AllLowercase_OnlyFirstCharUppercased()
        => "hello".CapFirst().ShouldBe("Hello");

    [Fact] public void NullInput_ReturnsNull()
        => ((string?)null).CapFirst().ShouldBeNull();

    [Fact] public void EmptyString_ReturnsNull()
        => string.Empty.CapFirst().ShouldBeNull();
}

public class TitleCaseTests
{
    [Fact] public void MultiWord_CapitalizesEachWord()
        => "hello world".TitleCase().ShouldBe("Hello World");

    [Fact] public void SingleWord_Capitalizes()
        => "hello".TitleCase().ShouldBe("Hello");

    [Fact] public void AlreadyTitled_ReturnsCorrectly()
        => "Hello World".TitleCase().ShouldBe("Hello World");

    [Fact] public void MixedCase_NormalizesEachWord()
        => "HELLO WORLD".TitleCase().ShouldBe("Hello World");

    [Fact] public void NullInput_ReturnsNull()
        => ((string?)null).TitleCase().ShouldBeNull();

    [Fact] public void WhitespaceOnly_ReturnsNull()
        => "   ".TitleCase().ShouldBeNull();
}

public class ClipTests
{
    [Fact] public void ShorterThanLimit_ReturnsOriginal()
        => "hi".Clip(10).ShouldBe("hi");

    [Fact] public void EqualToLimit_ReturnsOriginal()
        => "hello".Clip(5).ShouldBe("hello");

    [Fact] public void LongerThanLimit_TruncatesToLimit()
        => "hello world".Clip(5).ShouldBe("hello");

    [Fact] public void ZeroLimit_ReturnsEmpty()
        => "hello".Clip(0).ShouldBe(string.Empty);

    [Fact] public void NullInput_ReturnsNull()
        => ((string?)null).Clip(5).ShouldBeNull();
}

public class FilterTests
{
    [Fact] public void FilterCharArray_KeepsOnlyAllowedChars()
        => "a1b2c3".Filter('a', 'b', 'c').ShouldBe("abc");

    [Fact] public void FilterCharArray_EmptyAllowSet_ReturnsEmpty()
        => "abc".Filter().ShouldBe(string.Empty);

    [Fact] public void FilterFunc_PredicateFilter_HappyPath()
        => "a1b2c3".Filter(char.IsDigit).ShouldBe("123");

    [Fact] public void FilterFunc_NullInput_ReturnsNull()
        => ((string?)null).Filter(char.IsDigit).ShouldBeNull();
}

public class WithoutTests
{
    [Fact] public void RemovesSpecifiedChars()
        => "hello".Without('e', 'o').ShouldBe("hll");

    [Fact] public void NoMatchingChars_ReturnsOriginal()
        => "hello".Without('z').ShouldBe("hello");

    [Fact] public void EmptyExclusionSet_ReturnsOriginal()
        => "hello".Without().ShouldBe("hello");
}

public class NullIfEmptyOrWhitespaceTests
{
    [Fact] public void NullInput_ReturnsNull()
        => ((string?)null).NullIfEmptyOrWhitespace().ShouldBeNull();

    [Fact] public void EmptyString_ReturnsNull()
        => string.Empty.NullIfEmptyOrWhitespace().ShouldBeNull();

    [Fact] public void WhitespaceOnly_ReturnsNull()
        => "   ".NullIfEmptyOrWhitespace().ShouldBeNull();

    [Fact] public void NonEmpty_ReturnsOriginal()
        => "hello".NullIfEmptyOrWhitespace().ShouldBe("hello");
}

public class MakeSafeFileNameTests
{
    [Fact] public void SpacesReplacedWithUnderscore()
        => "my file".MakeSafeFileName().ShouldBe("my_file");

    [Fact] public void InvalidCharsReplacedWithUnderscore()
    {
        var result = "file<name>.txt".MakeSafeFileName();
        result.ShouldNotContain('<');
        result.ShouldNotContain('>');
    }

    [Fact] public void AlreadySafeFileName_ReturnsUnchanged()
        => "myfile.txt".MakeSafeFileName().ShouldBe("myfile.txt");
}

public class EndSentenceTests
{
    [Fact] public void AlreadyEndsDot_ReturnsUnchanged()
        => "Hello.".EndSentence().ShouldBe("Hello.");

    [Fact] public void AlreadyEndsQuestion_ReturnsUnchanged()
        => "Hello?".EndSentence().ShouldBe("Hello?");

    [Fact] public void AlreadyEndsExclamation_ReturnsUnchanged()
        => "Hello!".EndSentence().ShouldBe("Hello!");

    [Fact] public void NoTerminator_AppendsDot()
        => "Hello".EndSentence().ShouldBe("Hello.");

    [Fact] public void NullInput_ReturnsNull()
        => ((string?)null).EndSentence().ShouldBeNull();

    [Fact] public void WhitespaceInput_ReturnsUnchanged()
        => "   ".EndSentence().ShouldBe("   ");
}

public class ButAllTests
{
    [Fact] public void ReplacesAllCharsWithGivenChar()
        => "hello".ButAll('*').ShouldBe("*****");

    [Fact] public void EmptyString_ReturnsEmpty()
        => string.Empty.ButAll('*').ShouldBe(string.Empty);

    [Fact] public void WithMaxLength_ClampsLength()
        => "hello".ButAll('*', maxLength: 3).ShouldBe("***");
}

public class ToOnlyDigitsTests
{
    [Fact] public void FormattedPhone_ExtractsDigits()
        => "(555) 123-4567".ToOnlyDigits().ShouldBe("5551234567");

    [Fact] public void MixedAlphaNumeric_KeepsOnlyDigits()
        => "a1b2c3".ToOnlyDigits().ShouldBe("123");

    [Fact] public void AlreadyDigits_ReturnsUnchanged()
        => "12345".ToOnlyDigits().ShouldBe("12345");
}

public class JoinWithQuotesTests
{
    [Fact] public void DefaultDelimiterAndQuote_JoinsCorrectly()
        => new[] { "a", "b", "c" }.JoinWithQuotes().ShouldBe("\"a\",\"b\",\"c\"");

    [Fact] public void CustomDelimiterAndQuote()
        => new[] { "a", "b" }.JoinWithQuotes('|', '\'').ShouldBe("'a'|'b'");

    [Fact] public void EmptySequence_ReturnsEmpty()
        => Array.Empty<string>().JoinWithQuotes().ShouldBe(string.Empty);
}

public class ZeroOneOrManyTests
{
    [Fact] public void ZeroItems_ReturnsZeroString()
        => Array.Empty<int>().ZeroOneOrMany(_ => "many", _ => "one", "none").ShouldBe("none");

    [Fact] public void OneItem_ReturnsOneString()
        => new[] { 42 }.ZeroOneOrMany(_ => "many", x => $"item:{x}", "none").ShouldBe("item:42");

    [Fact] public void ManyItems_ReturnsManyString()
        => new[] { 1, 2, 3 }.ZeroOneOrMany(items => $"{items.Count()} items", x => $"item:{x}", "none").ShouldBe("3 items");

    [Fact] public void NullCollection_ReturnsZeroString()
        => ((IEnumerable<int>?)null).ZeroOneOrMany(_ => "many", _ => "one", "none").ShouldBe("none");
}

public class ToStringIfPresentAndPreSpaceTests
{
    [Fact] public void ToStringIfPresent_NonNull_AppliesFormatter()
        => ((object)"hello").ToStringIfPresent(v => $"VALUE: {v}").ShouldBe("VALUE: hello");

    [Fact] public void ToStringIfPresent_Null_ReturnsEmpty()
        => ((object?)null).ToStringIfPresent(v => $"VALUE: {v}").ShouldBe(string.Empty);

    [Fact] public void PreSpaceIfPresent_NonNull_PrependSpace()
        => ((object)"hello").PreSpaceIfPresent().ShouldBe(" hello");

    [Fact] public void PreSpaceIfPresent_Null_ReturnsNull()
        => ((object?)null).PreSpaceIfPresent().ShouldBeNull();

    [Fact] public void PreSpaceIfPresent_EmptyString_ReturnsEmpty()
        => ((object)string.Empty).PreSpaceIfPresent().ShouldBe(string.Empty);
}

public class GetTupleFromStringArrayTests
{
    private readonly string[] _ten = ["a", "b", "c", "d", "e", "f", "g", "h", "i", "j"];

    [Fact] public void GetTwo_ReturnsFirstTwoElements()
    {
        var (a, b) = _ten.GetTwo();
        a.ShouldBe("a");
        b.ShouldBe("b");
    }

    [Fact] public void GetThree_ReturnsFirstThreeElements()
    {
        var (a, b, c) = _ten.GetThree();
        c.ShouldBe("c");
    }

    [Fact] public void GetFour_ReturnsFirstFourElements()
    {
        var (_, _, _, d) = _ten.GetFour();
        d.ShouldBe("d");
    }

    [Fact] public void GetFive_ReturnsFirstFiveElements()
    {
        var (_, _, _, _, e) = _ten.GetFive();
        e.ShouldBe("e");
    }

    [Fact] public void GetSix_ReturnsFirstSixElements()
    {
        var (_, _, _, _, _, f) = _ten.GetSix();
        f.ShouldBe("f");
    }

    [Fact] public void GetSeven_ReturnsFirstSevenElements()
    {
        var (_, _, _, _, _, _, g) = _ten.GetSeven();
        g.ShouldBe("g");
    }

    [Fact] public void GetEight_ReturnsFirstEightElements()
    {
        var (_, _, _, _, _, _, _, h) = _ten.GetEight();
        h.ShouldBe("h");
    }

    [Fact] public void GetNine_ReturnsFirstNineElements()
    {
        var (_, _, _, _, _, _, _, _, i) = _ten.GetNine();
        i.ShouldBe("i");
    }

    [Fact] public void GetTen_ReturnsAllTenElements()
    {
        var (_, _, _, _, _, _, _, _, _, j) = _ten.GetTen();
        j.ShouldBe("j");
    }

    [Fact] public void GetTwo_UnderLength_ThrowsIndexOutOfRange()
        => Should.Throw<IndexOutOfRangeException>(() => new[] { "only one" }.GetTwo());
}

public class PhoneEmailExtensionTests
{
    // ── IsValidEmail ───────────────────────────────────────────────────────────────
    [Fact] public void IsValidEmail_ValidAddress_ReturnsTrue()
        => "user@example.com".IsValidEmail().ShouldBeTrue();

    [Fact] public void IsValidEmail_NoAtSign_ReturnsFalse()
        => "notanemail".IsValidEmail().ShouldBeFalse();

    [Fact] public void IsValidEmail_NoDomain_ReturnsFalse()
        => "user@".IsValidEmail().ShouldBeFalse();

    [Fact] public void IsValidEmail_Null_ReturnsFalse()
        => ((string?)null).IsValidEmail().ShouldBeFalse();

    [Fact] public void IsValidEmail_UnicodeDomain_ReturnsTrue()
        => "user@münchen.de".IsValidEmail().ShouldBeTrue();

    // ── IsValidPhoneNumber ──────────────────────────────────────────────────────────
    [Fact] public void IsValidPhoneNumber_TenDigitRaw_ReturnsTrue()
        => "5551234567".IsValidPhoneNumber().ShouldBeTrue();

    [Fact] public void IsValidPhoneNumber_FormattedWithDashes_ReturnsTrue()
        => "555-123-4567".IsValidPhoneNumber().ShouldBeTrue();

    [Fact] public void IsValidPhoneNumber_FormattedWithParens_ReturnsTrue()
        => "(555)123-4567".IsValidPhoneNumber().ShouldBeTrue();

    [Fact] public void IsValidPhoneNumber_WithCountryCode1_ReturnsTrue()
        => "15551234567".IsValidPhoneNumber().ShouldBeTrue();

    [Fact] public void IsValidPhoneNumber_TooShort_ReturnsFalse()
        => "12345".IsValidPhoneNumber().ShouldBeFalse();

    [Fact] public void IsValidPhoneNumber_ContainsLetters_ReturnsFalse()
        => "555-abc-4567".IsValidPhoneNumber().ShouldBeFalse();

    [Fact] public void IsValidPhoneNumber_Null_ReturnsFalse()
        => ((string?)null!).IsValidPhoneNumber().ShouldBeFalse();

    // ── ToFormattedPhoneNumber ──────────────────────────────────────────────────────
    [Fact] public void ToFormattedPhoneNumber_TenDigitRaw_FormatsCorrectly()
        => "5551234567".ToFormattedPhoneNumber().ShouldBe("(555) 123-4567");

    [Fact] public void ToFormattedPhoneNumber_ElevenDigitWithCountryCode_FormatsCorrectly()
        => "15551234567".ToFormattedPhoneNumber().ShouldBe("+1 (555) 123-4567");

    [Fact] public void ToFormattedPhoneNumber_Empty_ReturnsEmpty()
        => string.Empty.ToFormattedPhoneNumber().ShouldBe(string.Empty);

    // ── ToNormalizedPhoneNumber ─────────────────────────────────────────────────────
    [Fact] public void ToNormalizedPhoneNumber_TenDigit_PrependsPlus1()
        => "5551234567".ToNormalizedPhoneNumber().ShouldBe("+15551234567");

    [Fact] public void ToNormalizedPhoneNumber_ElevenDigitStartingWith1_PrependsPlus()
        => "15551234567".ToNormalizedPhoneNumber().ShouldBe("+15551234567");

    [Fact] public void ToNormalizedPhoneNumber_FormattedInput_Normalizes()
        => "(555) 123-4567".ToNormalizedPhoneNumber().ShouldBe("+15551234567");

    // ── P3-F: Email edge cases ─────────────────────────────────────────────────
    [Fact] public void IsValidEmail_WithEmptyString_ReturnsFalse()
        => "".IsValidEmail().ShouldBeFalse();

    [Fact] public void IsValidEmail_WithWhitespaceOnly_ReturnsFalse()
        => "   ".IsValidEmail().ShouldBeFalse();

    [Fact] public void IsValidEmail_WithQuotedLocalPart_ReturnsFalse()
        => "\"user\"@example.com".IsValidEmail().ShouldBeFalse();
}

// ── P2-B-i: Split(Func<char, bool>) ──────────────────────────────────────────
public class SplitByPredicateTests
{
    [Fact] public void Split_WithPredicate_SplitsOnMatchingChars()
        => "a,b;c".Split(c => c == ',' || c == ';').ShouldBe(new[] { "a", "b", "c" });

    [Fact] public void Split_WithPredicate_IncludesEmptyEntries()
        => "a,,b".Split(c => c == ',').ShouldBe(new[] { "a", "", "b" });

    [Fact] public void Split_WithRemoveEmptyEntries_ExcludesEmpties()
        => "a,,b".Split(c => c == ',', StringSplitOptions.RemoveEmptyEntries).ShouldBe(new[] { "a", "b" });

    [Fact] public void Split_WithNullSource_ReturnsEmptyArray()
        => ((string?)null).Split(c => c == ',').ShouldBeEmpty();
}

// ── P2-B-ii: WithQueryParameter ───────────────────────────────────────────────
public class WithQueryParameterTests
{
    [Fact] public void WithQueryParameter_NoExistingQuery_AddsFirstParameter()
        => "https://x.com".WithQueryParameter("q", "test").ShouldBe("https://x.com?q=test");

    [Fact] public void WithQueryParameter_ExistingQuery_AppendsWithAmpersand()
        => "https://x.com?a=1".WithQueryParameter("b", "2").ShouldBe("https://x.com?a=1&b=2");

    [Fact] public void WithQueryParameter_NullValue_AppendsEmptyValue()
        => "https://x.com".WithQueryParameter("q", null!).ShouldBe("https://x.com?q=");

    [Fact] public void WithQueryParameter_NullUrl_ThrowsNullReferenceException()
        => Should.Throw<NullReferenceException>(() => ((string?)null)!.WithQueryParameter("q", "v"));
}

// ── P2-B-iii: CalcLevenshteinDistanceTo ───────────────────────────────────────
public class LevenshteinDistanceTests
{
    [Fact] public void CalcLevenshteinDistanceTo_IdenticalStrings_ReturnsOne()
        => "hello".CalcLevenshteinDistanceTo("hello").ShouldBe(1.0);

    [Fact] public void CalcLevenshteinDistanceTo_SingleSubstitution_ReturnsNormalizedScore()
        => "kitten".CalcLevenshteinDistanceTo("sitten").ShouldBe(1.0 - 1.0 / 6.0);

    [Fact] public void CalcLevenshteinDistanceTo_CompletelyDifferent_ReturnsZero()
        => "abc".CalcLevenshteinDistanceTo("xyz").ShouldBe(0.0);

    [Fact] public void CalcLevenshteinDistanceTo_EmptySource_ReturnsZero()
        => "".CalcLevenshteinDistanceTo("abc").ShouldBe(0.0);

    [Fact] public void CalcLevenshteinDistanceTo_NullSource_ReturnsZero()
        => ((string?)null).CalcLevenshteinDistanceTo("abc").ShouldBe(0.0);

    [Fact] public void CalcLevenshteinDistanceTo_NullTarget_ReturnsZero()
        => "abc".CalcLevenshteinDistanceTo(null).ShouldBe(0.0);

    [Fact] public void CalcLevenshteinDistanceTo_BothNull_ReturnsZero()
        => ((string?)null).CalcLevenshteinDistanceTo(null).ShouldBe(0.0);
}

// ── P2-B-iv: ToBase64String / FromBase64String ────────────────────────────────
public class Base64Tests
{
    [Fact] public void ToBase64String_WithPlainText_ReturnsNonEmptyString()
    {
        var result = "Hello".ToBase64String();
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
    }

    [Fact] public void ToBase64String_Roundtrip_RestoresOriginal()
        => "Hello World".ToBase64String()!.FromBase64String().ShouldBe("Hello World");

    [Fact] public void ToBase64String_WithNull_ReturnsNull()
        => ((string?)null).ToBase64String().ShouldBeNull();

    [Fact] public void FromBase64String_WithNull_ReturnsNull()
        => ((string?)null).FromBase64String().ShouldBeNull();

    [Fact] public void FromBase64String_WithValidBase64_DecodesCorrectly()
    {
        var encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("test"));
        encoded.FromBase64String().ShouldBe("test");
    }

    [Fact] public void FromBase64String_WithInvalidBase64_ThrowsFormatException()
        => Should.Throw<FormatException>(() => "not-valid!!!".FromBase64String());
}
