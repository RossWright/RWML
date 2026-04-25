namespace RossWright.MetalNexus.Schemna;

public interface IMetalNexusRegistry
{
    IEnumerable<IEndpoint> Endpoints { get; }
    IEndpoint? FindEndpoint(Type requestType);
    void AddEndpoints(params Type[] types);
}