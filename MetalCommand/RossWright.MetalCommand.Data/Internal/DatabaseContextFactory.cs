using System.Reflection;

namespace RossWright.MetalCommand.Data;

internal class DatabaseContextFactory<TDBCTX> : IDatabaseContextFactory<TDBCTX>, IDisposable
    where TDBCTX : DbContext
{
    public DatabaseContextFactory(
        IEnumerable<DatabaseEnvironment> databaseEnvironments, 
        string defaultEnvironment)
    {
        Connections = databaseEnvironments.ToDictionary(_ => _.Environment, StringComparer.OrdinalIgnoreCase);
        DefaultEnvironment = defaultEnvironment;
        DatabaseEnvironments = databaseEnvironments.ToArray();
    }
    internal Dictionary<string, DatabaseEnvironment> Connections { get; }

    public DatabaseEnvironment[] DatabaseEnvironments { get; }
    public string DefaultEnvironment { get; }
    public EnvironmentEntry[] Environments =>
        DatabaseEnvironments.Select(e => new EnvironmentEntry { Name = e.Environment, IsProtected = e.IsProtected }).ToArray();

    private readonly List<TDBCTX> _issued = [];

    public TDBCTX GetContext(string? environment = null)
    {
        environment ??= DefaultEnvironment;
        if (!Connections.TryGetValue(environment, out var connection))
        {
            throw new InvalidOperationException($"No database context registered for environment '{environment}'.");
        }
        var dataBuilder = new DbContextOptionsBuilder<TDBCTX>();
        connection.SetOptions(dataBuilder);
        var ctx = MetalActivator.CreateInstance<TDBCTX>(dataBuilder.Options)!;
        _issued.Add(ctx);
        return ctx;
    }

    public void Dispose()
    {
        foreach (var ctx in _issued)
            ctx.Dispose();
        _issued.Clear();
    }
}
