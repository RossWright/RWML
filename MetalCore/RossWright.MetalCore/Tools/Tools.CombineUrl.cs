namespace RossWright;

public static partial class Tools
{
    /// <summary>
    /// Joins URL path segments into a single URL, trimming leading and trailing
    /// slashes from each segment and skipping any that are null or whitespace.
    /// </summary>
    /// <param name="parts">The path segments to join.</param>
    /// <returns>A URL string with segments separated by <c>/</c>.</returns>
    public static string CombineUrl(params string[] parts) =>
        string.Join("/", parts
            .Where(_ => !string.IsNullOrWhiteSpace(_))
            .Select(_ => _.Trim('/'))            
            .ToArray());
}
