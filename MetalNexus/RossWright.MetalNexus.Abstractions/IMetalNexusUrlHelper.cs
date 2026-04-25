namespace RossWright.MetalNexus;

public interface IMetalNexusUrlHelper
{
    string GetUrlFor<TRequest>(TRequest request)
        where TRequest : new();
}