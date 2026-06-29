namespace RossWright.MetalNexus.Testbed.Blazor.Components;

/// <summary>Represents a single entry in the tutorial log.</summary>
public sealed record LogEntry
{
    public int Number { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Feature { get; init; } = string.Empty;
    public string Narrative { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;
    public string Takeaway { get; init; } = string.Empty;
    public bool IsError { get; init; }
    public bool IsWarning { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.Now;
}
