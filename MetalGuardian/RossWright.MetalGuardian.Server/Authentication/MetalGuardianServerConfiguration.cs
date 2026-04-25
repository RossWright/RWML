namespace RossWright.MetalGuardian;

public interface IMetalGuardianServerConfiguration
{
    int JwtAccessTokenExpireMins { get; set; }
    string? JwtAudience { get; set; }
    string? JwtIssuer { get; set; }
    string? JwtIssuerSigningKey { get; set; }
    int? ProvisionalAccessTokenExpireMins { get; set; }
    int RefreshTokenExpireMins { get; set; }
}

public class MetalGuardianServerConfiguration : IMetalGuardianServerConfiguration
{
    public string? JwtIssuer { get; set; } = null!;
    public string? JwtAudience { get; set; } = null!;
    public string? JwtIssuerSigningKey { get; set; } = null!;
    public int JwtAccessTokenExpireMins { get; set; } = 60; // 1 hour
    public int RefreshTokenExpireMins { get; set; } = 24 * 60 * 60; // 60 days
    public int? ProvisionalAccessTokenExpireMins { get; set; } = 5; // 5 minutes
}
