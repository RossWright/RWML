using RossWright.MetalGuardian;

namespace RossWright.MetalGuardian.Server.Tests.AuthenticationServiceTests;

// ─── User / RefreshToken models ─────────────────────────────────────────────

internal record FakeUser : IAuthenticationUser
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "TestUser";
    public bool IsDisabled { get; set; }
    public string PasswordSalt { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public static FakeUser WithPassword(string password)
    {
        var user = new FakeUser();
        user.SetPassword(password);
        return user;
    }
}

internal class FakeRefreshToken : IRefreshToken
{
    public Guid UserId { get; set; }
    public IAuthenticationUser User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public DateTime LastSeen { get; set; }
}

// ─── Repository fakes ────────────────────────────────────────────────────────

internal class FakeAuthenticationRepository : IAuthenticationRepository
{
    private readonly List<FakeUser> _users = [];
    private readonly List<FakeRefreshToken> _refreshTokens = [];

    public void AddUser(FakeUser user) => _users.Add(user);

    public IReadOnlyList<FakeRefreshToken> RefreshTokens => _refreshTokens;

    public Task<IAuthenticationUser?> LookupUser(string userIdentity, CancellationToken cancellationToken)
    {
        var user = _users.FirstOrDefault(u => u.Name == userIdentity);
        return Task.FromResult<IAuthenticationUser?>(user);
    }

    public Task<IAuthenticationUser?> UpdateUser(Guid userId, Func<IAuthenticationUser, bool> update, CancellationToken cancellationToken)
    {
        var user = _users.FirstOrDefault(u => u.UserId == userId);
        if (user != null) update(user);
        return Task.FromResult<IAuthenticationUser?>(user);
    }

    public Task AddRefreshToken(Action<IRefreshToken> setProperties, CancellationToken cancellationToken)
    {
        var token = new FakeRefreshToken();
        setProperties(token);
        _refreshTokens.Add(token);
        return Task.CompletedTask;
    }

    public Task<IAuthenticationUser?> UpdateRefreshToken(Guid userId, string refreshToken,
        Action<IRefreshToken> setProperties, CancellationToken cancellationToken)
    {
        var rt = _refreshTokens.FirstOrDefault(t => t.UserId == userId && t.Token == refreshToken);
        if (rt == null) return Task.FromResult<IAuthenticationUser?>(null);
        var user = _users.FirstOrDefault(u => u.UserId == userId);
        if (user != null) rt.User = user;
        setProperties(rt);
        return Task.FromResult<IAuthenticationUser?>(user);
    }

    public Task DeleteRefreshToken(Guid userId, string refreshToken, CancellationToken cancellationToken)
    {
        _refreshTokens.RemoveAll(t => t.UserId == userId && t.Token == refreshToken);
        return Task.CompletedTask;
    }
}

internal class FakeUserDeviceRepository : IUserDeviceRepository
{
    private readonly List<FakeUserDevice> _devices = [];

    public void AddDevice(FakeUserDevice device) => _devices.Add(device);

    public Task Add(Action<IUserDevice> setProperties, CancellationToken cancellationToken)
    {
        var device = new FakeUserDevice();
        setProperties(device);
        _devices.Add(device);
        return Task.CompletedTask;
    }

    public Task<IUserDevice?> Get(Guid userId, string deviceFingerprint, CancellationToken cancellationToken)
    {
        var device = _devices.FirstOrDefault(d => d.UserId == userId && d.Fingerprint == deviceFingerprint);
        return Task.FromResult<IUserDevice?>(device);
    }

    public Task Update(Guid userId, string deviceFingerprint, Action<IUserDevice> setProperties, CancellationToken cancellationToken)
    {
        var device = _devices.FirstOrDefault(d => d.UserId == userId && d.Fingerprint == deviceFingerprint);
        if (device != null) setProperties(device);
        return Task.CompletedTask;
    }
}

internal class FakeUserDevice : IUserDevice
{
    public Guid UserId { get; set; }
    public IAuthenticationUser User { get; set; } = null!;
    public string Fingerprint { get; set; } = string.Empty;
    public DateTime? ExpiresOn { get; set; }
    public DateTime LastSeen { get; set; }
}

// ─── MFA / Claims providers ──────────────────────────────────────────────────

internal class FakeMfaProvider : IMultifactorAuthenticationProvider
{
    private readonly bool _provisional;
    public FakeMfaProvider(bool provisional) => _provisional = provisional;
    public bool ShouldLoginAsProvisional(IAuthenticationUser user, bool? isKnownDevice) => _provisional;
}

/// <summary>MFA provider that requires provisional unless device is known.</summary>
internal class FakeKnownDeviceMfaProvider : IMultifactorAuthenticationProvider
{
    public bool ShouldLoginAsProvisional(IAuthenticationUser user, bool? isKnownDevice) => isKnownDevice != true;
}

internal class FakeUserClaimsProvider : IUserClaimsProvider
{
    public List<(string, string)> Claims { get; } = [];
    public Task<IEnumerable<(string, string)>?> GetClaims(IAuthenticationUser user, CancellationToken cancellationToken)
        => Task.FromResult<IEnumerable<(string, string)>?>(Claims.Count > 0 ? Claims : null);
}
