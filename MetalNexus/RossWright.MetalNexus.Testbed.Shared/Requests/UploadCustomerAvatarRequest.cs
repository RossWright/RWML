using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Uploads a single avatar image. Validates file type and size.</summary>
[ApiRequest(HttpProtocol.PostViaQuery)]
[Authenticated]
[UploadLimit(5 * 1024 * 1024)]
[AllowedFileTypes("image/jpeg", "image/png", "image/webp")]
[MaxFileSize(5 * 1024 * 1024)]
[ProducesError<CustomerNotFoundException>]
public class UploadCustomerAvatarRequest : MetalNexusFileRequest, IRequest<CustomerDto>
{
    public int CustomerId { get; set; }
}
