namespace RossWright;

public class ToNormalizedPhoneNumberTests
{
    [Fact]
    public void TenDigitNumber_AddsCountryCode()
        => "5551234567".ToNormalizedPhoneNumber().ShouldBe("+15551234567");

    [Fact]
    public void TenDigitNumberWithFormatting_AddsCountryCode()
        => "(555) 123-4567".ToNormalizedPhoneNumber().ShouldBe("+15551234567");

    [Fact]
    public void ElevenDigitNumberStartingWithOne_AddsPlusSign()
        => "15551234567".ToNormalizedPhoneNumber().ShouldBe("+15551234567");

    [Fact]
    public void ElevenDigitNumberStartingWithOneWithFormatting_AddsPlusSign()
        => "+1 (555) 123-4567".ToNormalizedPhoneNumber().ShouldBe("+15551234567");

    [Fact]
    public void ElevenDigitNumberNotStartingWithOne_ReturnsAsIs()
        => "25551234567".ToNormalizedPhoneNumber().ShouldBe("25551234567");

    [Fact]
    public void NineDigitNumber_ReturnsAsIs()
        => "555123456".ToNormalizedPhoneNumber().ShouldBe("555123456");

    [Fact]
    public void TwelveDigitNumber_ReturnsAsIs()
        => "125551234567".ToNormalizedPhoneNumber().ShouldBe("125551234567");

    [Fact]
    public void EmptyString_ReturnsEmptyString()
        => "".ToNormalizedPhoneNumber().ShouldBe("");

    [Fact]
    public void OnlyNonDigits_ReturnsEmptyString()
        => "()- +".ToNormalizedPhoneNumber().ShouldBe("");

    [Fact]
    public void SingleDigit_ReturnsAsIs()
        => "1".ToNormalizedPhoneNumber().ShouldBe("1");

    [Fact]
    public void ThreeDigits_ReturnsAsIs()
        => "555".ToNormalizedPhoneNumber().ShouldBe("555");
}
