namespace RossWright;

/// <summary>
/// General-purpose extension methods for all object types.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="value"/> equals any of the supplied candidates.
    /// </summary>
    /// <typeparam name="T">The type of the value and candidates.</typeparam>
    /// <param name="value">The value to test.</param>
    /// <param name="possible">The set of candidate values to compare against.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is present in <paramref name="possible"/>; otherwise <see langword="false"/>.</returns>
    public static bool In<T>(this T value, params T[] possible) => possible.Contains(value);
}

