namespace RossWright.MetalCommand;

public static class ShowProgressConsoleExtensions
{
    public static void ShowProgress(this IConsole console,
        Action<Action<double>> body,
        IProgressIndicator[]? indicators = null) =>
        _ = ShowProgress(console, 
            new Func<Action<double>, Task<object>>(_ =>
            {
                body(_);
                return Task.FromResult((object)null!);
            }), indicators);

    public static TResult ShowProgress<TResult>(this IConsole console,
        Func<Action<double>, TResult> body, IProgressIndicator[]? indicators = null) =>
        ShowProgress(console, new Func<Action<double>, Task<TResult>>(_ => Task.FromResult(body(_))), indicators).Result;

    public static Task ShowProgress(this IConsole console,
        Func<Action<double>, Task> body, IProgressIndicator[]? indicators = null) =>
        ShowProgress(console, new Func<Action<double>, Task<object>>(async _ =>
        {
            await body(_);
            return null!;
        }), indicators);

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
