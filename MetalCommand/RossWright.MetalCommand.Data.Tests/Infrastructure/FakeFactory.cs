namespace RossWright.MetalCommand.Data.Tests.Infrastructure;

/// <summary>
/// IDatabaseContextFactory&lt;TestDbContext&gt; test double with a pre-built context.
/// </summary>
public class FakeFactory : IDatabaseContextFactory<TestDbContext>
{
    private readonly TestDbContext _context;

    public FakeFactory(
        TestDbContext context,
        DatabaseEnvironment[] environments,
        string? defaultEnvironment = null)
    {
        _context = context;
        DatabaseEnvironments = environments;
        DefaultEnvironment = defaultEnvironment ?? environments.FirstOrDefault()?.Environment ?? string.Empty;
    }

    public static FakeFactory CreateDefault(TestDbContext? context = null)
    {
        var envs = new[]
        {
            new DatabaseEnvironment { Environment = "Dev",  IsProtected = false, SetOptions = b => b.UseInMemoryDatabase("Dev") },
            new DatabaseEnvironment { Environment = "Prod", IsProtected = true,  SetOptions = b => b.UseInMemoryDatabase("Prod") },
        };
        return new FakeFactory(context ?? DbContextFixture.BuildInMemory(), envs, "Dev");
    }

    public string DefaultEnvironment { get; }
    public DatabaseEnvironment[] DatabaseEnvironments { get; }

    public TestDbContext GetContext(string? environment = null) => _context;
}
