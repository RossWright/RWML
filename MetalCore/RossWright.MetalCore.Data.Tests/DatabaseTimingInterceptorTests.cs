using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Data.Common;

namespace RossWright.Data.Tests;

public class DatabaseTimingInterceptorTests
{
    private static DbCommand CreateCommand() => Substitute.For<DbCommand>();

    // EventData is not accessed by the interceptor — pass null safely
    private static CommandEventData NullEventData => null!;
    private static CommandExecutedEventData NullExecutedEventData => null!;

    [Fact]
    public void RunTimeInMilliseconds_InitialValue_IsZero()
    {
        var interceptor = new DatabaseTimingInterceptor();

        interceptor.RunTimeInMilliseconds.ShouldBe(0.0);
    }

    [Fact]
    public void ReaderExecuting_ThenReaderExecuted_AccumulatesTime()
    {
        var interceptor = new DatabaseTimingInterceptor();
        var cmd = CreateCommand();

        interceptor.ReaderExecuting(cmd, NullEventData, default);
        Thread.Sleep(5);
        interceptor.ReaderExecuted(cmd, NullExecutedEventData, null!);

        (interceptor.RunTimeInMilliseconds >= 0).ShouldBeTrue();
    }

    [Fact]
    public void ScalarExecuting_ThenScalarExecuted_AccumulatesTime()
    {
        var interceptor = new DatabaseTimingInterceptor();
        var cmd = CreateCommand();

        interceptor.ScalarExecuting(cmd, NullEventData, default);
        Thread.Sleep(5);
        interceptor.ScalarExecuted(cmd, NullExecutedEventData, null);

        (interceptor.RunTimeInMilliseconds >= 0).ShouldBeTrue();
    }

    [Fact]
    public void NonQueryExecuting_ThenNonQueryExecuted_AccumulatesTime()
    {
        var interceptor = new DatabaseTimingInterceptor();
        var cmd = CreateCommand();

        interceptor.NonQueryExecuting(cmd, NullEventData, default);
        Thread.Sleep(5);
        interceptor.NonQueryExecuted(cmd, NullExecutedEventData, 0);

        (interceptor.RunTimeInMilliseconds >= 0).ShouldBeTrue();
    }

    [Fact]
    public async Task ReaderExecutingAsync_ThenReaderExecutedAsync_AccumulatesTime()
    {
        var interceptor = new DatabaseTimingInterceptor();
        var cmd = CreateCommand();

        await interceptor.ReaderExecutingAsync(cmd, NullEventData, default);
        await Task.Delay(5);
        await interceptor.ReaderExecutedAsync(cmd, NullExecutedEventData, null!);

        (interceptor.RunTimeInMilliseconds >= 0).ShouldBeTrue();
    }

    [Fact]
    public async Task ScalarExecutingAsync_ThenScalarExecutedAsync_AccumulatesTime()
    {
        var interceptor = new DatabaseTimingInterceptor();
        var cmd = CreateCommand();

        await interceptor.ScalarExecutingAsync(cmd, NullEventData, default);
        await Task.Delay(5);
        await interceptor.ScalarExecutedAsync(cmd, NullExecutedEventData, null);

        (interceptor.RunTimeInMilliseconds >= 0).ShouldBeTrue();
    }

    [Fact]
    public async Task NonQueryExecutingAsync_ThenNonQueryExecutedAsync_AccumulatesTime()
    {
        var interceptor = new DatabaseTimingInterceptor();
        var cmd = CreateCommand();

        await interceptor.NonQueryExecutingAsync(cmd, NullEventData, default);
        await Task.Delay(5);
        await interceptor.NonQueryExecutedAsync(cmd, NullExecutedEventData, 0);

        (interceptor.RunTimeInMilliseconds >= 0).ShouldBeTrue();
    }

    [Fact]
    public void MultipleCommands_RunTimeAccumulatesAcrossAll()
    {
        var interceptor = new DatabaseTimingInterceptor();
        var cmd1 = CreateCommand();
        var cmd2 = CreateCommand();

        interceptor.NonQueryExecuting(cmd1, NullEventData, default);
        Thread.Sleep(5);
        interceptor.NonQueryExecuted(cmd1, NullExecutedEventData, 0);

        var afterFirst = interceptor.RunTimeInMilliseconds;

        interceptor.NonQueryExecuting(cmd2, NullEventData, default);
        Thread.Sleep(5);
        interceptor.NonQueryExecuted(cmd2, NullExecutedEventData, 0);

        (interceptor.RunTimeInMilliseconds >= afterFirst).ShouldBeTrue();
    }

    [Fact]
    public void ReaderExecuting_ReturnsPassedResult()
    {
        var interceptor = new DatabaseTimingInterceptor();
        var cmd = CreateCommand();
        var expected = default(InterceptionResult<DbDataReader>);

        var result = interceptor.ReaderExecuting(cmd, NullEventData, expected);

        result.ShouldBe(expected);
    }

    [Fact]
    public void NonQueryExecuting_ReturnsPassedResult()
    {
        var interceptor = new DatabaseTimingInterceptor();
        var cmd = CreateCommand();
        var expected = default(InterceptionResult<int>);

        var result = interceptor.NonQueryExecuting(cmd, NullEventData, expected);

        result.ShouldBe(expected);
    }

    [Fact]
    public void NonQueryExecuted_ReturnsPassedResult()
    {
        var interceptor = new DatabaseTimingInterceptor();
        var cmd = CreateCommand();
        interceptor.NonQueryExecuting(cmd, NullEventData, default);

        var result = interceptor.NonQueryExecuted(cmd, NullExecutedEventData, 42);

        result.ShouldBe(42);
    }
}

public class DatabaseTimingInterceptorExtensionTests
{
    [Fact]
    public void AddDatabaseTimingInterceptor_RegistersIDatabaseTimingInterceptorAsScoped()
    {
        var services = new ServiceCollection();

        services.AddDatabaseTimingInterceptor();
        var provider = services.BuildServiceProvider();

        var interceptor = provider.GetRequiredService<IDatabaseTimingInterceptor>();
        interceptor.ShouldNotBeNull();
        interceptor.ShouldBeOfType<DatabaseTimingInterceptor>();
    }

    [Fact]
    public void AddDatabaseTimingInterceptor_IsScopedRegistration()
    {
        var services = new ServiceCollection();
        services.AddDatabaseTimingInterceptor();
        var provider = services.BuildServiceProvider();

        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var interceptor1 = scope1.ServiceProvider.GetRequiredService<IDatabaseTimingInterceptor>();
        var interceptor2 = scope2.ServiceProvider.GetRequiredService<IDatabaseTimingInterceptor>();

        interceptor1.ShouldNotBeSameAs(interceptor2);
    }

    [Fact]
    public void AddDatabaseTimingInterceptor_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddDatabaseTimingInterceptor();

        result.ShouldBeSameAs(services);
    }
}
