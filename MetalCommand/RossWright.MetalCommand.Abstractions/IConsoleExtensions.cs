using System.Text.Json;

namespace RossWright.MetalCommand;

/// <summary>
/// Extension methods for <see cref="IConsole"/> providing announce patterns, JSON dumping,
/// indented writing, and interactive prompts.
/// </summary>
public static class IConsoleExtensions
{
    /// <summary>
    /// Writes <c>"announcement... "</c>, runs <paramref name="action"/> with indented output,
    /// then writes the conclusion on the same line.
    /// </summary>
    /// <param name="console">The console to write to.</param>
    /// <param name="announcement">The task description (trailing dots are stripped before <c>"..."</c> is appended).</param>
    /// <param name="action">The synchronous work to perform.</param>
    /// <param name="conclusion">Optional factory for the completion text. Defaults to <c>"Done!"</c>.</param>
    public static void Announce(this IConsole console, string announcement, Action action, Func<string>? conclusion = null)
    {
        _ = console.AnnounceAsync(announcement, () =>
        {
            action();
            return Task.CompletedTask;
        }, conclusion);
    }

    /// <summary>
    /// Writes <c>"announcement... "</c>, awaits <paramref name="action"/> with indented output,
    /// then writes the conclusion on the same line.
    /// </summary>
    /// <param name="console">The console to write to.</param>
    /// <param name="announcement">The task description (trailing dots are stripped before <c>"..."</c> is appended).</param>
    /// <param name="action">The asynchronous work to perform.</param>
    /// <param name="conclusion">Optional factory for the completion text. Defaults to <c>"Done!"</c>.</param>
    public static async Task AnnounceAsync(this IConsole console, string announcement, Func<Task> action, Func<string>? conclusion = null)
    {
        console.ResetLine();
        console.Write(announcement.TrimEnd('.') + "... ");
        using (console.Indent())
        {
            await action();
        }
        console.WriteLine(conclusion != null ? conclusion() : "Done!");
    }

    /// <summary>
    /// Writes <c>"announcement... "</c>, runs <paramref name="action"/> with indented output,
    /// writes the conclusion, and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type returned by <paramref name="action"/>.</typeparam>
    /// <param name="console">The console to write to.</param>
    /// <param name="announcement">The task description.</param>
    /// <param name="action">The synchronous work to perform. Its return value is passed to <paramref name="conclusion"/>.</param>
    /// <param name="conclusion">Optional factory that receives the result and returns the completion text.</param>
    /// <returns>The value returned by <paramref name="action"/>.</returns>
    public static TResult Announce<TResult>(this IConsole console, string announcement, Func<TResult> action, Func<TResult, string>? conclusion = null)
    {
        TResult value = default(TResult)!;
        _ = console.AnnounceAsync(announcement, () =>
        {
            value = action();
            return Task.CompletedTask;
        }, conclusion == null ? null : () => conclusion(value));
        return value;
    }

    /// <summary>
    /// Writes <c>"announcement... "</c>, awaits <paramref name="action"/> with indented output,
    /// writes the conclusion, and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type returned by <paramref name="action"/>.</typeparam>
    /// <param name="console">The console to write to.</param>
    /// <param name="announcement">The task description.</param>
    /// <param name="action">The asynchronous work to perform.</param>
    /// <param name="conclusion">Optional factory that receives the result and returns the completion text.</param>
    /// <returns>The value returned by <paramref name="action"/>.</returns>
    public static async Task<TResult> AnnounceAsync<TResult>(this IConsole console, string announcement, Func<Task<TResult>> action, Func<TResult, string>? conclusion = null)
    {
        TResult value = default(TResult)!;
        await console.AnnounceAsync(announcement,
            async () => value = await action(),
            conclusion == null ? null : () => conclusion(value));
        return value;
    }

    /// <summary>
    /// Pretty-prints a JSON string at the current indent level. Does nothing when <paramref name="json"/> is <see langword="null"/>.
    /// </summary>
    /// <param name="console">The console to write to.</param>
    /// <param name="json">A JSON string to format and print.</param>
    public static void DumpJson(this IConsole console, string? json)
    {
        if (json != null)
        {
            console.WriteLineIndented(JsonFormatter.Format(json));
        }
    }

    /// <summary>
    /// Serializes <paramref name="obj"/> to JSON and pretty-prints it at the current indent level.
    /// Does nothing when <paramref name="obj"/> is <see langword="null"/>.
    /// </summary>
    /// <param name="console">The console to write to.</param>
    /// <param name="obj">The object to serialize and print.</param>
    public static void DumpJson(this IConsole console, object? obj)
    {
        if (obj != null)
        {
            console.DumpJson(JsonSerializer.Serialize(obj));
        }
    }

    /// <summary>
    /// Splits <paramref name="text"/> on newlines and writes each line at the current indent level.
    /// </summary>
    /// <param name="console">The console to write to.</param>
    /// <param name="text">Multi-line text to write.</param>
    /// <param name="textColor">Optional foreground color override.</param>
    /// <param name="backgroundColor">Optional background color override.</param>
    public static void WriteLineIndented(this IConsole console, string text, ConsoleColor? textColor = null, ConsoleColor? backgroundColor = null)
    {
        foreach (var line in text.Split(Environment.NewLine))
        {
            console.WriteLine(line, textColor, backgroundColor);
        }
    }

    /// <summary>
    /// Writes a yes/no prompt and waits for the user to answer.
    /// Accepts <c>y</c>, <c>yes</c>, <c>n</c>, or <c>no</c> (case-insensitive).
    /// Re-prompts on any other input.
    /// </summary>
    /// <param name="console">The console to read from and write to.</param>
    /// <param name="prompt">The question to display before the <c>[y/n]</c> hint.</param>
    /// <param name="defaultYes">
    /// When <see langword="true"/>, pressing Enter with no input counts as <c>yes</c>.
    /// When <see langword="false"/> (default), pressing Enter counts as <c>no</c>.
    /// </param>
    /// <returns><see langword="true"/> if the user answered yes, <see langword="false"/> for no.</returns>
    public static bool Confirm(this IConsole console, string prompt, bool defaultYes = false)
    {
        var hint = defaultYes ? "[Y/n]" : "[y/N]";
        while (true)
        {
            console.Write($"{prompt} {hint}: ");
            var input = console.ReadLine()?.Trim() ?? "";

            if (input.Length == 0)
                return defaultYes;

            if (input.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("yes", StringComparison.OrdinalIgnoreCase))
                return true;

            if (input.Equals("n", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("no", StringComparison.OrdinalIgnoreCase))
                return false;

            console.WriteErrorLine("Please enter y or n.");
        }
    }

    /// <summary>
    /// Writes a prompt and returns the user's input.
    /// When the user presses Enter with no input, <paramref name="defaultValue"/> is returned.
    /// </summary>
    /// <param name="console">The console to read from and write to.</param>
    /// <param name="prompt">The prompt text to display.</param>
    /// <param name="defaultValue">
    /// Value returned when the user presses Enter without typing anything.
    /// When non-null, it is shown in brackets after the prompt.
    /// </param>
    /// <returns>The user's input, or <paramref name="defaultValue"/> if no input was given.</returns>
    public static string? Prompt(this IConsole console, string prompt, string? defaultValue = null)
    {
        var hint = defaultValue != null ? $" [{defaultValue}]" : "";
        console.Write($"{prompt}{hint}: ");
        var input = console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(input) ? defaultValue : input;
    }

    /// <summary>
    /// Presents a numbered list of options and returns the chosen value.
    /// Re-prompts on invalid input.
    /// Options are displayed using <see cref="object.ToString"/>.
    /// </summary>
    /// <typeparam name="T">The type of the options. Can be any type — string, enum, etc.</typeparam>
    /// <param name="console">The console to read from and write to.</param>
    /// <param name="prompt">The prompt text displayed above the numbered list.</param>
    /// <param name="options">The set of values the user may choose from.</param>
    /// <returns>The selected option value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="options"/> is empty.</exception>
    public static T Choose<T>(this IConsole console, string prompt, T[] options) where T : notnull
    {
        if (options.Length == 0)
            throw new ArgumentException("At least one option must be provided.", nameof(options));

        while (true)
        {
            console.WriteLine(prompt);
            using (console.Indent())
            {
                for (var i = 0; i < options.Length; i++)
                {
                    console.WriteLine($"{i + 1}. {options[i]}");
                }
            }
            console.Write($"Enter 1-{options.Length}: ");
            var input = console.ReadLine()?.Trim() ?? "";

            if (int.TryParse(input, out var choice) && choice >= 1 && choice <= options.Length)
                return options[choice - 1];

            console.WriteErrorLine($"Please enter a number between 1 and {options.Length}.");
        }
    }
}