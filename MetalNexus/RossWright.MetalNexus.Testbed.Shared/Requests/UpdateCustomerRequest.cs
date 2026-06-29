using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Updates an existing customer. Sets Location header via IMetalNexusResponseContext.</summary>
[ApiRequest(HttpProtocol.PutViaBody)]
[Authenticated(UserRole.Admin, UserRole.Manager)]
[ProducesError<CustomerNotFoundException>]
[ProducesError<DuplicateEmailException>]
public class UpdateCustomerRequest : IRequest<CustomerDto>
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
}
