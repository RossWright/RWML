using System.Text;
using System.Text.Json;

namespace RossWright;

/// <summary>
/// JSON formatting utilities.
/// </summary>
public static class JsonFormatter
{
    /// <summary>
    /// Pretty-prints a JSON string with consistent indentation.
    /// Useful for debug display and structured log entries.
    /// </summary>
    /// <param name="jsonString">A valid JSON string to format.</param>
    /// <returns>An indented, human-readable JSON string.</returns>
    public static string Format(string jsonString)
    {
        using var document = JsonDocument.Parse(jsonString);
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        document.WriteTo(writer);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}