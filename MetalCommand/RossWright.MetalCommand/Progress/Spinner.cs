namespace RossWright.MetalCommand;

public class Spinner : IProgressIndicator
{
    private const string spin = "\\|/-";
    
    public int Width => 1;

    private int s = 0;
    public string Output(double progress) => spin[s++ % spin.Length].ToString();
}
