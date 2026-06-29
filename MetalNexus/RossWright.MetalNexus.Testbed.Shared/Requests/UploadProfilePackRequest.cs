using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Uploads named file slots: an avatar image and an optional PDF document.</summary>
[ApiRequest(HttpProtocol.PostViaQuery)]
[Authenticated]
[AllowedFileTypes("image/jpeg", "image/png")]
[MaxFileSize(2 * 1024 * 1024)]
[ProducesError<CustomerNotFoundException>]
public class UploadProfilePackRequest : MetalNexusFileRequest, IRequest<CustomerDto>
{
    public int CustomerId { get; set; }

    [FileSlot("avatar")]
    public MetalNexusFile? Avatar { get; set; }

    [FileSlot("document")]
    [AllowedFileTypes("application/pdf")]
    [MaxFileSize(10 * 1024 * 1024)]
    public MetalNexusFile? Document { get; set; }
}
