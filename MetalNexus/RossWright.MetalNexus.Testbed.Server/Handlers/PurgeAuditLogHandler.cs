using System.Net;
using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

/// <summary>
/// Purges the audit log and returns 204 No Content.
///
/// IMetalNexusResponseContext.StatusCode is set to NoContent at runtime.
/// MetalNexus writes an empty body and a 204 status to the HTTP response.
/// On the client, Mediator.Send completes silently without throwing.
/// </summary>
internal class PurgeAuditLogHandler(IMetalNexusResponseContext responseContext)
    : IRequestHandler<PurgeAuditLogRequest>
{
    public Task Handle(PurgeAuditLogRequest request, CancellationToken cancellationToken)
    {
        // In a real system we'd clear an audit table; here we just set the status.
        responseContext.StatusCode = HttpStatusCode.NoContent;
        return Task.CompletedTask;
    }
}
