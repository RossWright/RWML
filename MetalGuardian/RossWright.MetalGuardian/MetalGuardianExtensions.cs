using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalGuardian.Authentication;

namespace RossWright.MetalGuardian;

/// <summary>
/// Extension methods for registering MetalGuardian client services.
/// </summary>
public static class MetalGuardianExtensions
{
    /// <summary>
    /// Registers MetalGuardian client services with the dependency injection container.
    /// Use the <paramref name="setOptions"/> callback to configure authentication endpoints,
    /// password validation, device fingerprinting, and authenticated HTTP clients.
    /// </summary>
    public static IServiceCollection AddMetalGuardianClient(this IServiceCollection services,
        Action<IMetalGuardianClientOptionsBuilder> setOptions)
    {
        MetalGuardianClientOptionsBuilder optionsBuilder = new();
        setOptions(optionsBuilder);
        optionsBuilder.InitializeClient(services);
        return services;
    }

    /// <summary>
    /// Decodes an <see cref="AuthenticationTokens"/> access token into an
    /// <see cref="IAuthenticationInformation"/> instance, or <c>null</c> if the
    /// access token is absent or malformed.
    /// </summary>
    public static IAuthenticationInformation? DecodeAccessToken(this AuthenticationTokens authenticationTokens) =>
        AccessToken.TryCreate(authenticationTokens.AccessToken);
}
