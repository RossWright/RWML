using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

/// <summary>
/// Returns a stored document as a MetalNexusFile attachment.
///
/// MetalNexus detects the MetalNexusFile return type and writes the Data bytes
/// directly to the HTTP response with the declared ContentType and a
/// Content-Disposition: attachment header (because IsAttachment = true).
/// The client receives the raw bytes — no JSON wrapper.
/// </summary>
internal class DownloadCustomerDocumentHandler(InMemoryRepository repo)
    : IRequestHandler<DownloadCustomerDocumentRequest, MetalNexusFile>
{
    public Task<MetalNexusFile> Handle(DownloadCustomerDocumentRequest request, CancellationToken cancellationToken)
    {
        _ = repo.GetCustomer(request.CustomerId)
            ?? throw new CustomerNotFoundException(request.CustomerId);

        var doc = repo.GetDocument(request.CustomerId, request.DocumentId)
            ?? throw new ArgumentException($"Document {request.DocumentId} not found for customer {request.CustomerId}.");

        return Task.FromResult(new MetalNexusFile
        {
            FileName = doc.FileName,
            ContentType = doc.ContentType,
            Data = doc.Data,
            IsAttachment = true
        });
    }
}
