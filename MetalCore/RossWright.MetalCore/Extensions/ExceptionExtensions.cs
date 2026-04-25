using System.Text;

namespace RossWright;

/// <summary>
/// Exception formatting extensions.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Formats the full exception chain — type, message, stack trace, and all
    /// inner exceptions — as a readable multi-line string. More useful than
    /// <see cref="Exception.ToString"/> for structured log entries.
    /// </summary>
    /// <param name="exception">The exception to format.</param>
    /// <returns>A multi-line string describing the exception and its entire inner chain.</returns>
    public static string ToBetterString(this Exception exception)
    {
        var message = new StringBuilder();
        Exception? inner = exception;
        do
        {
            message.AppendLine($"{inner.GetType()} {inner.Message}");
            if (inner.StackTrace != null)
                message.AppendLine(inner.StackTrace);
            inner = inner.InnerException;
            if (inner != null)
                message.AppendLine("Inner Exception:");
        }
        while (inner != null);

        return message.ToString();
    }
}
