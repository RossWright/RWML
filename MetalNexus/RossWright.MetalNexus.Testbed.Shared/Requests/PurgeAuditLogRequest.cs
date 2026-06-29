using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Purges the audit log. Returns 204 No Content. Admin only.</summary>
[ApiRequest(HttpProtocol.PostViaBody)]
[Authenticated(UserRole.Admin)]
public class PurgeAuditLogRequest : IRequest { }
