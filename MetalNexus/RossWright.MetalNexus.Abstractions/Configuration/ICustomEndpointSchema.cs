namespace RossWright.MetalNexus.Schemna;

public interface ICustomEndpointSchema
{
    string DeterminePath(Type requestType, string proposal);
    string DetermineTag(Type requestType, string proposal);
    HttpProtocol DetermineHttpProtocol(Type requestType, HttpProtocol proposal);
    bool DetermineRequiresAuthentication(Type requestType, bool proposal);
    string[]? DetermineAuthorizedRoles(Type requestType, string[]? proposal);
}
