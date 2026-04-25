namespace RossWright;

/// <summary>
/// Extension methods for nullable <see cref="bool"/>.
/// </summary>
public static class NullableBoolExtensions
{
    /// <summary>
    /// Returns <see langword="true"/> if the nullable boolean is <see langword="null"/> or <see langword="true"/>.
    /// </summary>
    /// <param name="value">The nullable boolean to test.</param>
    /// <returns><see langword="true"/> when <paramref name="value"/> is <see langword="null"/> or <see langword="true"/>; otherwise <see langword="false"/>.</returns>
    public static bool IsNullOrTrue(this bool? value) => value != false;

    /// <summary>
    /// Returns <see langword="true"/> if the nullable boolean is <see langword="null"/> or <see langword="false"/>.
    /// </summary>
    /// <param name="value">The nullable boolean to test.</param>
    /// <returns><see langword="true"/> when <paramref name="value"/> is <see langword="null"/> or <see langword="false"/>; otherwise <see langword="false"/>.</returns>
    public static bool IsNullOrFalse(this bool? value) => value != true;
}
