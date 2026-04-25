namespace RossWright;

/// <summary>
/// Prevents concurrent or duplicate async loads for the same keyed resource.
/// Only one load function will execute at a time per key; subsequent callers
/// receive the same in-flight task. Supports optional cache expiry via
/// <see cref="ReloadAfterSeconds"/>.
/// </summary>
public class LoadGuard
{
    private readonly Dictionary<string, Task> _loadingTasks = new();
    private readonly object _lock = new();
    private DateTime _lastLoadTime = DateTime.MinValue;

    /// <summary>
    /// When set, the guard allows a reload after this many seconds have elapsed
    /// since the last completed load. When <see langword="null"/>, the resource
    /// is loaded at most once.
    /// </summary>
    public int? ReloadAfterSeconds { get; set; }

    /// <summary>
    /// Executes <paramref name="loadFunc"/> for the given <paramref name="key"/>,
    /// or returns an already in-flight task if one is running. If
    /// <see cref="ReloadAfterSeconds"/> is set and has not yet elapsed, returns
    /// <see cref="Task.CompletedTask"/> immediately.
    /// </summary>
    /// <param name="key">A string key identifying the resource to load.</param>
    /// <param name="loadFunc">
    /// The async function that performs the load. Called at most once at a time.
    /// </param>
    /// <returns>
    /// The in-flight or newly started <see cref="Task"/> for the load operation.
    /// </returns>
    public Task Load(string key, Func<Task> loadFunc)
    {
        lock (_lock)
        {
            var loadingTask = _loadingTasks.GetValueOrDefault(key);
            if (loadingTask != null && !loadingTask.IsCompleted) return loadingTask;

            // Prune the completed entry so it doesn't accumulate indefinitely
            if (loadingTask != null) _loadingTasks.Remove(key);

            if (ReloadAfterSeconds.HasValue &&
                (DateTime.UtcNow - _lastLoadTime).TotalSeconds < ReloadAfterSeconds)
            {
                return Task.CompletedTask;
            }
            loadingTask = loadFunc();
            _loadingTasks[key] = loadingTask;
            _lastLoadTime = DateTime.UtcNow;
            return loadingTask;
        }
    }
}
