using System.Diagnostics;
using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

/// <summary>
/// Demonstrates [HttpClientTimeout] on the request type.
/// The handler introduces an artificial delay. When ForceTimeout = true the
/// delay exceeds the 5-second client timeout, causing the client to cancel the
/// request with a TaskCanceledException or OperationCanceledException.
/// </summary>
internal class SlowReportHandler(InMemoryRepository repo)
    : IRequestHandler<SlowReportRequest, SlowReportResponse>
{
    public async Task<SlowReportResponse> Handle(SlowReportRequest request, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        // Normal delay: 1 second  |  forced timeout: 8 seconds (exceeds the 5 s client limit)
        var delayMs = request.ForceTimeout ? 8_000 : 1_000;
        await Task.Delay(delayMs, cancellationToken);
        sw.Stop();

        var customers = repo.GetAllCustomers();
        return new SlowReportResponse
        {
            CustomerCount = customers.Count,
            ElapsedSeconds = Math.Round(sw.Elapsed.TotalSeconds, 2)
        };
    }
}
