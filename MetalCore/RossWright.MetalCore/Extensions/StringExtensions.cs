using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Web;

namespace RossWright;

/// <summary>Extension methods for <see cref="string"/> and string sequences.</summary>
public static class StringExtensions
{
    /// <summary>
    /// Joins a string sequence, wrapping each element in <paramref name="quote"/> characters
    /// and separating them with <paramref name="delimiter"/>.
    /// </summary>
    /// <param name="input">The strings to join.</param>
    /// <param name="delimiter">The character placed between elements. Defaults to <c>,</c>.</param>
    /// <param name="quote">The character used to wrap each element. Defaults to <c>"</c>.</param>
    /// <returns>A delimited, quoted string, or <see cref="string.Empty"/> if the sequence is empty.</returns>
    public static string JoinWithQuotes(this IEnumerable<string> input, char delimiter = ',', char quote = '"')
    {
        if (input == null || !input.Any()) return string.Empty;
        return string.Join(delimiter, input.Select(s => $"{quote}{s}{quote}"));
    }

    /// <summary>
    /// Splits <paramref name="txt"/> on commas while treating double-quoted regions as
    /// single tokens.
    /// </summary>
    /// <param name="txt">The string to split.</param>
    /// <returns>An array of tokens.</returns>
    public static IEnumerable<string> SplitAroundQuotes(this string? txt) =>
        SplitAroundQuotes(txt, [','], ['"'], StringSplitOptions.None);
    /// <inheritdoc cref="SplitAroundQuotes(string?, char[], char[], StringSplitOptions)"/>
    public static IEnumerable<string> SplitAroundQuotes(this string? txt, char splitChar, StringSplitOptions options = StringSplitOptions.None) =>
        SplitAroundQuotes(txt, [splitChar], ['"'], options);

    /// <inheritdoc cref="SplitAroundQuotes(string?, char[], char[], StringSplitOptions)"/>
    public static IEnumerable<string> SplitAroundQuotes(this string? txt, char[] splitChars, StringSplitOptions options = StringSplitOptions.None) =>
        SplitAroundQuotes(txt, splitChars, ['"'], options);

    /// <inheritdoc cref="SplitAroundQuotes(string?, char[], char[], StringSplitOptions)"/>
    public static IEnumerable<string> SplitAroundQuotes(this string? txt, params char[] splitChars) =>
        SplitAroundQuotes(txt, splitChars, ['"'], StringSplitOptions.None);
    /// <summary>
    /// Splits <paramref name="txt"/> on any of <paramref name="splitChars"/> while treating regions
    /// enclosed by <paramref name="groupDelimiters"/> as single unsplit tokens.
    /// </summary>
    /// <param name="txt">The string to split, or <see langword="null"/>.</param>
    /// <param name="splitChars">Characters that act as delimiters between tokens.</param>
    /// <param name="groupDelimiters">Characters that open and close grouping regions (e.g., quotes).</param>
    /// <param name="options">Controls whether empty entries are removed from the result.</param>
    /// <returns>A sequence of tokens split around grouping regions.</returns>
    public static IEnumerable<string> SplitAroundQuotes(this string? txt, char[] splitChars, char[] groupDelimiters, StringSplitOptions options = StringSplitOptions.None)
    {
        if (string.IsNullOrWhiteSpace(txt)) return Array.Empty<string>();
        if (true != splitChars?.Any()) return [txt];
        if (groupDelimiters == null) groupDelimiters = Array.Empty<char>();

        List<string> parts = new();
        bool inQuotes = false;
        StringBuilder partBuilder = new();
        for (var i = 0; i < txt.Length; i++)
        {
            if (groupDelimiters.Contains(txt[i]))
            {
                if (i + 1 < txt.Length && txt[i + 1] == txt[i])
                {
                    partBuilder.Append(txt[i]);
                    i++; // skip repeated group delimiters
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (!inQuotes && splitChars.Contains(txt[i]))
            {
                if (partBuilder.Length > 0)
                {
                    parts.Add(partBuilder.ToString());
                }
                partBuilder.Clear();
            }
            else
            {
                partBuilder.Append(txt[i]);
            }
        }
        if (partBuilder.Length > 0)
        {
            parts.Add(partBuilder.ToString());
        }

        if (options.HasFlag(StringSplitOptions.RemoveEmptyEntries))
        {
            parts = parts.Where(_ => !string.IsNullOrWhiteSpace(_)).ToList();
        }

        if (options.HasFlag(StringSplitOptions.TrimEntries))
        {
            parts = parts.Select(_ => _.Trim()).ToList();
        }

        return parts.ToArray();
    }

    /// <summary>
    /// Splits <paramref name="txt"/> at characters where <paramref name="predicate"/> returns <see langword="true"/>.
    /// </summary>
    /// <param name="txt">The string to split.</param>
    /// <param name="predicate">Returns <see langword="true"/> for delimiter characters.</param>
    /// <param name="options">Options to trim entries or remove empty results.</param>
    /// <returns>An array of substrings between delimiter positions.</returns>
    public static string[] Split(this string? txt, Func<char, bool> predicate, StringSplitOptions options = StringSplitOptions.None)
    {
        if (string.IsNullOrWhiteSpace(txt)) return Array.Empty<string>();

        List<string> parts = new();
        StringBuilder partBuilder = new();
        for (var i = 0; i < txt.Length; i++)
        {
            if (predicate(txt[i]))
            {
                if (!options.HasFlag(StringSplitOptions.RemoveEmptyEntries) || partBuilder.Length > 0)
                {
                    if (options.HasFlag(StringSplitOptions.TrimEntries))
                    {
                        parts.Add(partBuilder.ToString().Trim());
                    }
                    else
                    {
                        parts.Add(partBuilder.ToString());
                    }
                }
                partBuilder.Clear();
            }
            else
            {
                partBuilder.Append(txt[i]);
            }
        }
        if (partBuilder.Length > 0)
            parts.Add(options.HasFlag(StringSplitOptions.TrimEntries) ? partBuilder.ToString().Trim() : partBuilder.ToString());
        return parts.ToArray();
    }

    /// <summary>
    /// Replaces characters that are invalid in file names, and spaces, with underscores.
    /// </summary>
    /// <param name="input">The string to sanitize.</param>
    /// <returns>A string safe to use as a file name.</returns>
    public static string MakeSafeFileName(this string input) => Path
        .GetInvalidFileNameChars()
        .Append(' ')
        .Aggregate(input, (f, c) => f.Replace(c, '_'));

    /// <summary>
    /// Returns <see langword="null"/> if the string is null, empty, or whitespace;
    /// otherwise returns the original string.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns>The original string, or <see langword="null"/>.</returns>
    public static string? NullIfEmptyOrWhitespace(this string? input) =>
        string.IsNullOrWhiteSpace(input) ? null : input;

    /// <summary>Returns a new string containing only characters in the <paramref name="allowed"/> set.</summary>
    /// <param name="input">The source string.</param>
    /// <param name="allowed">The set of characters to keep.</param>
    /// <returns>A filtered string, or <see langword="null"/> when the input is null.</returns>
    public static string? Filter(this string? input, params char[] allowed) =>
        input is null ? null : new string(input.Where(_ => allowed.Contains(_)).ToArray());

    /// <summary>Returns a new string containing only characters where <paramref name="predicate"/> returns <see langword="true"/>.</summary>
    /// <param name="txt">The source string.</param>
    /// <param name="predicate">The character-selection predicate.</param>
    /// <returns>A filtered string, or <see langword="null"/> when the input is null or empty.</returns>
    public static string? Filter(this string? txt, Func<char, bool> predicate) =>
        string.IsNullOrEmpty(txt) ? null : new string(txt.Where(predicate).ToArray());

    /// <summary>Returns a new string with all occurrences of the specified characters removed.</summary>
    /// <param name="input">The source string.</param>
    /// <param name="without">The characters to remove.</param>
    /// <returns>The string with the specified characters stripped out, or <see langword="null"/> when the input is null.</returns>
    public static string? Without(this string? input, params char[] without) =>
        input is null ? null : new string(input.Where(_ => !without.Contains(_)).ToArray());

    /// <summary>Truncates the string to at most <paramref name="maxChars"/> characters.</summary>
    /// <param name="input">The string to truncate.</param>
    /// <param name="maxChars">The maximum number of characters to keep.</param>
    /// <returns>The original string if shorter; otherwise the first <paramref name="maxChars"/> characters.</returns>
    public static string? Clip(this string? input, int maxChars) =>
        input?.Substring(0, Math.Min(input.Length, maxChars));
    
    /// <summary>Converts each word to Title Case (first letter upper, remainder lower). Words are separated by spaces.</summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>A Title Case string, or <see langword="null"/> if the input is null or whitespace.</returns>
    public static string? TitleCase(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
        }
        return string.Join(" ", words);
    }

    /// <summary>Capitalizes the first character of the string, leaving the rest unchanged.</summary>
    /// <param name="input">The string to capitalize.</param>
    /// <returns>The string with its first character uppercased, or <see langword="null"/> if the input is null or empty.</returns>
    public static string? CapFirst(this string? input)
    {
        if (input == null || !input.Any()) return null;
        Span<char> stringContent = input.ToCharArray();
        stringContent[0] = char.ToUpper(stringContent[0]);
        return stringContent.ToString();
    }

    /// <summary>
    /// Joins a string sequence into a natural-language list using
    /// <paramref name="conjunction"/> before the final element.
    /// </summary>
    /// <param name="input">The strings to join. Null and whitespace entries are excluded.</param>
    /// <param name="conjunction">The word placed before the last element. Defaults to <c>"and"</c>.</param>
    /// <returns>A formatted list (e.g. <c>"a, b and c"</c>), or <see langword="null"/> if the sequence is empty.</returns>
    public static string? CommaListJoin(this IEnumerable<string?>? input, string conjunction = "and")
    {
        input = input?.Where(_ => !string.IsNullOrWhiteSpace(_)).ToArray();
        if (input == null || !input.Any()) return null;
        var count = input.Count();
        if (count == 1) return input.First();
        return $"{string.Join(", ", input.Take(count - 1))} {conjunction} {input.Last()}";
    }

    /// <summary>
    /// Returns a different string depending on whether the collection is empty,
    /// has exactly one element, or has more than one.
    /// </summary>
    /// <typeparam name="TResult">The element type.</typeparam>
    /// <param name="collection">The collection to evaluate.</param>
    /// <param name="many">Called when there are two or more elements.</param>
    /// <param name="one">Called with the single element when there is exactly one.</param>
    /// <param name="zero">Returned when the collection is null or empty.</param>
    /// <returns>The appropriate string for the collection count.</returns>
    public static string ZeroOneOrMany<TResult>(this IEnumerable<TResult>? collection,
        Func<IEnumerable<TResult>, string> many, Func<TResult, string> one, string zero)
    {
        if (collection?.Any() != true) return zero;
        if (collection.Count() == 1) return one(collection.First());
        return many(collection);
    }

    /// <summary>Applies <paramref name="formatter"/> to <paramref name="value"/> only when its string representation is non-empty; otherwise returns <see cref="string.Empty"/>.</summary>
    /// <param name="value">The value to format.</param>
    /// <param name="formatter">The formatting function to apply when present.</param>
    /// <returns>The formatted string, or <see cref="string.Empty"/>.</returns>
    public static string ToStringIfPresent(this object? value, Func<object, string> formatter) =>
        string.IsNullOrWhiteSpace(value?.ToString()) ? string.Empty : formatter(value);

    /// <summary>Prepends a space to the string representation of <paramref name="value"/> when non-empty; otherwise returns the raw value.</summary>
    /// <param name="value">The value to check.</param>
    /// <returns>The value prefixed with a space when present, or the raw value otherwise.</returns>
    public static string? PreSpaceIfPresent(this object? value) =>
        string.IsNullOrWhiteSpace(value?.ToString()) ? value?.ToString() : $" {value}";

    /// <summary>Returns a string of the same length as <paramref name="text"/> filled with <paramref name="butAs"/>, with optional length clamping.</summary>
    /// <param name="text">The string whose length is used as the template.</param>
    /// <param name="butAs">The character to fill the output with.</param>
    /// <param name="minLength">Optional minimum output length.</param>
    /// <param name="maxLength">Optional maximum output length.</param>
    /// <returns>A repeated-character string.</returns>
    public static string ButAll(this string text, char butAs, int? minLength = null, int? maxLength = null) =>
        new string(butAs, text.Length.Clamp(minLength, maxLength));

    /// <summary>Appends <paramref name="punctuation"/> if the string does not already end with <c>.</c>, <c>?</c>, or <c>!</c>.</summary>
    /// <param name="input">The string to terminate.</param>
    /// <param name="punctuation">The punctuation to append. Defaults to <c>"."</c>.</param>
    /// <returns>The string with punctuation appended, or the original if already terminated.</returns>
    public static string? EndSentence(this string? input, string punctuation = ".") =>
        (!string.IsNullOrWhiteSpace(input) && !input.Last().In('.', '?', '!')) ? $"{input}{punctuation}" : input;

    /// <summary>Strips all non-digit characters from the string.</summary>
    /// <param name="txt">The string to process.</param>
    /// <returns>A string containing only digit characters.</returns>
    public static string ToOnlyDigits(this string txt)
    {
        var b = new StringBuilder();
        foreach (var c in txt)
        {
            if (char.IsDigit(c)) b.Append(c);
        }
        return b.ToString();
    }

    /// <summary>Converts PascalCase, camelCase, and Snake_Case to space-separated words.</summary>
    /// <param name="text">The string to transform.</param>
    /// <returns>A space-separated string.</returns>
    public static string SpaceOut(this string text)
    {
        var b = new StringBuilder();
        bool readyToSpace = false;
        foreach (var c in text)
        {
            if (readyToSpace && char.IsUpper(c)) b.Append(' ');
            b.Append(c == '_' ? ' ' : c);
            readyToSpace = char.IsLower(c);
        }
        return b.ToString();
    }

    /// <summary>Appends a URL-encoded query parameter to <paramref name="url"/>.</summary>
    /// <param name="url">The base URL.</param>
    /// <param name="name">The query parameter name.</param>
    /// <param name="value">The query parameter value.</param>
    /// <returns>The URL with the new query parameter appended.</returns>
    public static string WithQueryParameter(this string url, string name, string value) =>
            $"{url}{(url.Contains('?') ? "&" : "?")}{HttpUtility.UrlEncode(name)}={HttpUtility.UrlEncode(value)}";

    /// <summary>Calculates the normalised Levenshtein distance between <paramref name="s1"/> and <paramref name="s2"/>.</summary>
    /// <param name="s1">The first string to compare, or <see langword="null"/>.</param>
    /// <param name="s2">The second string to compare, or <see langword="null"/>.</param>
    /// <returns>A value representing the edit distance after normalising both strings to lowercase with whitespace removed; 0 if either input is empty.</returns>
    public static double CalcLevenshteinDistanceTo(this string? s1, string? s2)
    {
        string? NormalizeDescription(string? desc) => desc?.Trim().ToLowerInvariant().Replace(" ", "");
        s1 = NormalizeDescription(s1);
        s2 = NormalizeDescription(s2);
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0;

        int len1 = s1.Length, len2 = s2.Length;
        var matrix = new int[len1 + 1, len2 + 1];
        for (int i = 0; i <= len1; i++) matrix[i, 0] = i;
        for (int j = 0; j <= len2; j++) matrix[0, j] = j;

        for (int i = 1; i <= len1; i++)
            for (int j = 1; j <= len2; j++)
                matrix[i, j] = Math.Min(Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                                        matrix[i - 1, j - 1] + (s1[i - 1] == s2[j - 1] ? 0 : 1));

        return 1.0 - (double)matrix[len1, len2] / Math.Max(len1, len2);

    }

    /// <summary>Encodes the string as a UTF-8 Base64 string.</summary>
    /// <param name="text">The string to encode, or <see langword="null"/>.</param>
    /// <returns>The Base64-encoded string, or <see langword="null"/> if the input is <see langword="null"/>.</returns>
    public static string? ToBase64String([NotNullIfNotNull(nameof(text))] this string? text) =>
        text is null ? null : Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

    /// <summary>Decodes a UTF-8 Base64 string back to a plain string.</summary>
    /// <param name="base64Text">The Base64-encoded string to decode, or <see langword="null"/>.</param>
    /// <returns>The decoded string, or <see langword="null"/> if the input is <see langword="null"/>.</returns>
    public static string? FromBase64String([NotNullIfNotNull(nameof(base64Text))] this string? base64Text) =>
        base64Text is null ? null : Encoding.UTF8.GetString(Convert.FromBase64String(base64Text));
}
