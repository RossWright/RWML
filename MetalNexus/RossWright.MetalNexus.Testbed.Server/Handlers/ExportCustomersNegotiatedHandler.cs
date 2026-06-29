using System.Text;
using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

/// <summary>
/// Returns customers as CSV or JSON depending on the inbound Accept header.
///
/// IMetalNexusRequestContext is injected automatically by AddMetalNexusServer.
/// It gives handlers read access to the inbound HTTP request headers without
/// coupling to HttpContext directly, keeping the handler testable.
/// AcceptHeader contains the raw value of the Accept header, or null if absent.
/// </summary>
internal class ExportCustomersNegotiatedHandler(
    InMemoryRepository repo,
    IMetalNexusRequestContext requestContext)
    : IRequestHandler<ExportCustomersNegotiatedRequest, IMetalNexusRawResponse>
{
    public Task<IMetalNexusRawResponse> Handle(
        ExportCustomersNegotiatedRequest request,
        CancellationToken cancellationToken)
    {
        var customers = repo.GetAllCustomers();
        var acceptsCsv = requestContext.AcceptHeader?.Contains("text/csv") == true;

        if (acceptsCsv)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Email,Phone,CreatedAt");
            foreach (var c in customers)
                sb.AppendLine($"{c.Id},{c.Name},{c.Email},{c.Phone},{c.CreatedAt:O}");

            return Task.FromResult<IMetalNexusRawResponse>(new NegotiatedCsvResponse
            {
                Data = Encoding.UTF8.GetBytes(sb.ToString())
            });
        }

        // Default: JSON — serialize manually so we stay inside IMetalNexusRawResponse
        var json = System.Text.Json.JsonSerializer.Serialize(
            customers.Select(c => CustomerMapping.ToDto(c)),
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

        return Task.FromResult<IMetalNexusRawResponse>(new NegotiatedJsonResponse
        {
            Data = Encoding.UTF8.GetBytes(json)
        });
    }

    private sealed class NegotiatedCsvResponse : IMetalNexusRawResponse
    {
        public string ContentType => "text/csv";
        public byte[]? Data { get; init; }
        public Stream? DataStream => null;
    }

    private sealed class NegotiatedJsonResponse : IMetalNexusRawResponse
    {
        public string ContentType => "application/json";
        public byte[]? Data { get; init; }
        public Stream? DataStream => null;
    }
}
