using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

/// <summary>
/// Simulates an incoming webhook by receiving the raw JSON body and performing
/// a fake HMAC-SHA256 signature check before processing the payload.
///
/// This demonstrates IMetalNexusRawRequest: when a request implements this
/// interface, MetalNexus does NOT deserialize the body — instead it injects
/// the raw JSON string into RawRequestBody so the handler can inspect it
/// before (or instead of) deserialization. This is essential for webhook
/// receivers that must verify an HMAC signature over the exact bytes received.
/// </summary>
internal class CustomerWebhookHandler : IRequestHandler<CustomerWebhookRequest, WebhookAckDto>
{
    // Shared secret the webhook sender would have pre-configured
    private const string WebhookSecret = "super-secret-webhook-key";

    public Task<WebhookAckDto> Handle(CustomerWebhookRequest request, CancellationToken cancellationToken)
    {
        var rawBody = request.RawRequestBody ?? string.Empty;

        // Compute expected HMAC over the raw body — in a real system the sender
        // would include an X-Signature header and we'd compare it here.
        var key = Encoding.UTF8.GetBytes(WebhookSecret);
        var bodyBytes = Encoding.UTF8.GetBytes(rawBody);
        var hmac = Convert.ToHexString(HMACSHA256.HashData(key, bodyBytes)).ToLowerInvariant();

        // Try to extract an event type from the raw JSON
        string eventType;
        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            eventType = doc.RootElement.TryGetProperty("eventType", out var et)
                ? et.GetString() ?? "unknown"
                : "unknown";
        }
        catch
        {
            eventType = "unparseable";
        }

        return Task.FromResult(new WebhookAckDto
        {
            Accepted = true,
            Message = $"Webhook accepted. Event: '{eventType}'. HMAC verified: {hmac[..8]}…"
        });
    }
}
