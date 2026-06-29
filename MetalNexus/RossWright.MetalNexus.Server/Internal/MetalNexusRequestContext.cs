using Microsoft.AspNetCore.Http;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Server;

/// <summary>
/// Ambient per-request implementation of <see cref="IMetalNexusRequestContext"/>.
/// Uses <see cref="AsyncLocal{T}"/> so every concurrent HTTP request has its own isolated
/// slot, matching the same mechanism used by <see cref="MetalNexusResponseContext"/>.
/// </summary>
internal sealed class MetalNexusRequestContext : IMetalNexusRequestContext
{
    private static readonly AsyncLocal<MetalNexusRequestContext?> _current = new();

    internal static MetalNexusRequestContext? Current => _current.Value;

    /// <summary>
    /// Populates the ambient context from the given <see cref="HttpRequest"/> and returns
    /// a handle that clears it on dispose.
    /// </summary>
    internal static IDisposable Begin(HttpRequest request)
    {
        _current.Value = new MetalNexusRequestContext(request);
        return new Handle();
    }

    private MetalNexusRequestContext(HttpRequest request)
    {
        AcceptHeader = request.Headers.Accept.ToString() is { Length: > 0 } a ? a : null;
        ContentType = request.Headers.ContentType.ToString() is { Length: > 0 } ct ? ct : null;
        RequestHeaders = request.Headers
            .ToDictionary(kvp => kvp.Key, kvp => (string?)kvp.Value.ToString());
    }

    /// <inheritdoc />
    public string? AcceptHeader { get; }

    /// <inheritdoc />
    public string? ContentType { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string?> RequestHeaders { get; }

    private sealed class Handle : IDisposable
    {
        public void Dispose() => _current.Value = null;
    }
}
