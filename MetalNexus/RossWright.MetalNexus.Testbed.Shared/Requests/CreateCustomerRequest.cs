using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>
/// Creates a new customer. Returns 201 Created (set via IMetalNexusResponseContext in the handler).
/// </summary>
[ApiRequest(HttpProtocol.PostViaBody)]
[Authenticated]
[ProducesError<DuplicateEmailException>]
public class CreateCustomerRequest : IRequest<CustomerDto>
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
}
