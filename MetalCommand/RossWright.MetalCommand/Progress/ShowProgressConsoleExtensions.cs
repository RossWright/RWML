namespace RossWright.MetalCommand;

/// <summary>
/// Extension methods for <see cref="IConsole"/> that render inline progress indicators
/// while a body callback executes.
/// </summary>
public static class ShowProgressConsoleExtensions
{
    /// <summary>
    /// Renders progress indicators while <paramref name="body"/> executes synchronously.
    /// </summary>
    /// <param name="console">The console to render on.</param>
    /// <param name="body">Synchronous work that calls <c>report(0.0–1.0)</c> to update progress.</param>
    /// <param name="indicators">Indicators to display. Defaults to <c>[Spinner, ProgressBar]</c>.</param>
    public static void ShowProgress(this IConsole console,
        Action<Action<double>> body,
        IProgressIndicator[]? indicators = null) =>
        _ = ShowProgress(console, 
            new Func<Action<double>, Task<object>>(_ =>
            {
                body(_);
                return Task.FromResult((object)null!);
            }), indicators);

    /// <summary>
    /// Renders progress indicators while <paramref name="body"/> executes synchronously and returns its result.
    /// </summary>
    /// <typeparam name="TResult">The return type of the body.</typeparam>
    /// <param name="console">The console to render on.</param>
    /// <param name="body">Synchronous work that calls <c>report(0.0–1.0)</c> and returns a value.</param>
    /// <param name="indicators">Indicators to display. Defaults to <c>[Spinner, ProgressBar]</c>.</param>
    /// <returns>The value returned by <paramref name="body"/>.</returns>
    public static TResult ShowProgress<TResult>(this IConsole console,
        Func<Action<double>, TResult> body, IProgressIndicator[]? indicators = null) =>
        ShowProgress(console, new Func<Action<double>, Task<TResult>>(_ => Task.FromResult(body(_))), indicators).Result;

    /// <summary>
    /// Renders progress indicators while <paramref name="body"/> executes asynchronously.
    /// </summary>
    /// <param name="console">The console to render on.</param>
    /// <param name="body">Asynchronous work that calls <c>report(0.0–1.0)</c>.</param>
    /// <param name="indicators">Indicators to display. Defaults to <c>[Spinner, ProgressBar]</c>.</param>
    public static Task ShowProgress(this IConsole console,
        Func<Action<double>, Task> body, IProgressIndicator[]? indicators = null) =>
        ShowProgress(console, new Func<Action<double>, Task<object>>(async _ =>
        {
            await body(_);
            return null!;
        }), indicators);

    /// <summary>
    /// Renders progress indicators while <paramref name="body"/> executes asynchronously and returns its result.
    /// </summary>
    /// <typeparam name="TResult">The return type of the body.</typeparam>
    /// <param name="console">The console to render on.</param>
    /// <param name="body">Asynchronous work that calls <c>report(0.0–1.0)</c> and returns a value.</param>
    /// <param name="indicators">Indicators to display. Defaults to <c>[Spinner, ProgressBar]</c>.</param>
    /// <returns>The value returned by <paramref name="body"/>.</returns>
    public static async Task<TResult> ShowProgress<TResult>(this IConsole console, 
        Func<Action<double>, Task<TResult>> body, IProgressIndicator[]? indicators = null)
    {
        if (indicators == null)
        {
            indicators =
            [            
                new Spinner(),
                new ProgressBar()
            ];
        }
        var indicatorsWidth = indicators!.Sum(_ => _.Width) + (indicators!.Length - 1);

        var backup = new string('\b', indicatorsWidth);
        var blank = new string(' ', indicatorsWidth);
        using var hideCursor = console.HideCursor();
        console.Write(blank);
        var progressUpdate = new Action<double>(progress =>
            console.Write(backup + string.Join(" ", indicators.Select(_ => _.Output(progress)))));
        try
        {
            return await body(progressUpdate);
        }
        finally
        {
            console.Write($"{backup}{blank}{backup}");
        }
    }
}
