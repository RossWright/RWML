namespace RossWright.MetalNexus;

public interface IMetalNexusServerOptionsBuilder : IMetalNexusOptionsBuilder
{
    void SetMultipartBodyLengthLimit(long? limitInBytes); //server-only
}

public static class MetalNexusServerOptionsBuilderExtensions
{
    public static void MakeEndpointsAnonymousByDefault(this IMetalNexusServerOptionsBuilder builder) =>
        builder.ConfigureEndpointSchema(_ => _.RequiresAuthenticationByDefault = false);
}