namespace RossWright;

public class IsValidEmailTests
{
    [Fact]
    public void IsValidEmail_NullInput_ReturnsFalse()
        => ((string?)null).IsValidEmail().ShouldBeFalse();

    [Fact]
    public void IsValidEmail_EmptyString_ReturnsFalse()
        => string.Empty.IsValidEmail().ShouldBeFalse();

    [Fact]
    public void IsValidEmail_WhitespaceOnly_ReturnsFalse()
        => "   ".IsValidEmail().ShouldBeFalse();

    [Fact]
    public void IsValidEmail_ValidSimpleEmail_ReturnsTrue()
        => "test@example.com".IsValidEmail().ShouldBeTrue();

    [Fact]
    public void IsValidEmail_ValidEmailWithSubdomain_ReturnsTrue()
        => "user@mail.example.com".IsValidEmail().ShouldBeTrue();

    [Fact]
    public void IsValidEmail_ValidEmailWithHyphen_ReturnsTrue()
        => "test-user@example.com".IsValidEmail().ShouldBeTrue();

    [Fact]
    public void IsValidEmail_ValidEmailWithNumbers_ReturnsTrue()
        => "user123@example456.com".IsValidEmail().ShouldBeTrue();

    [Fact]
    public void IsValidEmail_ValidEmailWithPlus_ReturnsTrue()
        => "user+tag@example.com".IsValidEmail().ShouldBeTrue();

    [Fact]
    public void IsValidEmail_MissingAtSign_ReturnsFalse()
        => "userexample.com".IsValidEmail().ShouldBeFalse();

    [Fact]
    public void IsValidEmail_MissingDomain_ReturnsFalse()
        => "user@".IsValidEmail().ShouldBeFalse();

    [Fact]
    public void IsValidEmail_MissingLocalPart_ReturnsFalse()
        => "@example.com".IsValidEmail().ShouldBeFalse();

    [Fact]
    public void IsValidEmail_MultipleAtSigns_ReturnsFalse()
        => "user@@example.com".IsValidEmail().ShouldBeFalse();

    [Fact]
    public void IsValidEmail_ContainsQuotes_ReturnsFalse()
        => "\"test\"@example.com".IsValidEmail().ShouldBeFalse();

    [Fact]
    public void IsValidEmail_InvalidDomainWithTooManyLabels_ReturnsFalse()
        => "user@.example.com".IsValidEmail().ShouldBeFalse();

    [Fact]
    public void IsValidEmail_InvalidUnicodeDomain_ReturnsFalse()
    {
        // Domain with invalid Unicode that causes IdnMapping.GetAscii to throw ArgumentException
        // Using a domain with invalid characters that IdnMapping cannot process
        var email = "test@\u0001\u0002invalid.com";
        email.IsValidEmail().ShouldBeFalse();
    }

    [Fact]
    public void IsValidEmail_DomainWithInvalidIdnCharacters_ReturnsFalse()
    {
        // Domain starting with hyphen which is invalid for IDN
        var email = "test@-invalid.com";
        email.IsValidEmail().ShouldBeFalse();
    }

    [Fact]
    public void IsValidEmail_ExtremelyLongDomain_ReturnsFalse()
    {
        // Create a domain name that exceeds IDN limits (> 255 characters)
        var longDomain = new string('a', 260) + ".com";
        var email = $"test@{longDomain}";
        email.IsValidEmail().ShouldBeFalse();
    }

    [Fact]
    public void IsValidEmail_DomainWithNullCharacter_ReturnsFalse()
    {
        // Null character in domain should cause ArgumentException from IdnMapping
        var email = "test@exam\0ple.com";
        email.IsValidEmail().ShouldBeFalse();
    }

    [Fact]
    public void IsValidEmail_PathologicalInputForRegexTimeout_ReturnsFalse()
    {
        // Attempt to create input that causes catastrophic backtracking in domain normalization regex
        // Using many @ symbols followed by complex patterns
        var pathological = "a" + new string('@', 100) + new string('a', 1000);
        pathological.IsValidEmail().ShouldBeFalse();
    }

    [Fact]
    public void IsValidEmail_ComplexPathologicalPattern_ReturnsFalse()
    {
        // Another attempt at causing regex timeout with nested repetitions
        var localPart = new string('a', 100);
        var domain = string.Concat(Enumerable.Repeat("a.", 500)) + "com";
        var email = $"{localPart}@{domain}";
        email.IsValidEmail().ShouldBeFalse();
    }

    [Fact]
    public void IsValidEmail_RepeatingPatternForValidationTimeout_ReturnsFalse()
    {
        // Try to trigger timeout in the validation regex with catastrophic backtracking patterns
        // Using alternating characters that could cause backtracking
        var problematic = new string('a', 500) + "@" + new string('b', 500) + ".com";
        problematic.IsValidEmail().ShouldBeFalse();
    }

    [Fact]
    public void IsValidEmail_ValidIpAddress_ReturnsTrue()
        => "user@[192.168.1.1]".IsValidEmail().ShouldBeTrue();

    [Fact]
    public void IsValidEmail_InvalidIpAddressFormat_ReturnsFalse()
        => "user@[192.168.1]".IsValidEmail().ShouldBeFalse();

    [Fact]
    public void IsValidEmail_ValidUnicodeEmail_ReturnsTrue()
        => "user@münchen.de".IsValidEmail().ShouldBeTrue();

    [Fact]
    public void IsValidEmail_DomainEndsWithDot_ReturnsFalse()
        => "user@example.com.".IsValidEmail().ShouldBeFalse();

    [Fact]
    public void IsValidEmail_LocalPartStartsWithDot_ReturnsFalse()
        => ".user@example.com".IsValidEmail().ShouldBeFalse();

    [Fact]
    public void IsValidEmail_LocalPartEndsWithDot_ReturnsFalse()
        => "user.@example.com".IsValidEmail().ShouldBeFalse();

    [Fact]
    public void IsValidEmail_ConsecutiveDotsInLocalPart_ReturnsFalse()
        => "user..name@example.com".IsValidEmail().ShouldBeFalse();

    [Fact]
    public void IsValidEmail_DomainWithControlCharacters_ReturnsFalse()
    {
        // Domain with various control characters to trigger ArgumentException
        var email = "test@exa\u0003mple.com";
        email.IsValidEmail().ShouldBeFalse();
    }

    [Fact]
    public void IsValidEmail_DomainWithBidirectionalOverride_ReturnsFalse()
    {
        // BiDi override characters should cause ArgumentException from IdnMapping
        var email = "test@\u202Eexample.com";
        email.IsValidEmail().ShouldBeFalse();
    }

    [Fact]
    public void IsValidEmail_DomainExceedsMaxLength_ReturnsFalse()
    {
        // Create a single label that exceeds 63 characters (max for a DNS label)
        var longLabel = new string('a', 64);
        var email = $"test@{longLabel}.com";
        email.IsValidEmail().ShouldBeFalse();
    }
}
