namespace RossWright.MetalNexus;

/// <summary>
/// Binds a request property to a named HTTP header rather than the query string or request body.
/// </summary>
/// <remarks>
/// <para>
/// On the <b>client</b>, MetalNexus serializes the property value as the specified header when
/// sending the request, making it useful for API keys, tenant identifiers, and similar
/// per-request metadata.
/// </para>
/// <para>
/// On the <b>server</b>, MetalNexus reads the property value from the incoming request header
/// of the same name before invoking the handler.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class FromHeaderAttribute(string headerName) : Attribute
{
    /// <summary>The HTTP header name to read from or write to.</summary>
    public string HeaderName => headerName;
}
