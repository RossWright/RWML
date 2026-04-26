using RossWright;
using RossWright.MetalCommand;
using RossWright.MetalCommand.Data;
using RossWright.MetalCommand.Http;
using RossWright.MetalCommand.Tesbed;

await ConsoleApplication.CreateBuilder()
    .AddHttpConnections(cfg =>
    {
        cfg.AddDefaultByConfigurationName("local", "HttpUrls:local");
        cfg.AddByConfigurationName("test", "HttpUrls:test");
        cfg.AddProtectedByConfigurationName("prod", "HttpUrls:prod");
    })
    .AddPingCommand()
    .AddDatabaseContextFactory<TestbedDbContext>(_ =>
    {
        _.AddSqlServerDefaultByConfigurationName("local", "DataConnection_local");
        _.AddSqlServerByConfigurationName("test", "DataConnection_test");
        _.AddSqlServerProtectedByConfigurationName("prod", "DataConnection_prod");
    })
    .AddMigrateCommand<TestbedDbContext>(cfg =>
    {
        cfg.Invocations = ["migrate", "m"];
        cfg.EnvironmentPolicy = EnvironmentPolicy.Dangerous;
    })
    .AddObliterateCommand<TestbedDbContext>(cfg =>
    {
        cfg.Invocations = ["obliterate"];
        cfg.EnvironmentPolicy = EnvironmentPolicy.Dangerous;
    })
    .AddClearDataCommand<TestbedDbContext>(cfg =>
    {
        cfg.Invocations = ["clear-data", "cd"];
        cfg.TableNames = [nameof(TestbedDbContext.Things)];
    })
    .AddLoadDataCommand<TestbedDbContext>(cfg =>
    {
        cfg.Invocations = ["load-data", "ld"];
        cfg.LoadData = async ctx =>
        {
            ctx.DbContext.Things.AddRange(
                new Thing { Name = "Thing 1" },
                new Thing { Name = "Thing 2" },
                new Thing { Name = "Thing 3" });
        };
    })
    .AddReloadDatabaseCommand<TestbedDbContext>(cfg =>
    {
        cfg.Invocations = ["reload-data", "rd"];
    })
    .AddCommands(_ => _.ScanThisAssembly())
    .LoadContext(showWarnIfMissing: false)
    .Build()
    .RunAsync(args);

