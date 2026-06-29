using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace RossWright.MetalGuardian.Authentication;

internal class AuthenticationRepository<TDbContext, TUser, TRefreshToken>(
    TDbContext _dbCtx,
    Func<string, Expression<Func<TUser, bool>>> _userIdentityPredicate)
    : IAuthenticationRepository
    where TDbContext : DbContext, IMetalGuardianDbContext<TUser, TRefreshToken>
    where TUser : class, IAuthenticationUser
    where TRefreshToken : class, IRefreshToken, new()
{
    public async Task<IAuthenticationUser?> LookupUser(string userIdentity, CancellationToken cancellationToken) =>
        await _dbCtx.Users.FirstOrDefaultAsync(_userIdentityPredicate(userIdentity), cancellationToken);

    public async Task AddRefreshToken(Action<IRefreshToken> setProperties, CancellationToken cancellationToken)
    {
        var dbRefreshToken = new TRefreshToken();
        setProperties(dbRefreshToken);
        _dbCtx.RefreshTokens.Add(dbRefreshToken);
        await _dbCtx.SaveChangesAsync(cancellationToken);
    }

    public async Task<IAuthenticationUser?> UpdateRefreshToken(Guid userId, string refreshToken,
        Action<IRefreshToken> setProperties, CancellationToken cancellationToken)
    {
        var dbRefreshToken = await _dbCtx.RefreshTokens
            .Include(_ => _.User)
            .Where(_ => _.UserId == userId && _.Token == refreshToken)
            .FirstOrDefaultAsync(cancellationToken);
        if (dbRefreshToken != null)
        {
            setProperties(dbRefreshToken);
            await _dbCtx.SaveChangesAsync();
        }
        return dbRefreshToken?.User;
    }

    public async Task DeleteRefreshToken(Guid userId, string refreshToken,
        CancellationToken cancellationToken)
    {
        var dbRefreshToken = await _dbCtx.RefreshTokens
            .Where(_ => _.UserId == userId && _.Token == refreshToken)
            .FirstOrDefaultAsync(cancellationToken);
        if (dbRefreshToken != null)
        {
            _dbCtx.Set<TRefreshToken>().Remove(dbRefreshToken);
            await _dbCtx.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IAuthenticationUser?> UpdateUser(Guid userId, Func<IAuthenticationUser, bool> update, CancellationToken cancellationToken)
    { 
        var dbUser = await _dbCtx.Users.FirstOrDefaultAsync(_ => _.UserId == userId, cancellationToken);
        if (dbUser != null && update(dbUser)) await _dbCtx.SaveChangesAsync();
        return dbUser;
    }
}
