namespace RossWright.MetalNexus;

/// <summary>
/// Well-known constants for the MetalNexus wire protocol.
/// </summary>
public static class MetalNexusConstants
{
    /// <summary>
    /// Request header sent by the MetalNexus client on every outgoing request.
    /// When the server sees this header it returns error responses using the MetalNexus
    /// JSON envelope, enabling typed exception reconstruction on the client.
    /// When absent the server returns RFC 7807 <c>ProblemDetails</c> instead.
    /// Non-MetalNexus clients that want to opt into the MetalNexus error format can
    /// include this header manually.
    /// </summary>
    public const string ClientHeader = "X-MetalNexus-Client";

    /// <summary>The value sent with <see cref="ClientHeader"/>.</summary>
    public const string ClientHeaderValue = "1";
}
