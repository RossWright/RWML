using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalGuardian.Authentication;

namespace RossWright.MetalGuardian;

public static class MetalGuardianExtensions
{
    public static IServiceCollection AddMetalGuardianClient(this IServiceCollection services,
        Action<IMetalGuardianClientOptionsBuilder> setOptions)
    {
        MetalGuardianClientOptionsBuilder optionsBuilder = new();
        setOptions(optionsBuilder);
        optionsBuilder.InitializeClient(services);
        return services;
    }

    public static IAuthenticationInformation DecodeAccessToken(this AuthenticationTokens authenticationTokens) =>
        new AccessToken(authenticationTokens.AccessToken);
}
