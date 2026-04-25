namespace RossWright;

/// <summary>
/// Extension methods for <see cref="double"/>, <see cref="double"/>[], <see cref="double"/>?[], and <see cref="IEnumerable{T}"/> of <see cref="double"/>.
/// </summary>
public static class DoubleExtensions
{
    /// <summary>
    /// Computes the standard deviation of a sequence of <see cref="double"/> values.
    /// </summary>
    /// <param name="values">The source sequence. Must contain at least one element.</param>
    /// <returns>The population standard deviation of the sequence.</returns>
    public static double StandardDeviation(this IEnumerable<double> values)
    {
        var avg = values.Average();
        var count = values.Count();
        return Math.Sqrt(values.Sum(val => (val - avg) * (val - avg)) / count);
    }

    /// <summary>
    /// Formats a <see cref="double"/> as an accounting-style string: positive values as <c>1,234.56</c> and negative values in parentheses as <c>(1,234.56)</c>.
    /// Zero is formatted as <c>"-"</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The formatted accounting string.</returns>
    public static string ToAccountingString(this double value) =>
        value.ToString("#,##0.00;(#,##0.00);-");

    /// <summary>
    /// Returns <see langword="null"/> if the value is <see cref="double.NaN"/> or infinity; otherwise returns the value.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <returns><see langword="null"/> for non-real values; otherwise the original <see cref="double"/>.</returns>
    public static double? NullIfNotReal(this double value) =>
        !double.IsNaN(value) && !double.IsInfinity(value) ? value : (double?)null;

    /// <summary>
    /// Down-samples a large <see cref="double"/>[] to a target length by averaging values within each bucket.
    /// Returns the original array unchanged if it is already at or below <paramref name="sampleCount"/>.
    /// </summary>
    /// <param name="source">The source data array.</param>
    /// <param name="sampleCount">The desired output length. Defaults to <c>2000</c>.</param>
    /// <returns>A new array of length <paramref name="sampleCount"/> containing bucket averages, or the original array if no reduction was needed.</returns>
    public static double[] Downsample(this double[] source, int sampleCount = 2000)
    {
        if (source.Length <= sampleCount) return source;
        var result = new double[sampleCount];
        float inc = (float)source.Length / (float)sampleCount;
        int index = 0;
        while (index < sampleCount)
        {
            var start = (int)Math.Floor(index * inc);
            var end = (int)Math.Min(source.Length, Math.Ceiling((index + 1) * inc));
            double total = 0;
            int count = 0;
            for (var i = start; i < end; i++)
            {
                count++;
                total += source[i];
            }
            result[index] = total / count;
            index++;
        }
        return result;
    }
    /// <summary>
    /// Down-samples a large nullable <see cref="double"/>?[] to a target length by averaging the non-null values within each bucket.
    /// Buckets where all values are <see langword="null"/> produce a <see langword="null"/> output element.
    /// Returns the original array unchanged if it is already at or below <paramref name="sampleCount"/>.
    /// </summary>
    /// <param name="source">The source nullable data array.</param>
    /// <param name="sampleCount">The desired output length. Defaults to <c>2000</c>.</param>
    /// <returns>A new nullable array of length <paramref name="sampleCount"/> containing bucket averages, or the original array if no reduction was needed.</returns>
    public static double?[] Downsample(this double?[] source, int sampleCount = 2000)
    {
        if (source.Length <= sampleCount) return source;
        var result = new double?[sampleCount];
        float inc = (float)source.Length / (float)sampleCount;
        int index = 0;
        while (index < sampleCount)
        {
            var start = (int)Math.Floor(index * inc);
            var end = (int)Math.Min(source.Length, Math.Ceiling((index + 1) * inc));
            double total = 0;
            int count = 0;
            for (var i = start; i < end; i++)
            {
                if (source[i].HasValue)
                {
                    count++;
                    total += source[i]!.Value;
                }
            }
            result[index] = count == 0 ? null : total / count;
            index++;
        }
        return result;
    }

    /// <summary>Converts an angle from degrees to radians.</summary>
    /// <param name="degreeAngle">The angle in degrees.</param>
    /// <returns>The equivalent angle in radians.</returns>
    public static double FromDegreesToRadians(this double degreeAngle) => (Math.PI * degreeAngle / 180.0);

    /// <summary>Converts an angle from radians to degrees.</summary>
    /// <param name="radianAngle">The angle in radians.</param>
    /// <returns>The equivalent angle in degrees.</returns>
    public static double FromRadiansToDegrees(this double radianAngle) => (radianAngle * 180.0 / Math.PI);

    /// <summary>
    /// Clamps a <see cref="double"/> to optional minimum and/or maximum bounds.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The inclusive lower bound, or <see langword="null"/> for no lower bound.</param>
    /// <param name="max">The inclusive upper bound, or <see langword="null"/> for no upper bound.</param>
    /// <returns>The clamped value.</returns>
    public static double Clamp(this double value, double? min, double? max) =>
        Math.Min(Math.Max(value, min ?? double.MinValue), max ?? double.MaxValue);
}
