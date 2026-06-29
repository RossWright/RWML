using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using RossWright.MetalGuardian;
using RossWright.MetalGuardian.Authentication;
using System.Linq.Expressions;

namespace RossWright.MetalGuardian.Server.Tests.Authentication.Internal;

public class AuthenticationRepositoryTests
{
    [Fact]
    public async Task LookupUser_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var expectedUser = new TestUser
        {
            UserId = userId,
            Name = userName
        };

        var users = new List<TestUser> { expectedUser };
        var dbSet = CreateMockDbSet(users);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.Users.Returns(dbSet);

        Func<string, Expression<Func<TestUser, bool>>> userIdentityPredicate = 
            identity => u => u.Name == identity;

        var sut = new AuthenticationRepository<TestDbContext, TestUser, TestRefreshToken>(
            dbContext, 
            userIdentityPredicate);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.LookupUser(userName, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        result.Name.ShouldBe(userName);
    }

    [Fact]
    public async Task LookupUser_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var users = new List<TestUser>();
        var dbSet = CreateMockDbSet(users);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.Users.Returns(dbSet);

        Func<string, Expression<Func<TestUser, bool>>> userIdentityPredicate = 
            identity => u => u.Name == identity;

        var sut = new AuthenticationRepository<TestDbContext, TestUser, TestRefreshToken>(
            dbContext, 
            userIdentityPredicate);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.LookupUser("nonexistent", cancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task AddRefreshToken_CreatesNewToken_AndSavesToDatabase()
    {
        // Arrange
        var dbContext = Substitute.For<TestDbContext>();
        var dbSet = Substitute.For<DbSet<TestRefreshToken>>();
        dbContext.RefreshTokens.Returns(dbSet);

        Func<string, Expression<Func<TestUser, bool>>> userIdentityPredicate = 
            identity => u => u.Name == identity;

        var sut = new AuthenticationRepository<TestDbContext, TestUser, TestRefreshToken>(
            dbContext, 
            userIdentityPredicate);

        var userId = Guid.NewGuid();
        var token = "test-token";
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.AddRefreshToken(rt =>
        {
            rt.UserId = userId;
            rt.Token = token;
        }, cancellationToken);

        // Assert
        dbSet.Received(1).Add(Arg.Is<TestRefreshToken>(t => t.UserId == userId && t.Token == token));
        await dbContext.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task AddRefreshToken_InvokesSetPropertiesCallback()
    {
        // Arrange
        var dbContext = Substitute.For<TestDbContext>();
        var dbSet = Substitute.For<DbSet<TestRefreshToken>>();
        dbContext.RefreshTokens.Returns(dbSet);

        Func<string, Expression<Func<TestUser, bool>>> userIdentityPredicate = 
            identity => u => u.Name == identity;

        var sut = new AuthenticationRepository<TestDbContext, TestUser, TestRefreshToken>(
            dbContext, 
            userIdentityPredicate);

        var callbackInvoked = false;
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.AddRefreshToken(rt =>
        {
            callbackInvoked = true;
        }, cancellationToken);

        // Assert
        callbackInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateRefreshToken_WhenTokenExists_UpdatesTokenAndReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test-token";
        var user = new TestUser { UserId = userId, Name = "testuser" };
        var originalLastSeen = DateTime.UtcNow.AddDays(-1);
        var newLastSeen = DateTime.UtcNow;
        var refreshToken = new TestRefreshToken
        {
            UserId = userId,
            Token = token,
            User = user,
            LastSeen = originalLastSeen
        };

        var tokens = new List<TestRefreshToken> { refreshToken };
        var dbSet = CreateMockDbSetWithInclude(tokens);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.RefreshTokens.Returns(dbSet);

        Func<string, Expression<Func<TestUser, bool>>> userIdentityPredicate = 
            identity => u => u.Name == identity;

        var sut = new AuthenticationRepository<TestDbContext, TestUser, TestRefreshToken>(
            dbContext, 
            userIdentityPredicate);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.UpdateRefreshToken(userId, token, rt =>
        {
            rt.LastSeen = newLastSeen;
        }, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        refreshToken.LastSeen.ShouldBe(newLastSeen);
        await dbContext.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateRefreshToken_WhenTokenDoesNotExist_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test-token";

        var tokens = new List<TestRefreshToken>();
        var dbSet = CreateMockDbSetWithInclude(tokens);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.RefreshTokens.Returns(dbSet);

        Func<string, Expression<Func<TestUser, bool>>> userIdentityPredicate = 
            identity => u => u.Name == identity;

        var sut = new AuthenticationRepository<TestDbContext, TestUser, TestRefreshToken>(
            dbContext, 
            userIdentityPredicate);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.UpdateRefreshToken(userId, token, rt =>
        {
            rt.LastSeen = DateTime.UtcNow;
        }, cancellationToken);

        // Assert
        result.ShouldBeNull();
        await dbContext.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateRefreshToken_WhenTokenExists_InvokesSetPropertiesCallback()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test-token";
        var user = new TestUser { UserId = userId, Name = "testuser" };
        var refreshToken = new TestRefreshToken
        {
            UserId = userId,
            Token = token,
            User = user
        };

        var tokens = new List<TestRefreshToken> { refreshToken };
        var dbSet = CreateMockDbSetWithInclude(tokens);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.RefreshTokens.Returns(dbSet);

        Func<string, Expression<Func<TestUser, bool>>> userIdentityPredicate = 
            identity => u => u.Name == identity;

        var sut = new AuthenticationRepository<TestDbContext, TestUser, TestRefreshToken>(
            dbContext, 
            userIdentityPredicate);
        var cancellationToken = CancellationToken.None;
        var callbackInvoked = false;

        // Act
        await sut.UpdateRefreshToken(userId, token, rt =>
        {
            callbackInvoked = true;
        }, cancellationToken);

        // Assert
        callbackInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateRefreshToken_WhenTokenDoesNotExist_DoesNotInvokeCallback()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test-token";

        var tokens = new List<TestRefreshToken>();
        var dbSet = CreateMockDbSetWithInclude(tokens);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.RefreshTokens.Returns(dbSet);

        Func<string, Expression<Func<TestUser, bool>>> userIdentityPredicate = 
            identity => u => u.Name == identity;

        var sut = new AuthenticationRepository<TestDbContext, TestUser, TestRefreshToken>(
            dbContext, 
            userIdentityPredicate);
        var cancellationToken = CancellationToken.None;
        var callbackInvoked = false;

        // Act
        await sut.UpdateRefreshToken(userId, token, rt =>
        {
            callbackInvoked = true;
        }, cancellationToken);

        // Assert
        callbackInvoked.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteRefreshToken_WhenTokenExists_RemovesTokenAndSaves()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test-token";
        var refreshToken = new TestRefreshToken
        {
            UserId = userId,
            Token = token
        };

        var tokens = new List<TestRefreshToken> { refreshToken };
        var dbSet = CreateMockDbSet(tokens);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.RefreshTokens.Returns(dbSet);
        var mockSet = Substitute.For<DbSet<TestRefreshToken>>();
        dbContext.Set<TestRefreshToken>().Returns(mockSet);

        Func<string, Expression<Func<TestUser, bool>>> userIdentityPredicate = 
            identity => u => u.Name == identity;

        var sut = new AuthenticationRepository<TestDbContext, TestUser, TestRefreshToken>(
            dbContext, 
            userIdentityPredicate);
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.DeleteRefreshToken(userId, token, cancellationToken);

        // Assert
        mockSet.Received(1).Remove(Arg.Is<TestRefreshToken>(t => t.UserId == userId && t.Token == token));
        await dbContext.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task DeleteRefreshToken_WhenTokenDoesNotExist_DoesNotRemoveOrSave()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "test-token";

        var tokens = new List<TestRefreshToken>();
        var dbSet = CreateMockDbSet(tokens);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.RefreshTokens.Returns(dbSet);
        var mockSet = Substitute.For<DbSet<TestRefreshToken>>();
        dbContext.Set<TestRefreshToken>().Returns(mockSet);

        Func<string, Expression<Func<TestUser, bool>>> userIdentityPredicate = 
            identity => u => u.Name == identity;

        var sut = new AuthenticationRepository<TestDbContext, TestUser, TestRefreshToken>(
            dbContext, 
            userIdentityPredicate);
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.DeleteRefreshToken(userId, token, cancellationToken);

        // Assert
        mockSet.DidNotReceive().Remove(Arg.Any<TestRefreshToken>());
        await dbContext.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateUser_WhenUserExistsAndUpdateReturnsTrue_SavesChangesAndReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new TestUser
        {
            UserId = userId,
            Name = "testuser",
            PasswordSalt = "oldsalt"
        };

        var users = new List<TestUser> { user };
        var dbSet = CreateMockDbSet(users);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.Users.Returns(dbSet);

        Func<string, Expression<Func<TestUser, bool>>> userIdentityPredicate = 
            identity => u => u.Name == identity;

        var sut = new AuthenticationRepository<TestDbContext, TestUser, TestRefreshToken>(
            dbContext, 
            userIdentityPredicate);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.UpdateUser(userId, u =>
        {
            u.PasswordSalt = "newsalt";
            return true;
        }, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        user.PasswordSalt.ShouldBe("newsalt");
        await dbContext.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateUser_WhenUserExistsAndUpdateReturnsFalse_DoesNotSaveAndReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new TestUser
        {
            UserId = userId,
            Name = "testuser",
            PasswordSalt = "oldsalt"
        };

        var users = new List<TestUser> { user };
        var dbSet = CreateMockDbSet(users);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.Users.Returns(dbSet);

        Func<string, Expression<Func<TestUser, bool>>> userIdentityPredicate = 
            identity => u => u.Name == identity;

        var sut = new AuthenticationRepository<TestDbContext, TestUser, TestRefreshToken>(
            dbContext, 
            userIdentityPredicate);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.UpdateUser(userId, u =>
        {
            return false;
        }, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        await dbContext.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateUser_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var users = new List<TestUser>();
        var dbSet = CreateMockDbSet(users);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.Users.Returns(dbSet);

        Func<string, Expression<Func<TestUser, bool>>> userIdentityPredicate = 
            identity => u => u.Name == identity;

        var sut = new AuthenticationRepository<TestDbContext, TestUser, TestRefreshToken>(
            dbContext, 
            userIdentityPredicate);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.UpdateUser(userId, u =>
        {
            return true;
        }, cancellationToken);

        // Assert
        result.ShouldBeNull();
        await dbContext.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateUser_WhenUserExists_InvokesUpdateCallback()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new TestUser
        {
            UserId = userId,
            Name = "testuser"
        };

        var users = new List<TestUser> { user };
        var dbSet = CreateMockDbSet(users);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.Users.Returns(dbSet);

        Func<string, Expression<Func<TestUser, bool>>> userIdentityPredicate = 
            identity => u => u.Name == identity;

        var sut = new AuthenticationRepository<TestDbContext, TestUser, TestRefreshToken>(
            dbContext, 
            userIdentityPredicate);
        var cancellationToken = CancellationToken.None;
        var callbackInvoked = false;

        // Act
        await sut.UpdateUser(userId, u =>
        {
            callbackInvoked = true;
            return true;
        }, cancellationToken);

        // Assert
        callbackInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateUser_WhenUserDoesNotExist_DoesNotInvokeCallback()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var users = new List<TestUser>();
        var dbSet = CreateMockDbSet(users);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.Users.Returns(dbSet);

        Func<string, Expression<Func<TestUser, bool>>> userIdentityPredicate = 
            identity => u => u.Name == identity;

        var sut = new AuthenticationRepository<TestDbContext, TestUser, TestRefreshToken>(
            dbContext, 
            userIdentityPredicate);
        var cancellationToken = CancellationToken.None;
        var callbackInvoked = false;

        // Act
        await sut.UpdateUser(userId, u =>
        {
            callbackInvoked = true;
            return true;
        }, cancellationToken);

        // Assert
        callbackInvoked.ShouldBeFalse();
    }

    // Helper method to create a mock DbSet with async query support
    private static DbSet<T> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var dbSet = Substitute.For<DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>>();
        
        ((IQueryable<T>)dbSet).Provider.Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        ((IQueryable<T>)dbSet).Expression.Returns(queryable.Expression);
        ((IQueryable<T>)dbSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<T>)dbSet).GetEnumerator().Returns(_ => queryable.GetEnumerator());
        ((IAsyncEnumerable<T>)dbSet).GetAsyncEnumerator(Arg.Any<CancellationToken>()).Returns(_ => new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        
        return dbSet;
    }

    // Helper method to create a mock DbSet with Include support for refresh tokens
    private static DbSet<TestRefreshToken> CreateMockDbSetWithInclude(List<TestRefreshToken> data)
    {
        var queryable = data.AsQueryable();
        var dbSet = Substitute.For<DbSet<TestRefreshToken>, IQueryable<TestRefreshToken>, IAsyncEnumerable<TestRefreshToken>>();
        
        ((IQueryable<TestRefreshToken>)dbSet).Provider.Returns(new TestAsyncQueryProvider<TestRefreshToken>(queryable.Provider));
        ((IQueryable<TestRefreshToken>)dbSet).Expression.Returns(queryable.Expression);
        ((IQueryable<TestRefreshToken>)dbSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<TestRefreshToken>)dbSet).GetEnumerator().Returns(_ => queryable.GetEnumerator());
        ((IAsyncEnumerable<TestRefreshToken>)dbSet).GetAsyncEnumerator(Arg.Any<CancellationToken>()).Returns(_ => new TestAsyncEnumerator<TestRefreshToken>(queryable.GetEnumerator()));
        
        return dbSet;
    }

    // Test helper types
    public abstract class TestDbContext : DbContext, IMetalGuardianDbContext<TestUser, TestRefreshToken>
    {
        public abstract DbSet<TestUser> Users { get; }
        public abstract DbSet<TestRefreshToken> RefreshTokens { get; }
    }

    public class TestUser : IAuthenticationUser
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDisabled { get; set; }
        public string PasswordSalt { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }

    public class TestRefreshToken : IRefreshToken
    {
        public Guid UserId { get; set; }
        public IAuthenticationUser User { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresOn { get; set; }
        public DateTime LastSeen { get; set; }
    }

    // Async query provider implementation for testing
    internal class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => 
            new TestAsyncEnumerable<TEntity>(expression, this);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => 
            new TestAsyncEnumerable<TElement>(expression, this as IAsyncQueryProvider ?? new TestAsyncQueryProvider<TElement>(inner));

        public object? Execute(Expression expression) => 
            inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => 
            inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(nameof(IQueryProvider.Execute), 1, [typeof(Expression)])!
                .MakeGenericMethod(resultType)
                .Invoke(inner, [expression]);

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, [executionResult])!;
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        private readonly IAsyncQueryProvider _provider;

        public TestAsyncEnumerable(Expression expression, IAsyncQueryProvider provider) : base(expression)
        {
            _provider = provider;
        }

        IQueryProvider IQueryable.Provider => _provider;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => 
            new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    internal class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;

        public ValueTask<bool> MoveNextAsync() => 
            new(inner.MoveNext());

        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
