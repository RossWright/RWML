namespace RossWright.MetalNexus;

[AttributeUsage(AttributeTargets.Class)]
public class ApiRequestAttribute : Attribute
{
    public ApiRequestAttribute(
        HttpProtocol protocol = HttpProtocol.Auto,
        string? path = null,
        string? tag = null,
        string? connectionName = null)
    {
        Tag = tag;
        Path = path;
        HttpProtocol = protocol;
        ConnectionName = connectionName;
    }
    public string? Tag { get; }
    public string? Path { get; }
    public HttpProtocol HttpProtocol { get; }
    public string? ConnectionName { get; }
}

public enum HttpProtocol
{
    Auto,
    Get,
    PostViaBody,
    PostViaQuery,
    PutViaBody,
    PutViaQuery,
    PatchViaBody,
    PatchViaQuery,
    Delete
}

public static class HttpProtocolExtensions
{
    public static HttpMethod ToHttpMethod(this HttpProtocol protocol) => protocol switch
    {
        HttpProtocol.Get => HttpMethod.Get,
        HttpProtocol.PostViaBody or HttpProtocol.PostViaQuery => HttpMethod.Post,
        HttpProtocol.PutViaBody or HttpProtocol.PutViaQuery => HttpMethod.Put,
        HttpProtocol.PatchViaBody or HttpProtocol.PatchViaQuery => HttpMethod.Patch,
        HttpProtocol.Delete => HttpMethod.Delete,
        _ => throw new InvalidOperationException($"Cannot determine HTTP method for Auto")
    };

    public static bool UsesQueryParams(this HttpProtocol protocol) => protocol.In(
        HttpProtocol.Get, 
        HttpProtocol.PostViaQuery, 
        HttpProtocol.PutViaQuery,
        HttpProtocol.PatchViaQuery);
}