using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

/// <summary>
/// Stores all uploaded files as customer documents.
///
/// [MaxFileCount(5)] is validated automatically before the handler runs.
/// [NoUploadLimit] removes the server-wide multipart body size cap for this
/// specific endpoint, allowing larger batches.
/// </summary>
internal class UploadCustomerDocumentsHandler(InMemoryRepository repo)
    : IRequestHandler<UploadCustomerDocumentsRequest, CustomerDto>
{
    public Task<CustomerDto> Handle(UploadCustomerDocumentsRequest request, CancellationToken cancellationToken)
    {
        var customer = repo.GetCustomer(request.CustomerId)
            ?? throw new CustomerNotFoundException(request.CustomerId);

        foreach (var file in request.Files ?? [])
            repo.AddDocument(request.CustomerId, file.FileName, file.ContentType, file.Data ?? []);

        var updated = repo.GetCustomer(request.CustomerId)!;
        return Task.FromResult(CustomerMapping.ToDto(updated));
    }
}
