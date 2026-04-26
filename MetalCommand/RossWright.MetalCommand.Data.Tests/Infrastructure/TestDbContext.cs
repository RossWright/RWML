namespace RossWright.MetalCommand.Data.Tests.Infrastructure;

public class TestItem
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<TestItem> Items => Set<TestItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestItem>().HasKey(e => e.Id);
    }
}
