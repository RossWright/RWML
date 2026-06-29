using RossWright.MetalNexus;

namespace RossWright.MetalGuardian.Authentication;

internal class MetalGuardianUrlHelper(
    IMetalGuardianAuthenticationClient client,
    IMetalNexusUrlHelper _urlHelper)
    : IMetalGuardianUrlHelper
{
    public string GetUrlFor<TRequest>(TRequest request, string? connectionName = null)
        where TRequest : new()
    {
        var authInfo = client.GetUser(connectionName);
        var url = _urlHelper.GetUrlFor<TRequest>(request);
        if (authInfo != null) url = url.WithQueryParameter("access_token", authInfo.Token);
        return url;
    }
}
