namespace RossWright.MetalNexus;

/// <summary>
/// Configuration builder for the MetalNexus server, extending the shared
/// <see cref="IMetalNexusOptionsBuilder"/> with server-only settings.
/// </summary>
public interface IMetalNexusServerOptionsBuilder : IMetalNexusOptionsBuilder
{
    /// <summary>
    /// Overrides the ASP.NET Core multipart body length limit for all file-upload endpoints.
    /// Individual endpoints can further override this via <see cref="UploadLimitAttribute"/> or
    /// <see cref="NoUploadLimitAttribute"/>.
    /// </summary>
    /// <param name="limitInBytes">
    /// The maximum allowed multipart body size in bytes, or <c>null</c> to remove the limit
    /// entirely (equivalent to applying <see cref="NoUploadLimitAttribute"/> globally).
    /// </param>
    void SetMultipartBodyLengthLimit(long? limitInBytes);
}

/// <summary>Convenience extensions on <see cref="IMetalNexusServerOptionsBuilder"/>.</summary>
public static class MetalNexusServerOptionsBuilderExtensions
{
    /// <summary>
    /// Configures all endpoints to be anonymous by default, so authentication must be
    /// explicitly required via <see cref="AuthenticatedAttribute"/> per endpoint.
    /// </summary>
    /// <param name="builder">The server options builder to configure.</param>
    public static void MakeEndpointsAnonymousByDefault(this IMetalNexusServerOptionsBuilder builder) =>
        builder.ConfigureEndpointSchema(_ => _.RequiresAuthenticationByDefault = false);
}