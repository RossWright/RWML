using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Uploads up to 5 documents for a customer.</summary>
[ApiRequest(HttpProtocol.PostViaQuery)]
[Authenticated]
[MaxFileCount(5)]
[MaxFileSize(10 * 1024 * 1024)]
[NoUploadLimit]
[ProducesError<CustomerNotFoundException>]
public class UploadCustomerDocumentsRequest : MetalNexusFileRequest, IRequest<CustomerDto>
{
    public int CustomerId { get; set; }
}
