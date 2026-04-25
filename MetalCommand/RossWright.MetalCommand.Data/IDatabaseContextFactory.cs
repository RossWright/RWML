namespace RossWright.MetalCommand.Data;

public interface IDatabaseContextFactory
{
    string DefaultEnvironment { get; }
    DatabaseEnvironment[] DatabaseEnvironments { get; }
}

public interface IDatabaseContextFactory<TDBCTX> : IDatabaseContextFactory where TDBCTX : DbContext
{
    TDBCTX? GetContext(string? environment = null);
}

public static class IDatabaseContextFactoryExtensions
{
    public static ArgumentDescriptor GetEnvironmentArg(
        this IDatabaseContextFactory dbCtxFac, string name = "env",
        string? helpDetail = null) => 
        ArgumentDescriptor.OptionalWithValidValues(name, 
            dbCtxFac.DatabaseEnvironments
                .Select(_ => _.Environment)
                .ToArray(),
            dbCtxFac.DefaultEnvironment, helpDetail);

    public static ArgumentDescriptor GetUnprotectedEnvironmentArg(
        this IDatabaseContextFactory dbCtxFac, string name = "env",
        string? helpDetail = null) => 
        ArgumentDescriptor.OptionalWithValidValues(name,
            dbCtxFac.DatabaseEnvironments
                .Where(_ => !_.IsProtected)
                .Select(_ => _.Environment)
                .ToArray(),
            dbCtxFac.DefaultEnvironment, helpDetail);
}
