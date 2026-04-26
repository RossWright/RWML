namespace RossWright.MetalCommand;

/// <summary>
/// An <see cref="IProgressIndicator"/> that renders a plain percentage string such as <c>" 42%"</c>.
/// Always 4 characters wide.
/// </summary>
public class Percentage : IProgressIndicator
{
    /// <inheritdoc/>
    public int Width => 4;

    /// <inheritdoc/>
    public string Output(double progress) => 
        $"{Math.Min(1, Math.Max(0, progress)) * 100,3:F0}%";
}
