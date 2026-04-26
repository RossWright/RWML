using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RossWright.MetalCommand.Tesbed;

public class TestbedDbContext : DbContext
{
    public TestbedDbContext(DbContextOptions<TestbedDbContext> options) : base(options) { }
    public DbSet<Thing> Things { get; set; } = null!;
}

public class Thing
{
    public Guid ThingId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
}

internal class TestbedDbContextFactory : IDesignTimeDbContextFactory<TestbedDbContext>
{
    public TestbedDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DataConnection_local")
            ?? throw new InvalidOperationException("Connection string 'DataConnection_local' not found.");

        var options = new DbContextOptionsBuilder<TestbedDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new TestbedDbContext(options);
    }
}
