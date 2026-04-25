namespace RossWright.MetalGuardian;

public interface IMetalGuardianUrlHelper
{
    string GetUrlFor<TRequest>(TRequest request, string? connectionName = null)
        where TRequest : new();
}
