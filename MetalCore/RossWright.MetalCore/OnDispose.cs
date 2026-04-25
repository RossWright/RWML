namespace RossWright;

/// <summary>An <see cref="IDisposable"/> implementation that invokes a delegate when disposed.</summary>
public class OnDispose : IDisposable
{
    /// <summary>Initializes a new instance that calls <paramref name="onDispose"/> when disposed.</summary>
    /// <param name="onDispose">The action to run on disposal.</param>
    public OnDispose(Action onDispose) => _onDispose = onDispose;
    private Action? _onDispose;
    /// <summary>Invokes the registered action and prevents it from being called again.</summary>
    public void Dispose()
    {
        if (_onDispose != null)
        {
            _onDispose();
            _onDispose = null;
        }
    }
}

/// <summary>An <see cref="IAsyncDisposable"/> implementation that invokes an async delegate when disposed.</summary>
public class OnDisposeAsync : IAsyncDisposable
{
    /// <summary>Initializes a new instance that calls <paramref name="onDispose"/> when disposed.</summary>
    /// <param name="onDispose">The async function to run on disposal.</param>
    public OnDisposeAsync(Func<Task> onDispose) => _onDispose = onDispose;
    private Func<Task>? _onDispose;
    /// <summary>Invokes the registered async function and prevents it from being called again.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_onDispose != null)
        {
            await _onDispose();
            _onDispose = null;
        }
    }
}
