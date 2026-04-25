using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace RossWright.Data.Tests;

// Test entities
class Product : IHasId
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}

class ProductDto : IHasId
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}

class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    public DbSet<Product> Products { get; set; } = null!;
}

public class RefreshTableTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task RefreshTable_NullNewData_EarlyReturn_NoChanges()
    {
        await using var ctx = CreateContext();
        ctx.Products.Add(new Product { Name = "Existing" });
        await ctx.SaveChangesAsync();

        var result = await ctx.Products.RefreshTable<Product, ProductDto>(null);

        result.Adds.ShouldBe(0);
        result.Updates.ShouldBe(0);
        result.Deletes.ShouldBe(0);
        (await ctx.Products.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task RefreshTable_EmptyOldData_AddsAllNewItems()
    {
        await using var ctx = CreateContext();
        var newData = new[] { new ProductDto { Name = "A" }, new ProductDto { Name = "B" } };

        var result = await ctx.Products.RefreshTable<Product, ProductDto>(newData);

        result.Adds.ShouldBe(2);
        result.Updates.ShouldBe(0);
        result.Deletes.ShouldBe(0);
    }

    [Fact]
    public async Task RefreshTable_MatchedRecords_UpdatesExisting()
    {
        await using var ctx = CreateContext();
        var id = Guid.NewGuid();
        ctx.Products.Add(new Product { Id = id, Name = "Old" });
        await ctx.SaveChangesAsync();

        var newData = new[] { new ProductDto { Id = id, Name = "New" } };

        var result = await ctx.Products.RefreshTable<Product, ProductDto>(newData, (db, dto) => db.Name = dto.Name);

        result.Adds.ShouldBe(0);
        result.Updates.ShouldBe(1);
        result.Deletes.ShouldBe(0);
    }

    [Fact]
    public async Task RefreshTable_DeleteSourceEntitiesTrue_RemovesAbsentRecords()
    {
        await using var ctx = CreateContext();
        var keepId = Guid.NewGuid();
        ctx.Products.Add(new Product { Id = keepId, Name = "Keep" });
        ctx.Products.Add(new Product { Name = "Delete" });
        await ctx.SaveChangesAsync();

        var newData = new[] { new ProductDto { Id = keepId, Name = "Keep" } };

        var result = await ctx.Products.RefreshTable<Product, ProductDto>(newData, (db, dto) => db.Name = dto.Name, deleteSourceEntities: true);
        await ctx.SaveChangesAsync();

        result.Deletes.ShouldBe(1);
        (await ctx.Products.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task RefreshTable_DeleteSourceEntitiesFalse_DoesNotRemoveAbsentRecords()
    {
        await using var ctx = CreateContext();
        var keepId = Guid.NewGuid();
        ctx.Products.Add(new Product { Id = keepId, Name = "Keep" });
        ctx.Products.Add(new Product { Name = "Stay" });
        await ctx.SaveChangesAsync();

        var newData = new[] { new ProductDto { Id = keepId, Name = "Keep" } };

        var result = await ctx.Products.RefreshTable<Product, ProductDto>(newData, (db, dto) => db.Name = dto.Name, deleteSourceEntities: false);

        result.Deletes.ShouldBe(0);
    }

    [Fact]
    public async Task RefreshTable_ZeroNetChange_ReturnsZeroForAllCounts()
    {
        await using var ctx = CreateContext();
        var id = Guid.NewGuid();
        ctx.Products.Add(new Product { Id = id, Name = "Same" });
        await ctx.SaveChangesAsync();

        var newData = new[] { new ProductDto { Id = id, Name = "Same" } };

        var result = await ctx.Products.RefreshTable<Product, ProductDto>(newData, (db, dto) => db.Name = dto.Name, deleteSourceEntities: false);

        result.Adds.ShouldBe(0);
        result.Deletes.ShouldBe(0);
        result.Updates.ShouldBe(1);
    }

    [Fact]
    public async Task RefreshTable_CustomFetchOldData_UsesProvidedDelegate()
    {
        await using var ctx = CreateContext();
        var id = Guid.NewGuid();
        ctx.Products.Add(new Product { Id = id, Name = "Old" });
        ctx.Products.Add(new Product { Name = "Excluded" });
        await ctx.SaveChangesAsync();

        // Custom fetch: only return the product with the known id
        Task<List<Product>> CustomFetch(DbSet<Product> set) =>
            set.Where(p => p.Id == id).ToListAsync();

        var newData = new[] { new ProductDto { Id = id, Name = "Updated" } };

        var result = await ctx.Products.RefreshTable(CustomFetch, newData, (db, dto) => db.Name = dto.Name);

        // Only 1 was fetched, 1 was updated
        result.Updates.ShouldBe(1);
        // The excluded product is not in scope so gets 0 deletes
        result.Deletes.ShouldBe(0);
    }

    [Fact]
    public async Task RefreshTable_CustomIsSamePredicate_MatchesOnCustomKey()
    {
        await using var ctx = CreateContext();
        ctx.Products.Add(new Product { Name = "Match" });
        await ctx.SaveChangesAsync();

        var newData = new[] { new ProductDto { Name = "Match" } };

        // Match by Name instead of Id
        bool CustomIsSame(Product db, ProductDto dto) => db.Name == dto.Name;

        var result = await ctx.Products.RefreshTable(newData, CustomIsSame, (db, dto) => db.Name = dto.Name);

        result.Adds.ShouldBe(0);
        result.Updates.ShouldBe(1);
    }
}
