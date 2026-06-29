using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Demonstrates per-request client timeout. Handler has an artificial delay.</summary>
[ApiRequest(HttpProtocol.PostViaBody)]
[Authenticated]
[HttpClientTimeout(5)]
public class SlowReportRequest : IRequest<SlowReportResponse>
{
    /// <summary>When true, the server delays longer than the client timeout to demonstrate cancellation.</summary>
    public bool ForceTimeout { get; set; }
}

public class SlowReportResponse
{
    public int CustomerCount { get; init; }
    public double ElapsedSeconds { get; init; }
}
