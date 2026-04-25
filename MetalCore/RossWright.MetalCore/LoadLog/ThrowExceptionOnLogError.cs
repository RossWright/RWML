namespace RossWright;

/// <summary>
/// An <see cref="ILoadLog"/> wrapper that throws a <see cref="MetalCoreException"/>
/// when an error (or optionally a warning) is logged. Optionally forwards all
/// messages to an inner <see cref="ILoadLog"/>.
/// </summary>
public class ThrowExceptionOnLogError : ILoadLog
{
    /// <summary>
    /// Initializes a new <see cref="ThrowExceptionOnLogError"/> with no inner log.
    /// </summary>
    /// <param name="throwOnWarn">
    /// When <see langword="true"/>, warnings also cause a
    /// <see cref="MetalCoreException"/> to be thrown.
    /// </param>
    public ThrowExceptionOnLogError(bool throwOnWarn = false) =>
        (_log, _throwOnWarn) = (null, throwOnWarn);

    /// <summary>
    /// Initializes a new <see cref="ThrowExceptionOnLogError"/> that wraps an
    /// existing <see cref="ILoadLog"/>.
    /// </summary>
    /// <param name="log">The inner log to forward messages to.</param>
    /// <param name="throwOnWarn">
    /// When <see langword="true"/>, warnings also cause a
    /// <see cref="MetalCoreException"/> to be thrown.
    /// </param>
    public ThrowExceptionOnLogError(ILoadLog log, bool throwOnWarn = false) =>
        (_log, _throwOnWarn) = (log, throwOnWarn);
    readonly ILoadLog? _log;
    readonly bool _throwOnWarn;

    /// <inheritdoc/>
    public IDisposable BeginScope() =>
        _log?.BeginScope() ?? new OnDispose(() => { });

    /// <inheritdoc/>
    /// <exception cref="MetalCoreException">
    /// Thrown when <paramref name="level"/> is <see cref="LogLevel.Error"/>, or
    /// when <paramref name="level"/> is <see cref="LogLevel.Warning"/> and
    /// <c>throwOnWarn</c> was set to <see langword="true"/>.
    /// </exception>
    public void Log(LogLevel level, string message)
    {
        if (level == LogLevel.Error 
            || (level == LogLevel.Warning && _throwOnWarn))
        {
            throw new MetalCoreException(message);
        }
        _log?.Log(level, message);
    }

    /// <inheritdoc/>
    public string? ModuleName
    {
        get => _log?.ModuleName;
        set { if (_log != null) _log.ModuleName = value; }
    }
}