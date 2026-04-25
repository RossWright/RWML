using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RossWright.MetalGuardian.Authentication;

internal interface IAccessTokenFactory
{
    string Create(IAuthenticationUser user, IEnumerable<(string, string)>? claims, int? expirationMins = null);
    bool Validate(string? accessToken);
}

internal class AccessTokenFactory : IAccessTokenFactory
{
    public AccessTokenFactory(IMetalGuardianServerConfiguration configuration)
    {
        var jwtIssuer = configuration.JwtIssuer;
        var jwtAudience = configuration.JwtAudience;
        var accessTokenExpireMins = configuration.JwtAccessTokenExpireMins;
        var issuerSigningKey = configuration.JwtIssuerSigningKey;

        if (string.IsNullOrEmpty(jwtIssuer) ||
            string.IsNullOrEmpty(jwtAudience) ||
            string.IsNullOrEmpty(issuerSigningKey) ||
            accessTokenExpireMins <= 0)
        {
            throw new MetalGuardianException(
                "Metal Guardian requires a config section called " +
                "MetalGuardian containing fields for " +
                (string.IsNullOrEmpty(jwtIssuer) ? "JwtIssuer, " : string.Empty) +
                (string.IsNullOrEmpty(jwtAudience) ? "JwtAudience, " : string.Empty) +
                (string.IsNullOrEmpty(issuerSigningKey) ? "JwtIssuerSigningKey, " : string.Empty) +
                "and, optionally, JwtAccessTokenExpireMins");
        }

        _jwtIssuer = jwtIssuer;
        _jwtAudience = jwtAudience;
        _issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey));
        _accessTokenExpireMins = accessTokenExpireMins;
        _relaxedTokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = false,
            ValidateLifetime = false,

            ValidIssuer = _jwtIssuer,
            ValidAudience = _jwtAudience,
            IssuerSigningKey = _issuerSigningKey,
            ClockSkew = TimeSpan.Zero,
        };
        StrictTokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = false,
            ValidateLifetime = true,

            ValidIssuer = _jwtIssuer,
            ValidAudience = _jwtAudience,
            IssuerSigningKey = _issuerSigningKey,
            ClockSkew = TimeSpan.Zero,
        };
    }

    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly SecurityKey _issuerSigningKey;
    private readonly int _accessTokenExpireMins;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();
    private readonly TokenValidationParameters _relaxedTokenValidationParameters;
    public TokenValidationParameters StrictTokenValidationParameters { get; }

    public string Create(IAuthenticationUser user, IEnumerable<(string, string)>? claims, int? expirationMins = null)
    {
        if (user.UserId == default) throw new ArgumentException(nameof(user.UserId));
        if (user.Name?.Any() == false) throw new ArgumentException(nameof(user.Name));
        var tokenClaims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()) };
        if (user.Name != null) tokenClaims.Add(new Claim(ClaimTypes.Name, user.Name));
        if (claims?.Any() == true) tokenClaims.AddRange(claims!.Select(_ => new Claim(_.Item1, _.Item2)));
        var expires = DateTime.UtcNow.AddMinutes(expirationMins ?? _accessTokenExpireMins);
        var jwt = new SigningCredentials(_issuerSigningKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_jwtIssuer, _jwtAudience, tokenClaims, null, expires, jwt);
        return _jwtSecurityTokenHandler.WriteToken(token);
    }

    public bool Validate(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken)) return false;
        SecurityToken? securityToken = null;
        try
        {
            _jwtSecurityTokenHandler.ValidateToken(accessToken,
                _relaxedTokenValidationParameters, out securityToken);
        }
        catch
        {
            return false;
        }
        var jwtSecurityToken = securityToken as JwtSecurityToken;
        return jwtSecurityToken?.Header.Alg.ToLower() == SecurityAlgorithms.HmacSha256.ToLower();
    }
}
