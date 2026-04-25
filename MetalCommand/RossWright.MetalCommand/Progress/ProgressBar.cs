namespace RossWright.MetalCommand;

public class ProgressBar : IProgressIndicator
{
    public ProgressBar(bool showPercent = true, int length = 52) => barLength = length - 2;

    private const string pieces = " \u258C\u2588";
    public int Width => barLength + 2;
    private int barLength = 50;
    bool showPercent = true;
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
