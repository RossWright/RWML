using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Returns all customers as a raw CSV download.</summary>
[ApiRequest]
[Anonymous]
public class ExportCustomersCsvRequest : IRequest<CustomersCsvResponse> { }

public sealed class CustomersCsvResponse : IMetalNexusRawResponse
{
    public string ContentType => "text/csv";
    public byte[]? Data { get; init; }
    public Stream? DataStream => null;
}
