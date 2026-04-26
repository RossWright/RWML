namespace RossWright.MetalCommand;

/// <summary>
/// Renders a progress value (0.0–1.0) as a fixed-width string for inline display in the console.
/// Implement this interface to supply a custom indicator to <see cref="ShowProgressConsoleExtensions.ShowProgress"/>.
/// </summary>
public interface IProgressIndicator
{
    /// <summary>The fixed character width of the rendered output.</summary>
    int Width { get; }

    /// <summary>
    /// Renders <paramref name="progress"/> as a string of exactly <see cref="Width"/> characters.
    /// </summary>
    /// <param name="progress">A value between 0.0 (no progress) and 1.0 (complete).</param>
    /// <returns>A fixed-width string representation of the progress.</returns>
    string Output(double progress);
}
