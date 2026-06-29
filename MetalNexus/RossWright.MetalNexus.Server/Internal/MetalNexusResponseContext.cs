using System.Net;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Server;

/// <summary>
/// Ambient per-request implementation of <see cref="IMetalNexusResponseContext"/>.
/// Uses <see cref="AsyncLocal{T}"/> so every concurrent HTTP request has its own isolated slot,
/// matching exactly the mechanism used by <c>IHttpContextAccessor</c> internally.
/// </summary>
internal sealed class MetalNexusResponseContext : IMetalNexusResponseContext
{
    private static readonly AsyncLocal<MetalNexusResponseContext?> _current = new();

    /// <summary>Returns the ambient context for the current async execution context, or <c>null</c>.</summary>
    internal static MetalNexusResponseContext? Current => _current.Value;

    /// <summary>
    /// Sets up an ambient <see cref="MetalNexusResponseContext"/> for the current async
    /// execution context.  Dispose the returned handle to clear the ambient value.
    /// </summary>
    internal static IDisposable Begin()
    {
        var ctx = new MetalNexusResponseContext();
        _current.Value = ctx;
        return new Handle();
    }

    private HttpStatusCode _statusCode = HttpStatusCode.OK;

    /// <inheritdoc />
    public HttpStatusCode StatusCode
    {
        get => _statusCode;
        set
        {
            _statusCode = value;
            IsStatusCodeSet = true;
        }
    }

    /// <summary>True when the handler explicitly assigned <see cref="StatusCode"/>.</summary>
    internal bool IsStatusCodeSet { get; private set; }

    /// <inheritdoc />
    public string? Location { get; set; }

    private sealed class Handle : IDisposable
    {
        public void Dispose() => _current.Value = null;
    }
}
