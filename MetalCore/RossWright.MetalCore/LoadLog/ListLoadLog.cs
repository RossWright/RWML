namespace RossWright;

/// <summary>
/// An <see cref="ILoadLog"/> implementation that buffers all log entries in
/// memory. Useful for capturing and asserting on startup diagnostics in tests.
/// </summary>
public class ListLoadLog : ILoadLog
{
    private int _scopeLevel = 0;

    /// <inheritdoc/>
    public string? ModuleName { get; set; }

    /// <summary>All entries recorded since this instance was created.</summary>
    public List<Entry> Entries { get; } = new();

    /// <inheritdoc/>
    public IDisposable BeginScope()
    {
        _scopeLevel++;
        return new OnDispose(() => _scopeLevel--);
    }

    /// <inheritdoc/>
    public void Log(LogLevel level, string message) => 
        Entries.Add(new Entry { ScopeLevel = _scopeLevel, Level = level, Message = message });

    /// <summary>A single captured log entry.</summary>
    public class Entry
    {
        /// <summary>The nesting depth at which this entry was logged.</summary>
        public int ScopeLevel { get; set; }

        /// <summary>The severity level of this entry.</summary>
        public LogLevel Level { get; set; }

        /// <summary>The log message text.</summary>
        public string Message { get; set; } = null!;
    }
}