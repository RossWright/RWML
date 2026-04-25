namespace RossWright;

/// <summary>Provides general-purpose utility methods for common application tasks.</summary>
public static partial class Tools
{
    /// <summary>
    /// Appends non-null query-string parameters to <paramref name="to"/> and
    /// returns the resulting URL. Parameters whose value is <see langword="null"/>
    /// are silently omitted.
    /// </summary>
    /// <param name="to">The base URL to append query parameters to.</param>
    /// <param name="queryParams">
    /// Name/value pairs to add as query parameters. Entries with a
    /// <see langword="null"/> value are skipped.
    /// </param>
    /// <returns>The URL with non-null query parameters appended.</returns>
    public static string BuildQuery(string to, params (string name, object? value)[]? queryParams)
    {
        if (queryParams?.Any() != true) return to;
        var items = System.Web.HttpUtility.ParseQueryString(string.Empty);
        foreach (var queryParam in queryParams)
            if (queryParam.value != null)
                items.Add(queryParam.name, queryParam.value.ToString());
        var query = items.ToString();

        var separator = to.Contains('?') ? "&" : "?";
        return $"{to}{separator}{query}";
    }
}
