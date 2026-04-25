namespace RossWright.MetalCommand;

public interface IProgressIndicator
{
    int Width { get; }
    string Output(double progress);
}
