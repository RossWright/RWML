namespace RossWright.MetalCommand;

/// <summary>
/// An <see cref="IProgressIndicator"/> that renders a Unicode block-fill bar with an optional
/// percentage overlaid in the unfilled region.
/// </summary>
public class ProgressBar : IProgressIndicator
{
    /// <summary>
    /// Initializes a new <see cref="ProgressBar"/>.
    /// </summary>
    /// <param name="showPercent">When <see langword="true"/> (default), overlays the percentage text inside the bar.</param>
    /// <param name="length">Total character width including the surrounding brackets. Defaults to 52.</param>
    public ProgressBar(bool showPercent = true, int length = 52)
    {
        this.showPercent = showPercent;
        barLength = length - 2;
    }

    private const string pieces = " \u258C\u2588";
    /// <inheritdoc/>
    public int Width => barLength + 2;
    private int barLength = 50;
    bool showPercent = true;
    /// <inheritdoc/>
    public string Output(double progress)
    {
        var barInc = 100 / barLength;
        var bar = new char[barLength];
        for (var i = 0; i < barLength; i++)
        {
            var segment = (int)Math.Floor(Math.Min(barInc, 
                Math.Max(0, progress * 100 - i * barInc)));
            var thirds = segment * 2 / barInc;
            bar[i] = pieces[thirds];            
        }
        if (showPercent)
        {
            var percent = $"{Math.Min(100, Math.Max(0, progress)) * 100:F0}%";
            var fill = Array.IndexOf(bar, pieces[0]);
            if (fill == -1) fill = barLength;
            if (fill + percent.Length < barLength - 1)
            {
                for (int i = 0; i < percent.Length; i++)
                {
                    bar[fill + i + 1] = percent[i];
                }
            }
            else
            {
                for (int i = 0; i < percent.Length; i++)
                {
                    bar[fill / 2 - percent.Length / 2 + i] = percent[i];
                }
            }
        }
        return $"[{new string(bar)}]";
    }
}
