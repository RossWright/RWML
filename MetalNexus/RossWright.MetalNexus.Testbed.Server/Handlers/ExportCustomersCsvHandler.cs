using System.Text;
using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

/// <summary>
/// Returns all customers as a raw CSV download.
///
/// CustomersCsvResponse implements IMetalNexusRawResponse, which signals to
/// MetalNexus that it should write the Data bytes verbatim to the HTTP response
/// body instead of JSON-serializing the object. The ContentType property drives
/// the response Content-Type header.
/// </summary>
internal class ExportCustomersCsvHandler(InMemoryRepository repo)
    : IRequestHandler<ExportCustomersCsvRequest, CustomersCsvResponse>
{
    public Task<CustomersCsvResponse> Handle(ExportCustomersCsvRequest request, CancellationToken cancellationToken)
    {
        var customers = repo.GetAllCustomers();
        var csv = BuildCsv(customers);
        return Task.FromResult(new CustomersCsvResponse
        {
            Data = Encoding.UTF8.GetBytes(csv)
        });
    }

    private static string BuildCsv(IEnumerable<Customer> customers)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Name,Email,Phone,CreatedAt");
        foreach (var c in customers)
            sb.AppendLine($"{c.Id},{Escape(c.Name)},{Escape(c.Email)},{Escape(c.Phone)},{c.CreatedAt:O}");
        return sb.ToString();
    }

    private static string Escape(string value) =>
        value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}
