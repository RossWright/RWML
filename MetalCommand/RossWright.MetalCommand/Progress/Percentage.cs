namespace RossWright.MetalCommand;

public class Percentage : IProgressIndicator
{
    public int Width => 4;
    public string Output(double progress) => 
        $"{Math.Min(100, Math.Max(0, progress)) * 100,3:F0}%";
}
