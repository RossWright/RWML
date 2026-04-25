namespace RossWright; 

/// <summary>
/// Convenience methods for parsing strings to nullable value types.
/// All methods accept <see langword="null"/> or whitespace and return
/// <see langword="null"/> on any parse failure instead of throwing.
/// </summary>
public static class ParseOrNull
{
    /// <summary>Parses a string to <see cref="bool"/>, or returns <see langword="null"/> on failure.</summary>
    /// <param name="text">The string to parse.</param>
    /// <returns>The parsed value, or <see langword="null"/>.</returns>
    public static bool? Bool(string? text) => !string.IsNullOrWhiteSpace(text) &&
        bool.TryParse(text, out var value) ? value : null;

    /// <summary>Parses a string to <see cref="DateTime"/>, or returns <see langword="null"/> on failure.</summary>
    /// <param name="text">The string to parse.</param>
    /// <returns>The parsed value, or <see langword="null"/>.</returns>
    public static DateTime? DateTime(string? text) => !string.IsNullOrWhiteSpace(text) && 
        System.DateTime.TryParse(text, out var value) ? value : null;

    /// <summary>Parses a string to <see cref="DateOnly"/>, or returns <see langword="null"/> on failure.</summary>
    /// <param name="text">The string to parse.</param>
    /// <returns>The parsed value, or <see langword="null"/>.</returns>
    public static DateOnly? DateOnly(string? text) => !string.IsNullOrWhiteSpace(text) &&
        System.DateOnly.TryParse(text, out var value) ? value : null;

    /// <summary>Parses a string to <see cref="int"/>, or returns <see langword="null"/> on failure.</summary>
    /// <param name="text">The string to parse.</param>
    /// <returns>The parsed value, or <see langword="null"/>.</returns>
    public static int? Int(string? text) => !string.IsNullOrWhiteSpace(text) && 
        int.TryParse(text, out var value) ? value : null;

    /// <summary>Parses a string to <see cref="Guid"/>, or returns <see langword="null"/> on failure.</summary>
    /// <param name="text">The string to parse.</param>
    /// <returns>The parsed value, or <see langword="null"/>.</returns>
    public static Guid? Guid(string? text) => !string.IsNullOrWhiteSpace(text) && 
        System.Guid.TryParse(text, out var value) ? value : null;

    /// <summary>Parses a string to <see cref="double"/>, or returns <see langword="null"/> on failure.</summary>
    /// <param name="text">The string to parse.</param>
    /// <returns>The parsed value, or <see langword="null"/>.</returns>
    public static double? Double(string? text) => !string.IsNullOrWhiteSpace(text) &&
        double.TryParse(text, out var value) ? value : null;
}
