namespace RossWright.MetalShout;

internal interface IPushServiceConnector
{
    Task Connect(string? connectionName = null, CancellationToken cancellationToken = default);
}
