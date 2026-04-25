namespace RossWright.MetalGuardian;

public record AuthenticationTokens
{
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
}
