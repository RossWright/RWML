namespace RossWright.MetalGuardian;

public interface IMultifactorAuthenticationProvider
{
    /// <summary>
    /// When true, the user will given a provisional access token on login,
    /// A real access token will only be issued once MFA is verfied.
    /// </summary>
    bool ShouldLoginAsProvisional(IAuthenticationUser user, bool? isKnownDevice);
}
