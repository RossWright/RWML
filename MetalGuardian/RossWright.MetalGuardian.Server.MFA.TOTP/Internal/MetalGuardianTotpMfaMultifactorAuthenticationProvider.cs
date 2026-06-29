namespace RossWright.MetalGuardian;

internal class MetalGuardianTotpMfaMultifactorAuthenticationProvider
    : IMultifactorAuthenticationProvider
{
    public bool ShouldLoginAsProvisional(IAuthenticationUser user, bool? isKnownDevice)
    {
        var mfaUser = (ITotpMfaAuthenticationUser)user;
        return (mfaUser.IsMfaTotpRequired && !mfaUser.IsMfaTotpEnabled) || 
            (isKnownDevice != true && mfaUser.IsMfaTotpEnabled);
    }
}
