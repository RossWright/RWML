namespace RossWright.MetalGuardian;

public class OneTimePasswordOptions
{
    /// <summary>
    /// The number of digits in a one time password, must be between 4 and 24 (default 6)
    /// </summary>
    public int NumberOfDigits { get; set; } = 6;

    /// <summary>
    /// The number of minutes before an issued one time password expires (default 10)
    /// </summary>
    public int ExpirationInMinutes { get; set; } = 10;
}