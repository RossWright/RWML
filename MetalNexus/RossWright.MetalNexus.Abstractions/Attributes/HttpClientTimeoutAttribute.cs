namespace RossWright.MetalNexus;

/// <summary>
/// Overrides the <see cref="HttpClient"/> send timeout for this specific endpoint.
/// </summary>
/// <remarks>
/// By default, MetalNexus uses the timeout configured on the underlying <see cref="HttpClient"/>
/// registration.  Apply this attribute to a request type to set a per-endpoint timeout that
/// takes precedence over the client-level default.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class HttpClientTimeoutAttribute : Attribute
{
    /// <summary>
    /// Initializes a new <see cref="HttpClientTimeoutAttribute"/>.
    /// </summary>
    /// <param name="timeoutSeconds">The maximum number of seconds to wait for a response before the request is cancelled.</param>
    public HttpClientTimeoutAttribute(int timeoutSeconds) => TimeoutSeconds = timeoutSeconds;
    /// <summary>The maximum number of seconds to wait for a response before the request is cancelled.</summary>
    public int TimeoutSeconds { get; }
}