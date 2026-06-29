namespace RossWright.MetalGuardian;

internal class MetalGuardianTotpMfaUserClaimsProvider
    : IUserClaimsProvider
{
    public Task<IEnumerable<(string, string)>?> GetClaims(IAuthenticationUser user, CancellationToken cancellationToken)
    {
        var mfaUser = (ITotpMfaAuthenticationUser)user;
        return Task.FromResult<IEnumerable<(string, string)>?>(
        [
            (MetalGuardianTotpMfaClaimTypes.NeedsToSetupTotpMfaClaimType,
                (!mfaUser.IsMfaTotpEnabled && mfaUser.IsMfaTotpRequired).ToString()),
            (MetalGuardianTotpMfaClaimTypes.HasTotpMfaEnabledClaimType,
                mfaUser.IsMfaTotpEnabled.ToString())
        ]);
    }
}
