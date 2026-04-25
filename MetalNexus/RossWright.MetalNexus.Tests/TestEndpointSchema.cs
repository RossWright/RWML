using RossWright.MetalNexus.Schemna;

namespace RossWright.MetalNexus.Tests;

public class TestEndpointSchema : ICustomEndpointSchema
{
    public TestEndpointSchema()
    {
        OnDeterminePath = (_, proposal) => proposal;
        OnDetermineTag = (_, proposal) => proposal;
        OnDetermineHttpProtocol = (_, proposal) => proposal;
        OnDetermineRequiresAuthentication = (_, proposal) => proposal;
        OnDetermineAuthorizedRoles = (_, proposal) => proposal;
    }

    public string DeterminePath(Type requestType, string proposal) =>        
        OnDeterminePath(requestType, proposal);
    public Func<Type, string, string> OnDeterminePath { get; set; }

    public string DetermineTag(Type requestType, string proposal) => 
        OnDetermineTag(requestType, proposal);
    public Func<Type, string, string> OnDetermineTag { get; set; }

    public HttpProtocol DetermineHttpProtocol(Type requestType, HttpProtocol proposal) => 
        OnDetermineHttpProtocol(requestType, proposal);
    public Func<Type, HttpProtocol, HttpProtocol> OnDetermineHttpProtocol { get; set; }

    public bool DetermineRequiresAuthentication(Type requestType, bool proposal) => 
        OnDetermineRequiresAuthentication(requestType, proposal);
    public Func<Type, bool, bool> OnDetermineRequiresAuthentication { get; set; }

    public string[]? DetermineAuthorizedRoles(Type requestType, string[]? proposal) => 
        OnDetermineAuthorizedRoles(requestType, proposal);
    public Func<Type, string[]?, string[]?> OnDetermineAuthorizedRoles { get; set; }
}
