using RossWright.MetalGuardian.Internal;
using System.Security.Claims;
using System.Text.Json;

namespace RossWright.MetalGuardian.Authentication;

internal class AccessToken : IAuthenticationInformation
{
    public AccessToken(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new MetalGuardianException("Invalid Access Token");
        }

        Token = accessToken;

        var payload = accessToken.Split('.')[1];

        //Parse Base64 Without Padding
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }
        var jsonBytes = Convert.FromBase64String(payload);

        _claims = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes)!;
        if (_claims is null)
        {
            throw new MetalGuardianException("Invalid Access Token");
        }

        if (!_claims.TryGetValue("exp", out object? exp) ||
            !_claims.TryGetValue(ClaimTypes.NameIdentifier, out object? userId))
        {
            throw new MetalGuardianException("Invalid Access Token");
        }
        ExpiresOn = UNIX_TIMESTAMP_ORIGIN.AddSeconds(long.Parse(exp!.ToString()!));
        UserId = Guid.TryParse(userId.ToString()!, out var guid) ? guid : Guid.Empty;
        UserName = _claims.GetValueOrDefault(ClaimTypes.Name)?.ToString();
        IsProvisional = ParseOrNull.Bool(_claims.GetValueOrDefault(MetalGuardianClaimTypes.ProvisionalLogin)?.ToString()) ?? false; 
        IsKnownDevice = ParseOrNull.Bool(_claims.GetValueOrDefault(MetalGuardianClaimTypes.IsKnownDevice)?.ToString());
    }
    private static readonly DateTimeOffset UNIX_TIMESTAMP_ORIGIN = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
    private Dictionary<string, object> _claims = null!;

    public string Token { get; }        
    public DateTimeOffset ExpiresOn { get; }        
    public Guid UserId { get; }
    public string? UserName { get; }
    public bool IsProvisional { get; }
    public bool? IsKnownDevice { get; }

    public string? GetAdditionalClaim(string claimType) =>
        _claims.GetValueOrDefault(claimType)?.ToString();

    public ClaimsIdentity AsClaimsIdentity() => new ClaimsIdentity(_claims
        .Select(_ => new Claim(_.Key, _.Value!.ToString()!)), "MetalGuardian");
}
