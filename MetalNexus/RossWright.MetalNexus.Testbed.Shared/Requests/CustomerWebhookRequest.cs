using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Simulates a webhook with a raw request body for HMAC-style signature verification.</summary>
[ApiRequest(HttpProtocol.PostViaBody)]
[Anonymous]
public class CustomerWebhookRequest : IMetalNexusRawRequest<WebhookAckDto>
{
    public string? RawRequestBody { get; set; }
}

public class WebhookAckDto
{
    public bool Accepted { get; init; }
    public string Message { get; init; } = string.Empty;
}
