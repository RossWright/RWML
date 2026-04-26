namespace RossWright.MetalCommand;

/// <summary>
/// An <see cref="IProgressIndicator"/> that renders a rotating spinner character
/// (<c>\</c>, <c>|</c>, <c>/</c>, <c>-</c>). Always 1 character wide.
/// </summary>
public class Spinner : IProgressIndicator
{
    private const string spin = "\\|/-";

    /// <inheritdoc/>
    public int Width => 1;

    /// <inheritdoc/>
    private int s = 0;
    public string Output(double progress) => spin[s++ % spin.Length].ToString();
}
