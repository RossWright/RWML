using RossWright.MetalInjection;
using System.Net.Http.Headers;

namespace RossWright.MetalNexus.Testbed.Blazor;

/// <summary>Injects the current bearer token from <see cref="TokenService"/> into every outbound request.</summary>
[TransientService(typeof(AuthDelegatingHandler))]
public sealed class AuthDelegatingHandler(TokenService tokenService) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (tokenService.Token is { Length: > 0 } token)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
