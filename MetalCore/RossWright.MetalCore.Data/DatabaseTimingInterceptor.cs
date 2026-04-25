using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace RossWright;

/// <summary>
/// Extension methods for registering and attaching the <see cref="IDatabaseTimingInterceptor"/> EF Core interceptor.
/// </summary>
public static class DatabaseTimingInterceptorExtensions
{
    /// <summary>
    /// Registers <see cref="IDatabaseTimingInterceptor"/> as a scoped service in the DI container.
    /// </summary>
    /// <param name="service">The service collection.</param>
    /// <returns>The same <paramref name="service"/> for fluent chaining.</returns>
    public static IServiceCollection AddDatabaseTimingInterceptor(this IServiceCollection service) =>
        service.AddScoped<IDatabaseTimingInterceptor, DatabaseTimingInterceptor>();

    /// <summary>
    /// Attaches the scoped <see cref="IDatabaseTimingInterceptor"/> to a <see cref="DbContextOptionsBuilder"/>.
    /// </summary>
    /// <param name="builder">The EF Core options builder.</param>
    /// <param name="services">The service provider used to resolve the interceptor.</param>
    /// <returns>The same <paramref name="builder"/> for fluent chaining.</returns>
    public static DbContextOptionsBuilder UseDatabaseTimingInterceptor(this DbContextOptionsBuilder builder, IServiceProvider services) =>
        builder.AddInterceptors((IDbCommandInterceptor)services.GetRequiredService<IDatabaseTimingInterceptor>());
}

/// <summary>
/// Exposes cumulative EF Core command execution time for the current scope.
/// </summary>
public interface IDatabaseTimingInterceptor
{
    /// <summary>Gets the total time spent executing EF Core database commands in the current scope, in milliseconds.</summary>
    double RunTimeInMilliseconds { get; }
}

/// <summary>
/// EF Core <see cref="IDbCommandInterceptor"/> implementation that tracks per-command execution time.
/// </summary>
public class DatabaseTimingInterceptor : IDbCommandInterceptor, IDatabaseTimingInterceptor
{
    Dictionary<DbCommand, long> commandTimings = new Dictionary<DbCommand, long>();
    void Start(DbCommand cmd) => commandTimings.Add(cmd, DateTime.UtcNow.Ticks);
    void Stop(DbCommand cmd) => commandTimings[cmd] = DateTime.UtcNow.Ticks - commandTimings[cmd];
    /// <inheritdoc/>
    public double RunTimeInMilliseconds => commandTimings.Values.Sum() / (double)TimeSpan.TicksPerMillisecond;
    /// <inheritdoc/>
    public InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result) { Start(command); return result; }
    /// <inheritdoc/>
    public InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result) { Start(command); return result; }
    /// <inheritdoc/>
    public InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result) { Start(command); return result; }
    /// <inheritdoc/>
    public ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default) { Start(command); return new(result); }
    /// <inheritdoc/>
    public ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default) { Start(command); return new(result); }
    /// <inheritdoc/>
    public ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) { Start(command); return new(result); }
    /// <inheritdoc/>
    public DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result) { Stop(command); return result; }
    /// <inheritdoc/>
    public object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result) { Stop(command); return result; }
    /// <inheritdoc/>
    public int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result) { Stop(command); return result; }
    /// <inheritdoc/>
    public ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default) { Stop(command); return new(result); }
    /// <inheritdoc/>
    public ValueTask<object?> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object? result, CancellationToken cancellationToken = default) { Stop(command); return new(result); }
    /// <inheritdoc/>
    public ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default) { Stop(command); return new(result); }
}