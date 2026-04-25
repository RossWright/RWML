namespace RossWright.MetalCommand.Data;

public static class TryParseEnvironmentExtension
{
    public static string? TryParseEnvironment<TDBCTX>(
        this IConsole console,
        IDatabaseContextFactory<TDBCTX> iDbCtxFac,
        string? environment,
        bool allowProtected = false)
        where TDBCTX : DbContext
    {
        var dbCtxFac = ((DatabaseContextFactory<TDBCTX>)iDbCtxFac);

        environment ??= dbCtxFac.DefaultEnvironment;
        if (!dbCtxFac.Connections.TryGetValue(environment.ToLower(), out var connection))
        {
            var validEnv = dbCtxFac.Connections.Values
                .Where(_ => allowProtected || !_.IsProtected)
                .Select(_ => _.Environment);
            console.WriteErrorLine("Unknown environment" + (validEnv.Any()
                ? $", try using {validEnv.CommaListJoin("or")}"
                : ", no valid environments are available for this command"));
            return null;
        }

        if (connection.IsProtected)
        {
            if (!allowProtected)
            {
                var validEnv = dbCtxFac.Connections.Values
                    .Where(_ => !_.IsProtected)
                    .Select(_ => _.Environment);
                console.WriteErrorLine("That enviroment cannot be used with this command" + (validEnv.Any()
                    ? $", try using {validEnv.CommaListJoin("or")}"
                    : ", no valid environments are available for this command"));
                return null;
            }

            console.Write($"Are you sure? (type \"yes\" to confirm): ");
            if (console.ReadLine() != "yes")
            {
                console.WriteErrorLine("Command aborted");
                return null;
            }
        }

        return connection.Environment;
    }
}
