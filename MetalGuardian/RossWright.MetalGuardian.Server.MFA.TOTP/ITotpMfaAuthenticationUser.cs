namespace RossWright.MetalGuardian;

public interface ITotpMfaAuthenticationUser : IAuthenticationUser
{
    public string? MfaTotpSecret { get; set; }
    public bool IsMfaTotpEnabled { get; set; }
    bool IsMfaTotpRequired { get; }
}