namespace RossWright;

/// <summary>US phone number validation and formatting extension methods for <see cref="string"/>.</summary>
public static class PhoneNumberStringExtensions
{
    /// <summary>
    /// Returns <see langword="true"/> if the string represents a valid 10-digit US phone
    /// number. Formatting characters (<c>+</c>, <c>(</c>, <c>)</c>, <c>-</c>) are tolerated;
    /// a leading <c>1</c> country code is accepted and stripped before validation.
    /// </summary>
    /// <param name="text">The string to validate.</param>
    /// <returns><see langword="true"/> if the phone number is valid; otherwise <see langword="false"/>.</returns>
    public static bool IsValidPhoneNumber(this string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        if (text.Any(_ => !char.IsDigit(_) && _ != '+' && _ != '(' && _ != ')' && _ != '-')) return false;
        text = text.ToOnlyDigits();
        if (text.StartsWith('1')) text = text.Substring(1);
        return text.Length == 10;
    }
    /// <summary>
    /// Formats a digit string as <c>(555) 123-4567</c>. When more than 10 digits are
    /// present the leading digits are treated as the country code and prefixed with <c>+</c>.
    /// </summary>
    /// <param name="phonenumber">The phone number string to format.</param>
    /// <returns>A formatted phone number string.</returns>
    public static string ToFormattedPhoneNumber(this string phonenumber)
    {
        if (String.IsNullOrEmpty(phonenumber))
            return "";
        var phoneStringBuilder = new System.Text.StringBuilder();
        var validPhone = phonenumber.ToOnlyDigits();

        var showCountryCode = validPhone.Length > 10;
        if (showCountryCode)
        {
            //phone number is complete, build from 4 parts
            var countryCodeLength = validPhone.Length - 10;
            phoneStringBuilder.Append($"+{validPhone.Substring(0, countryCodeLength)} ");//country code
            phoneStringBuilder.Append($"({validPhone.Substring(countryCodeLength, 3)}) "); //area code
            phoneStringBuilder.Append($"{validPhone.Substring(countryCodeLength + 3, 3)}-"); //phone first 3
            phoneStringBuilder.Append($"{validPhone.Substring(countryCodeLength + 6, 4)}"); //phone last 4
        }
        else
        {
            //assume no country code
            for (int i = 0; i < validPhone.Length; i++)
            {
                if (i == 0)
                    phoneStringBuilder.Append($"({validPhone[i]}"); //area-code start
                else if (i == 2)
                    phoneStringBuilder.Append($"{validPhone[i]}) "); //area-code end
                else if (i == 5)
                    phoneStringBuilder.Append($"{validPhone[i]}-"); //phone first 3 end
                else
                    phoneStringBuilder.Append(validPhone[i]);
            }
        }
        return phoneStringBuilder.ToString();
    }
    /// <summary>
    /// Normalizes a phone number string to E.164 format (<c>+1XXXXXXXXXX</c>).
    /// Strips all non-digit characters before normalizing.
    /// </summary>
    /// <param name="phoneNumber">The phone number string to normalize.</param>
    /// <returns>An E.164-formatted phone number string.</returns>
    public static string ToNormalizedPhoneNumber(this string phoneNumber)
    {
        phoneNumber = phoneNumber.ToOnlyDigits();
        if (phoneNumber.Length == 11 && phoneNumber.StartsWith('1')) return $"+{phoneNumber}";
        if (phoneNumber.Length == 10) return $"+1{phoneNumber}";
        return phoneNumber;
    }
}
