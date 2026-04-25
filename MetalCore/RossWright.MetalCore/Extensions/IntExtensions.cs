namespace RossWright;

/// <summary>
/// Extension methods for <see cref="int"/>.
/// </summary>
public static class IntExtensions
{
    /// <summary>
    /// Clamps an integer to optional minimum and/or maximum bounds.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The inclusive lower bound, or <see langword="null"/> for no lower bound.</param>
    /// <param name="max">The inclusive upper bound, or <see langword="null"/> for no upper bound.</param>
    /// <returns>The clamped value.</returns>
    public static int Clamp(this int value, int? min, int? max) => 
        Math.Min(Math.Max(value, min ?? int.MinValue), max ?? int.MaxValue);
}