using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

/// <summary>
/// Stores the first uploaded file as the customer's avatar.
///
/// The request derives from MetalNexusFileRequest, which tells MetalNexus to
/// decode the multipart form body and populate request.Files[]. The class-level
/// [AllowedFileTypes] and [MaxFileSize] attributes are validated automatically
/// before this handler is called — a ValidationException is thrown if violated.
/// [UploadLimit] overrides the server-wide multipart body size limit.
/// </summary>
internal class UploadCustomerAvatarHandler(InMemoryRepository repo)
    : IRequestHandler<UploadCustomerAvatarRequest, CustomerDto>
{
    public Task<CustomerDto> Handle(UploadCustomerAvatarRequest request, CancellationToken cancellationToken)
    {
        var customer = repo.GetCustomer(request.CustomerId)
            ?? throw new CustomerNotFoundException(request.CustomerId);

        if (request.Files is not { Length: > 0 })
            throw new ArgumentException("No file uploaded.");

        var file = request.Files[0];

        // Store as a document and update the avatar URL on the customer
        var doc = repo.AddDocument(request.CustomerId, file.FileName, file.ContentType, file.Data ?? [])!;
        repo.SetAvatarUrl(request.CustomerId, file.FileName);

        // Return the full updated customer DTO
        var updated = repo.GetCustomer(request.CustomerId)!;
        return Task.FromResult(CustomerMapping.ToDto(updated));
    }
}
