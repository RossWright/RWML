namespace RossWright.MetalGuardian;

/// <summary>
/// Shared configuration options available to all MetalGuardian client registration paths.
/// </summary>
public interface IMetalGuardianOptionsBuilder
{
    /// <summary>
    /// Configures the client to communicate with the built-in MetalNexus authentication endpoints
    /// (login, logout, refresh). Call this unless you are supplying a custom
    /// <see cref="IAuthenticationApiService"/> via <see cref="IMetalGuardianClientOptionsBuilder.UseAuthenticationApiService{TAuthenticationApiService}"/>.
    /// </summary>
    void UseMetalNexusAuthenticationEndpoints();

    /// <summary>
    /// Registers <see cref="IPasswordValidator"/> in the container, optionally customizing
    /// the active <see cref="PasswordRequirements"/> via <paramref name="configure"/>.
    /// </summary>
    void UsePasswordValidator(Action<PasswordRequirements>? configure = null);
}