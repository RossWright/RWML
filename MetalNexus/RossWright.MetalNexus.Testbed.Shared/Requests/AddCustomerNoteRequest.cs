using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Adds a note to a customer. Demonstrates PATCH method.</summary>
[ApiRequest(HttpProtocol.PatchViaBody)]
[Authenticated]
[ProducesError<CustomerNotFoundException>]
public class AddCustomerNoteRequest : IRequest<CustomerNoteDto>
{
    public int CustomerId { get; set; }
    public string Text { get; set; } = null!;
}
