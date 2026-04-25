namespace RossWright.MetalNexus.Schemna;

public interface IEndpoint
{
    Type RequestType { get; }
    Type? ResponseType { get; }

    string? HttpClientName { get; }
    HttpMethod HttpMethod { get; }
    string Path { get; }
    bool RequestAsQueryParams { get; }
    bool HasPathParams { get; }

    string? Tag { get; }

    bool RequiresAuthentication { get; }
    string[]? AuthorizedRoles { get; }
    bool AllowProvisional { get; }

    TimeSpan? HttpClientTimeout { get; }
    string[] HeaderProperties { get; }
}

