using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Deletes a customer. Admin-only.</summary>
[ApiRequest(HttpProtocol.Delete)]
[Authenticated(UserRole.Admin)]
[ProducesError<CustomerNotFoundException>]
public class DeleteCustomerRequest : IRequest
{
    public int Id { get; set; }
}
