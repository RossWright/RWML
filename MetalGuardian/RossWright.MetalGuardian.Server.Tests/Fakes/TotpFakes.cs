namespace RossWright.MetalGuardian.Server.Tests.Fakes;

// ─── TOTP user record ────────────────────────────────────────────────────────

internal class FakeTotpUser : IAuthenticationUser, ITotpMfaAuthenticationUser
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "TotpUser";
    public bool IsDisabled { get; set; }
    public string PasswordSalt { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? MfaTotpSecret { get; set; }
    public bool IsMfaTotpEnabled { get; set; }
    public bool IsMfaTotpRequired { get; set; }
}

// ─── TOTP authentication repository fake ────────────────────────────────────

internal class FakeTotpAuthRepo : IAuthenticationRepository
{
    private readonly Dictionary<Guid, FakeTotpUser> _users = new();

    public Guid AddUser(bool totpEnabled = false, string? totpSecret = null)
    {
        var user = new FakeTotpUser
        {
            IsMfaTotpEnabled = totpEnabled,
            MfaTotpSecret = totpSecret
        };
        _users[user.UserId] = user;
        return user.UserId;
    }

    public FakeTotpUser GetUser(Guid userId) => _users[userId];

    public Task<IAuthenticationUser?> LookupUser(string userIdentity, CancellationToken cancellationToken) =>
        Task.FromResult<IAuthenticationUser?>(null);

    public Task<IAuthenticationUser?> UpdateUser(Guid userId, Func<IAuthenticationUser, bool> update, CancellationToken cancellationToken)
    {
        if (!_users.TryGetValue(userId, out var user))
            return Task.FromResult<IAuthenticationUser?>(null);
        update(user);
        return Task.FromResult<IAuthenticationUser?>(user);
    }

    public Task AddRefreshToken(Action<IRefreshToken> setProperties, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task<IAuthenticationUser?> UpdateRefreshToken(Guid userId, string refreshToken,
        Action<IRefreshToken> setProperties, CancellationToken cancellationToken) =>
        Task.FromResult<IAuthenticationUser?>(null);

    public Task DeleteRefreshToken(Guid userId, string refreshToken, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

// ─── TOTP auth service fake ──────────────────────────────────────────────────

internal class FakeTotpAuthService : IMetalGuardianAuthenticationService
{
    public bool LoginCalled { get; private set; }

    public Task<AuthenticationTokens> Login(string userIdentity, string password, string? deviceFingerprint, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<AuthenticationTokens> Login(IAuthenticationUser user, CancellationToken cancellationToken)
    {
        LoginCalled = true;
        return Task.FromResult(new AuthenticationTokens { AccessToken = "access-token", RefreshToken = "refresh-token" });
    }

    public Task Logout(AuthenticationTokens tokens, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task<AuthenticationTokens> Refresh(AuthenticationTokens tokens, CancellationToken cancellationToken) =>
        throw new NotSupportedException();
}

// ─── TOTP device repository fake ─────────────────────────────────────────────

internal class FakeTotpDeviceRepo : IUserDeviceRepository
{
    private readonly List<FakeTotpDevice> _devices = [];
    public bool AddCalled { get; private set; }
    public bool UpdateCalled { get; private set; }

    public void AddExistingDevice(Guid userId, string fingerprint)
    {
        _devices.Add(new FakeTotpDevice { UserId = userId, Fingerprint = fingerprint });
    }

    public Task Add(Action<IUserDevice> setProperties, CancellationToken cancellationToken)
    {
        AddCalled = true;
        var device = new FakeTotpDevice();
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
        UpdateCalled = true;
        var device = _devices.FirstOrDefault(d => d.UserId == userId && d.Fingerprint == deviceFingerprint);
        if (device != null) setProperties(device);
        return Task.CompletedTask;
    }
}

internal class FakeTotpDevice : IUserDevice
{
    public Guid UserId { get; set; }
    public IAuthenticationUser User { get; set; } = null!;
    public string Fingerprint { get; set; } = string.Empty;
    public DateTime? ExpiresOn { get; set; }
    public DateTime LastSeen { get; set; }
}
