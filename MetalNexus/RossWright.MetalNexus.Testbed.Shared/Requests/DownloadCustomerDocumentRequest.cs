using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Downloads a stored document as a file attachment.</summary>
[ApiRequest]
[Authenticated]
[ProducesError<CustomerNotFoundException>]
public class DownloadCustomerDocumentRequest : IRequest<MetalNexusFile>
{
    public int CustomerId { get; set; }
    public int DocumentId { get; set; }
}
