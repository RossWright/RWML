namespace RossWright;

/// <summary>
/// Destructuring extension methods for <see cref="string"/> arrays, returning typed value tuples of 2–10 elements.
/// </summary>
public static class GetTupleFromStringArrayExtensions
{
    /// <summary>Destructures the first 2 elements of a <see cref="string"/> array into a value tuple.</summary>
    /// <param name="args">The source string array. Must contain at least 2 elements.</param>
    /// <returns>A 2-element value tuple of strings.</returns>
    public static (string, string) GetTwo(this string[] args) => 
        (args[0], args[1]);

    /// <summary>Destructures the first 3 elements of a <see cref="string"/> array into a value tuple.</summary>
    /// <param name="args">The source string array. Must contain at least 3 elements.</param>
    /// <returns>A 3-element value tuple of strings.</returns>
    public static (string, string, string) GetThree(this string[] args) => 
        (args[0], args[1], args[2]);

    /// <summary>Destructures the first 4 elements of a <see cref="string"/> array into a value tuple.</summary>
    /// <param name="args">The source string array. Must contain at least 4 elements.</param>
    /// <returns>A 4-element value tuple of strings.</returns>
    public static (string, string, string, string) GetFour(this string[] args) => 
        (args[0], args[1], args[2], args[3]);

    /// <summary>Destructures the first 5 elements of a <see cref="string"/> array into a value tuple.</summary>
    /// <param name="args">The source string array. Must contain at least 5 elements.</param>
    /// <returns>A 5-element value tuple of strings.</returns>
    public static (string, string, string, string, string) GetFive(this string[] args) => 
        (args[0], args[1], args[2], args[3], args[4]);

    /// <summary>Destructures the first 6 elements of a <see cref="string"/> array into a value tuple.</summary>
    /// <param name="args">The source string array. Must contain at least 6 elements.</param>
    /// <returns>A 6-element value tuple of strings.</returns>
    public static (string, string, string, string, string, string) GetSix(this string[] args) => 
        (args[0], args[1], args[2], args[3], args[4], args[5]);

    /// <summary>Destructures the first 7 elements of a <see cref="string"/> array into a value tuple.</summary>
    /// <param name="args">The source string array. Must contain at least 7 elements.</param>
    /// <returns>A 7-element value tuple of strings.</returns>
    public static (string, string, string, string, string, string, string) GetSeven(this string[] args) =>
        (args[0], args[1], args[2], args[3], args[4], args[5], args[6]);

    /// <summary>Destructures the first 8 elements of a <see cref="string"/> array into a value tuple.</summary>
    /// <param name="args">The source string array. Must contain at least 8 elements.</param>
    /// <returns>An 8-element value tuple of strings.</returns>
    public static (string, string, string, string, string, string, string, string) GetEight(this string[] args) =>
        (args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);

    /// <summary>Destructures the first 9 elements of a <see cref="string"/> array into a value tuple.</summary>
    /// <param name="args">The source string array. Must contain at least 9 elements.</param>
    /// <returns>A 9-element value tuple of strings.</returns>
    public static (string, string, string, string, string, string, string, string, string) GetNine(this string[] args) =>
        (args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);

    /// <summary>Destructures the first 10 elements of a <see cref="string"/> array into a value tuple.</summary>
    /// <param name="args">The source string array. Must contain at least 10 elements.</param>
    /// <returns>A 10-element value tuple of strings.</returns>
    public static (string, string, string, string, string, string, string, string, string, string) GetTen(this string[] args) =>
        (args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
}