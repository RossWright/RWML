using System.Reflection;

namespace RossWright.MetalCommand.Data;

internal class DatabaseContextFactory<TDBCTX> : IDatabaseContextFactory<TDBCTX> where TDBCTX : DbContext
{
    public DatabaseContextFactory(
        IEnumerable<DatabaseEnvironment> databaseEnvironments, 
        string defaultEnvironment)
    {
        Connections = databaseEnvironments.ToDictionary(_ => _.Environment);
        DefaultEnvironment = defaultEnvironment;
        DatabaseEnvironments = databaseEnvironments.ToArray();
    }
    internal Dictionary<string, DatabaseEnvironment> Connections { get; }

    public DatabaseEnvironment[] DatabaseEnvironments { get; }
    public string DefaultEnvironment { get; }

    public TDBCTX? GetContext(string? environment = null)
    {
        environment ??= DefaultEnvironment;
        if (!Connections.TryGetValue(environment, out var connection))
        {
            throw new KeyNotFoundException("Invalid environment");
        }
        var dataBuilder = new DbContextOptionsBuilder<TDBCTX>();
        connection.SetOptions(dataBuilder);
        return MetalActivator.CreateInstance<TDBCTX>(dataBuilder.Options)!;
    }
}
