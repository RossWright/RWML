namespace RossWright.MetalGuardian;

/// <summary>
/// Configuration contract for MetalGuardian JWT and token settings.
/// </summary>
public interface IMetalGuardianServerConfiguration
{
    /// <summary>
    /// Lifetime of issued access tokens, in minutes. Default is 60 (1 hour).
    /// </summary>
    int JwtAccessTokenExpireMins { get; set; }

    /// <summary>
    /// Expected audience value embedded in and validated against JWTs.
    /// </summary>
    string? JwtAudience { get; set; }

    /// <summary>
    /// Expected issuer value embedded in and validated against JWTs.
    /// </summary>
    string? JwtIssuer { get; set; }

    /// <summary>
    /// Symmetric key used to sign and verify JWTs.
    /// </summary>
    string? JwtIssuerSigningKey { get; set; }

    /// <summary>
    /// Lifetime of provisional (MFA-pending) access tokens, in minutes.
    /// Default is 5. Set to <c>null</c> to use <see cref="JwtAccessTokenExpireMins"/>.
    /// </summary>
    int? ProvisionalAccessTokenExpireMins { get; set; }

    /// <summary>
    /// Lifetime of refresh tokens, in minutes. Default is 86,400 (24 × 60 × 60 = 60 days).
    /// </summary>
    int RefreshTokenExpireMins { get; set; }
}

/// <summary>
/// Default implementation of <see cref="IMetalGuardianServerConfiguration"/>.
/// Populate via <c>appsettings.json</c> or call
/// <see cref="IMetalGuardianServerOptionBuilder.UseJwtConfiguration"/> to supply a
/// custom instance.
/// </summary>
public class MetalGuardianServerConfiguration : IMetalGuardianServerConfiguration
{
    /// <inheritdoc />
    public string? JwtIssuer { get; set; } = null!;
    /// <inheritdoc />
    public string? JwtAudience { get; set; } = null!;
    /// <inheritdoc />
    public string? JwtIssuerSigningKey { get; set; } = null!;
    /// <inheritdoc />
    public int JwtAccessTokenExpireMins { get; set; } = 60; // 1 hour
    /// <inheritdoc />
    public int RefreshTokenExpireMins { get; set; } = 24 * 60 * 60; // 60 days
    /// <inheritdoc />
    public int? ProvisionalAccessTokenExpireMins { get; set; } = 5; // 5 minutes
}
