namespace RossWright;

/// <summary>
/// Extension methods for <see cref="TimeSpan"/>.
/// </summary>
public static class TimeSpanExtensions
{
    /// <summary>
    /// Formats a <see cref="TimeSpan"/> as a human-readable duration string, using the largest meaningful unit.
    /// </summary>
    /// <param name="age">The duration to format.</param>
    /// <returns>
    /// A string such as <c>"3 hours"</c>, <c>"2 weeks"</c>, or <c>"45 seconds"</c>.
    /// </returns>
    public static string ToRelativeTime(this TimeSpan age)
    {
        if (age.TotalDays > 365) return $"{age.TotalDays / 365.0:0.##} years";
        if (age.TotalDays > 14) return $"{age.TotalDays / 7.0:0.##} weeks";
        if (age.TotalHours > 24) return $"{age.TotalDays:0.##} days";
        if (age.TotalHours > 1) return $"{age.TotalHours:0.##} hours";
        if (age.TotalMinutes > 1) return $"{age.TotalMinutes:0.##} minutes";
        if (age.TotalSeconds > 1) return $"{age.TotalSeconds:0.##} seconds";
        return $"{age.TotalMilliseconds:0.##} milliseconds";
    }
}

