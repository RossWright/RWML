using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using RossWright.MetalGuardian;
using RossWright.MetalGuardian.Authentication;
using System.Linq.Expressions;

namespace RossWright.MetalGuardian.Server.Tests.Authentication.Internal;

public class UserDeviceRepositoryTests
{
    [Fact]
    public async Task Add_CreatesNewDevice_AndSavesToDatabase()
    {
        // Arrange
        var dbContext = Substitute.For<TestDbContext>();
        var dbSet = Substitute.For<DbSet<TestUserDevice>>();
        dbContext.UserDevices.Returns(dbSet);
        
        var sut = new UserDeviceRepository<TestDbContext, TestUser, TestRefreshToken, TestUserDevice>(dbContext);
        var userId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Add(device =>
        {
            device.UserId = userId;
            device.Fingerprint = fingerprint;
        }, cancellationToken);

        // Assert
        dbSet.Received(1).Add(Arg.Is<TestUserDevice>(d => d.UserId == userId && d.Fingerprint == fingerprint));
        await dbContext.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task Add_InvokesSetPropertiesCallback()
    {
        // Arrange
        var dbContext = Substitute.For<TestDbContext>();
        var dbSet = Substitute.For<DbSet<TestUserDevice>>();
        dbContext.UserDevices.Returns(dbSet);
        
        var sut = new UserDeviceRepository<TestDbContext, TestUser, TestRefreshToken, TestUserDevice>(dbContext);
        var callbackInvoked = false;
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Add(device =>
        {
            callbackInvoked = true;
        }, cancellationToken);

        // Assert
        callbackInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Get_WhenDeviceExists_ReturnsDevice()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";
        var expectedDevice = new TestUserDevice
        {
            UserId = userId,
            Fingerprint = fingerprint
        };

        var devices = new List<TestUserDevice> { expectedDevice };
        var dbSet = CreateMockDbSet(devices);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.UserDevices.Returns(dbSet);

        var sut = new UserDeviceRepository<TestDbContext, TestUser, TestRefreshToken, TestUserDevice>(dbContext);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.Get(userId, fingerprint, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        result.Fingerprint.ShouldBe(fingerprint);
    }

    [Fact]
    public async Task Get_WhenDeviceDoesNotExist_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";

        var devices = new List<TestUserDevice>();
        var dbSet = CreateMockDbSet(devices);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.UserDevices.Returns(dbSet);

        var sut = new UserDeviceRepository<TestDbContext, TestUser, TestRefreshToken, TestUserDevice>(dbContext);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.Get(userId, fingerprint, cancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task Get_WhenUserIdDoesNotMatch_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";
        var device = new TestUserDevice
        {
            UserId = differentUserId,
            Fingerprint = fingerprint
        };

        var devices = new List<TestUserDevice> { device };
        var dbSet = CreateMockDbSet(devices);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.UserDevices.Returns(dbSet);

        var sut = new UserDeviceRepository<TestDbContext, TestUser, TestRefreshToken, TestUserDevice>(dbContext);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.Get(userId, fingerprint, cancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task Get_WhenFingerprintDoesNotMatch_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";
        var differentFingerprint = "different-fingerprint";
        var device = new TestUserDevice
        {
            UserId = userId,
            Fingerprint = differentFingerprint
        };

        var devices = new List<TestUserDevice> { device };
        var dbSet = CreateMockDbSet(devices);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.UserDevices.Returns(dbSet);

        var sut = new UserDeviceRepository<TestDbContext, TestUser, TestRefreshToken, TestUserDevice>(dbContext);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.Get(userId, fingerprint, cancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task Update_WhenDeviceExists_UpdatesDeviceAndSaves()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";
        var originalLastSeen = DateTime.UtcNow.AddDays(-1);
        var newLastSeen = DateTime.UtcNow;
        var device = new TestUserDevice
        {
            UserId = userId,
            Fingerprint = fingerprint,
            LastSeen = originalLastSeen
        };

        var devices = new List<TestUserDevice> { device };
        var dbSet = CreateMockDbSet(devices);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.UserDevices.Returns(dbSet);

        var sut = new UserDeviceRepository<TestDbContext, TestUser, TestRefreshToken, TestUserDevice>(dbContext);
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Update(userId, fingerprint, d =>
        {
            d.LastSeen = newLastSeen;
        }, cancellationToken);

        // Assert
        device.LastSeen.ShouldBe(newLastSeen);
        await dbContext.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Update_WhenDeviceDoesNotExist_DoesNotSave()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";

        var devices = new List<TestUserDevice>();
        var dbSet = CreateMockDbSet(devices);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.UserDevices.Returns(dbSet);

        var sut = new UserDeviceRepository<TestDbContext, TestUser, TestRefreshToken, TestUserDevice>(dbContext);
        var cancellationToken = CancellationToken.None;

        // Act
        await sut.Update(userId, fingerprint, d =>
        {
            d.LastSeen = DateTime.UtcNow;
        }, cancellationToken);

        // Assert
        await dbContext.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Update_WhenDeviceExists_InvokesSetPropertiesCallback()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";
        var device = new TestUserDevice
        {
            UserId = userId,
            Fingerprint = fingerprint
        };

        var devices = new List<TestUserDevice> { device };
        var dbSet = CreateMockDbSet(devices);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.UserDevices.Returns(dbSet);

        var sut = new UserDeviceRepository<TestDbContext, TestUser, TestRefreshToken, TestUserDevice>(dbContext);
        var cancellationToken = CancellationToken.None;
        var callbackInvoked = false;

        // Act
        await sut.Update(userId, fingerprint, d =>
        {
            callbackInvoked = true;
        }, cancellationToken);

        // Assert
        callbackInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Update_WhenDeviceDoesNotExist_DoesNotInvokeCallback()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";

        var devices = new List<TestUserDevice>();
        var dbSet = CreateMockDbSet(devices);

        var dbContext = Substitute.For<TestDbContext>();
        dbContext.UserDevices.Returns(dbSet);

        var sut = new UserDeviceRepository<TestDbContext, TestUser, TestRefreshToken, TestUserDevice>(dbContext);
        var cancellationToken = CancellationToken.None;
        var callbackInvoked = false;

        // Act
        await sut.Update(userId, fingerprint, d =>
        {
            callbackInvoked = true;
        }, cancellationToken);

        // Assert
        callbackInvoked.ShouldBeFalse();
    }

    // Helper method to create a mock DbSet with async query support
    private static DbSet<TestUserDevice> CreateMockDbSet(List<TestUserDevice> devices)
    {
        var queryable = devices.AsQueryable();
        var dbSet = Substitute.For<DbSet<TestUserDevice>, IQueryable<TestUserDevice>, IAsyncEnumerable<TestUserDevice>>();
        
        ((IQueryable<TestUserDevice>)dbSet).Provider.Returns(new TestAsyncQueryProvider<TestUserDevice>(queryable.Provider));
        ((IQueryable<TestUserDevice>)dbSet).Expression.Returns(queryable.Expression);
        ((IQueryable<TestUserDevice>)dbSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<TestUserDevice>)dbSet).GetEnumerator().Returns(_ => queryable.GetEnumerator());
        ((IAsyncEnumerable<TestUserDevice>)dbSet).GetAsyncEnumerator(Arg.Any<CancellationToken>()).Returns(_ => new TestAsyncEnumerator<TestUserDevice>(queryable.GetEnumerator()));
        
        return dbSet;
    }

    // Test helper types
    public abstract class TestDbContext : DbContext, IMetalGuardianDbContext<TestUser, TestRefreshToken, TestUserDevice>
    {
        public abstract DbSet<TestUser> Users { get; }
        public abstract DbSet<TestRefreshToken> RefreshTokens { get; }
        public abstract DbSet<TestUserDevice> UserDevices { get; }
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

    public class TestUserDevice : IUserDevice
    {
        public Guid UserId { get; set; }
        public IAuthenticationUser User { get; set; } = null!;
        public string Fingerprint { get; set; } = string.Empty;
        public DateTime? ExpiresOn { get; set; }
        public DateTime LastSeen { get; set; }
    }

    // Async query provider implementation for testing
    internal class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => 
            new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => 
            new TestAsyncEnumerable<TElement>(expression);

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

    internal class TestAsyncEnumerable<T>(Expression expression) : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
    {
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
