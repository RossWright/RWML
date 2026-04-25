namespace RossWright;

/// <summary>
/// Extension methods for <see cref="DateTime"/> and <see cref="DayOfWeek"/>.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Formats a <see cref="DateTime"/> as a local short date and short time string (e.g., <c>"1/1/2025 3:00 PM"</c>).
    /// Both the date and time are converted to local time.
    /// </summary>
    /// <param name="dt">The date-time value to format.</param>
    /// <returns>A string combining the local short date and local short time.</returns>
    public static string ToLocalShortDateTimeString(this DateTime dt) =>
        $"{dt.ToLocalTime().ToShortDateString()} {dt.ToLocalTime().ToShortTimeString()}";

    /// <summary>
    /// Formats a <see cref="DateTime"/> as a short date and local short time string (e.g., <c>"1/1/2025 3:00 PM"</c>).
    /// The date part uses the original value; the time part is converted to local time.
    /// </summary>
    /// <param name="dt">The date-time value to format.</param>
    /// <returns>A string combining the short date and the local short time.</returns>
    public static string ToShortDateTimeString(this DateTime dt) =>
        $"{dt.ToShortDateString()} {dt.ToLocalTime().ToShortTimeString()}";

    /// <summary>
    /// Formats a <see cref="DateTime"/> as a human-readable relative time string.
    /// </summary>
    /// <param name="dt">The UTC date-time to format relative to the current time.</param>
    /// <returns>
    /// A contextual string such as <c>"Just now"</c>, <c>"An hour ago"</c>, <c>"Yesterday at 3:00 PM"</c>,
    /// or a full date for older values.
    /// </returns>
    public static string ToRelativeTime(this DateTime dt)
    {
        var localTime = dt.ToLocalTime();
        var age = DateTime.UtcNow - dt;
        if (age.TotalHours < 1)
        {
            if (age.TotalMinutes < 1) return "Just now";
            if (age.TotalMinutes < 2) return "A minute ago";
            return $"{Math.Floor(age.TotalMinutes)} minutes ago";
        }
        if (age.TotalHours < 12)
        {
            if (age.TotalHours < 2) return "An hour ago";
            return $"{Math.Floor(age.TotalHours)} hours ago";
        }
        if (localTime.Date == DateTime.Today) return "Today at " + localTime.ToShortTimeString();
        if (localTime.Date == DateTime.Today.AddDays(-1)) return "Yesterday at " + localTime.ToShortTimeString();
        if (localTime.Date > DateTime.Today.AddDays(-6)) return $"{localTime.DayOfWeek.Abbr()} at {localTime.ToShortTimeString()}";
        if (localTime.Date.Year == DateTime.Today.Year) return $"{localTime:MMM} {localTime.Day} at {localTime.ToShortTimeString()}";
        return $"{localTime:MMM} {localTime.Day}, {localTime.Year} {localTime.ToShortTimeString()}";
    }

    /// <summary>
    /// Returns the 3-letter abbreviation for a <see cref="DayOfWeek"/> value (e.g., <c>"Mon"</c>, <c>"Tue"</c>).
    /// </summary>
    /// <param name="dow">The day of week.</param>
    /// <returns>The first three characters of the day name.</returns>
    public static string Abbr(this DayOfWeek dow) =>
        new string(dow.ToString()!.Take(3).ToArray());
}

