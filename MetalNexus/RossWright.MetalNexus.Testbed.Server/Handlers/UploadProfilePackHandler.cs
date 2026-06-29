using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

/// <summary>
/// Stores named file slots: an avatar image (slot "avatar") and an optional
/// PDF document (slot "document").
///
/// The [FileSlot] attribute on each property tells MetalNexus to route the
/// uploaded multipart field with that name to that property, instead of
/// collecting it into the anonymous Files[] array. Per-slot [AllowedFileTypes]
/// and [MaxFileSize] on the "document" property override the class-level values
/// just for that slot.
/// </summary>
internal class UploadProfilePackHandler(InMemoryRepository repo)
    : IRequestHandler<UploadProfilePackRequest, CustomerDto>
{
    public Task<CustomerDto> Handle(UploadProfilePackRequest request, CancellationToken cancellationToken)
    {
        _ = repo.GetCustomer(request.CustomerId)
            ?? throw new CustomerNotFoundException(request.CustomerId);

        if (request.Avatar is not null)
        {
            repo.AddDocument(request.CustomerId, request.Avatar.FileName, request.Avatar.ContentType, request.Avatar.Data ?? []);
            repo.SetAvatarUrl(request.CustomerId, request.Avatar.FileName);
        }

        if (request.Document is not null)
            repo.AddDocument(request.CustomerId, request.Document.FileName, request.Document.ContentType, request.Document.Data ?? []);

        var updated = repo.GetCustomer(request.CustomerId)!;
        return Task.FromResult(CustomerMapping.ToDto(updated));
    }
}
