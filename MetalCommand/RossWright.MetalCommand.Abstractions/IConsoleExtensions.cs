using System.Text.Json;

namespace RossWright.MetalCommand;

public static class IConsoleExtensions
{
    public static void Announce(this IConsole console, string announcement, Action action, Func<string>? conclusion = null)
    {
        _ = console.AnnounceAsync(announcement, () =>
        {
            action();
            return Task.CompletedTask;
        }, conclusion);
    }

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

    public static async Task<TResult> AnnounceAsync<TResult>(this IConsole console, string announcement, Func<Task<TResult>> action, Func<TResult, string>? conclusion = null)
    {
        TResult value = default(TResult)!;
        await console.AnnounceAsync(announcement,
            async () => value = await action(),
            conclusion == null ? null : () => conclusion(value));
        return value;
    }

    public static void DumpJson(this IConsole console, string? json)
    {
        if (json != null)
        {
            console.WriteLineIndented(JsonFormatter.Format(json));
        }
    }

    public static void DumpJson(this IConsole console, object? obj)
    {
        if (obj != null)
        {
            console.DumpJson(JsonSerializer.Serialize(obj));
        }
    }

    public static void WriteLineIndented(this IConsole console, string text)
    {
        foreach (var line in text.Split(Environment.NewLine))
        {
            console.WriteLine(line);
        }
    }
}