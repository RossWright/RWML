namespace RossWright.MetalGuardian;

public interface IBaseAddressRepository
{
    string DefaultConnectionName { get; }
    public string GetBaseAddress(string? connectionName = null);
}
