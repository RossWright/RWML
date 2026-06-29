namespace RossWright.MetalGuardian.Authentication;

internal class BaseAddressRepository : IBaseAddressRepository
{
    public IEnumerable<KeyValuePair<string, string?>> BaseUrlsByConnectionName => _baseAddresses;
    private readonly Dictionary<string, string?> _baseAddresses = new();

    public void Add(string connectionName, string? baseAddress, bool asDefault)
    {
        if (asDefault || !_baseAddresses.Any())
        {
            DefaultConnectionName = connectionName;
        }
        _baseAddresses[connectionName] = baseAddress;
    }

    public string DefaultConnectionName { get; private set; } = 
        Microsoft.Extensions.Options.Options.DefaultName;

    public string GetBaseAddress(string? connectionName = null) =>
        _baseAddresses.GetValueOrDefault(connectionName ?? DefaultConnectionName) 
        ?? string.Empty;
}
