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

    [Fact]
    public async Task RefreshTable_CustomIsSameWithCopyTo_UsesCopyToForUpdate()
    {
        await using var ctx = CreateContext();
        ctx.Products.Add(new Product { Name = "Match" });
        await ctx.SaveChangesAsync();

        var newData = new[] { new ProductDto { Name = "Match" } };

        // Match by Name instead of Id, use CopyTo for updates
        bool CustomIsSame(Product db, ProductDto dto) => db.Name == dto.Name;

        var result = await ctx.Products.RefreshTable(newData, CustomIsSame);

        result.Adds.ShouldBe(0);
        result.Updates.ShouldBe(1);
    }

    [Fact]
    public async Task RefreshTable_CustomIsSameWithCopyTo_AddsNewItems()
    {
        await using var ctx = CreateContext();

        var newData = new[] { new ProductDto { Name = "NewProduct" } };

        // Match by Name
        bool CustomIsSame(Product db, ProductDto dto) => db.Name == dto.Name;

        var result = await ctx.Products.RefreshTable(newData, CustomIsSame);

        result.Adds.ShouldBe(1);
        result.Updates.ShouldBe(0);
        result.Deletes.ShouldBe(0);
    }

    [Fact]
    public async Task RefreshTable_CustomIsSameWithCopyTo_DeletesWhenEnabled()
    {
        await using var ctx = CreateContext();
        ctx.Products.Add(new Product { Name = "ToDelete" });
        await ctx.SaveChangesAsync();

        var newData = Array.Empty<ProductDto>();

        // Match by Name
        bool CustomIsSame(Product db, ProductDto dto) => db.Name == dto.Name;

        var result = await ctx.Products.RefreshTable(newData, CustomIsSame, deleteSourceEntities: true);

        result.Adds.ShouldBe(0);
        result.Updates.ShouldBe(0);
        result.Deletes.ShouldBe(1);
    }

    [Fact]
    public async Task RefreshTable_CustomIsSameWithCopyTo_NullNewData_NoChanges()
    {
        await using var ctx = CreateContext();
        ctx.Products.Add(new Product { Name = "Existing" });
        await ctx.SaveChangesAsync();

        // Match by Name
        bool CustomIsSame(Product db, ProductDto dto) => db.Name == dto.Name;

        var result = await ctx.Products.RefreshTable<Product, ProductDto>(null, CustomIsSame);

        result.Adds.ShouldBe(0);
        result.Updates.ShouldBe(0);
        result.Deletes.ShouldBe(0);
    }

    [Fact]
    public async Task RefreshTable_BasicOverloadWithCopyTo_UpdatesProperties()
    {
        await using var ctx = CreateContext();
        var id = Guid.NewGuid();
        ctx.Products.Add(new Product { Id = id, Name = "OldName" });
        await ctx.SaveChangesAsync();

        var newData = new[] { new ProductDto { Id = id, Name = "NewName" } };

        var result = await ctx.Products.RefreshTable<Product, ProductDto>(newData);

        result.Adds.ShouldBe(0);
        result.Updates.ShouldBe(1);
        result.Deletes.ShouldBe(0);
        var updated = await ctx.Products.FindAsync(id);
        updated!.Name.ShouldBe("NewName");
    }

    [Fact]
    public async Task RefreshTable_BasicOverloadWithCopyTo_DeletesWhenEnabled()
    {
        await using var ctx = CreateContext();
        var keepId = Guid.NewGuid();
        ctx.Products.Add(new Product { Id = keepId, Name = "Keep" });
        ctx.Products.Add(new Product { Name = "Delete" });
        await ctx.SaveChangesAsync();

        var newData = new[] { new ProductDto { Id = keepId, Name = "Keep" } };

        var result = await ctx.Products.RefreshTable<Product, ProductDto>(newData, deleteSourceEntities: true);

        result.Adds.ShouldBe(0);
        result.Updates.ShouldBe(1);
        result.Deletes.ShouldBe(1);
    }

    [Fact]
    public async Task RefreshTable_CustomFetchWithNullData_NoChanges()
    {
        await using var ctx = CreateContext();
        ctx.Products.Add(new Product { Name = "Existing" });
        await ctx.SaveChangesAsync();

        Task<List<Product>> CustomFetch(DbSet<Product> set) => set.ToListAsync();

        var result = await ctx.Products.RefreshTable<Product, ProductDto>(CustomFetch, null, (db, dto) => db.Name = dto.Name);

        result.Adds.ShouldBe(0);
        result.Updates.ShouldBe(0);
        result.Deletes.ShouldBe(0);
    }

    [Fact]
    public async Task RefreshTable_CustomFetchWithDelete_DeletesAbsentRecords()
    {
        await using var ctx = CreateContext();
        var id = Guid.NewGuid();
        ctx.Products.Add(new Product { Id = id, Name = "Keep" });
        ctx.Products.Add(new Product { Name = "Delete" });
        await ctx.SaveChangesAsync();

        Task<List<Product>> CustomFetch(DbSet<Product> set) => set.ToListAsync();

        var newData = new[] { new ProductDto { Id = id, Name = "Keep" } };

        var result = await ctx.Products.RefreshTable(CustomFetch, newData, (db, dto) => db.Name = dto.Name, deleteSourceEntities: true);

        result.Adds.ShouldBe(0);
        result.Updates.ShouldBe(1);
        result.Deletes.ShouldBe(1);
    }

    [Fact]
    public async Task RefreshTable_CustomIsSame_EmptyOldData_AddsAll()
    {
        await using var ctx = CreateContext();

        var newData = new[] { new ProductDto { Name = "NewItem" } };

        bool CustomIsSame(Product db, ProductDto dto) => db.Name == dto.Name;

        var result = await ctx.Products.RefreshTable(newData, CustomIsSame, (db, dto) => db.Name = dto.Name);

        result.Adds.ShouldBe(1);
        result.Updates.ShouldBe(0);
        result.Deletes.ShouldBe(0);
    }

    [Fact]
    public async Task RefreshTable_MixedOperations_AddsUpdatesAndDeletes()
    {
        await using var ctx = CreateContext();
        var updateId = Guid.NewGuid();
        var deleteId = Guid.NewGuid();
        ctx.Products.Add(new Product { Id = updateId, Name = "ToUpdate" });
        ctx.Products.Add(new Product { Id = deleteId, Name = "ToDelete" });
        await ctx.SaveChangesAsync();

        var newData = new[]
        {
            new ProductDto { Id = updateId, Name = "Updated" },
            new ProductDto { Name = "ToAdd" }
        };

        var result = await ctx.Products.RefreshTable<Product, ProductDto>(newData, (db, dto) => db.Name = dto.Name, deleteSourceEntities: true);

        result.Adds.ShouldBe(1);
        result.Updates.ShouldBe(1);
        result.Deletes.ShouldBe(1);
    }

    [Fact]
    public async Task RefreshTable_CustomIsSameWithUpdate_MultipleMatches()
    {
        await using var ctx = CreateContext();
        ctx.Products.Add(new Product { Name = "Product" });
        ctx.Products.Add(new Product { Name = "Product" });
        await ctx.SaveChangesAsync();

        var newData = new[] { new ProductDto { Name = "Product" } };

        bool CustomIsSame(Product db, ProductDto dto) => db.Name == dto.Name;

        var result = await ctx.Products.RefreshTable(newData, CustomIsSame, (db, dto) => { });

        result.Adds.ShouldBe(0);
        result.Updates.ShouldBe(2);
        result.Deletes.ShouldBe(0);
    }

    [Fact]
    public async Task RefreshTable_EmptyNewData_NoDeletes_WhenDeleteSourceEntitiesFalse()
    {
        await using var ctx = CreateContext();
        ctx.Products.Add(new Product { Name = "Existing" });
        await ctx.SaveChangesAsync();

        var newData = Array.Empty<ProductDto>();

        var result = await ctx.Products.RefreshTable<Product, ProductDto>(newData, deleteSourceEntities: false);

        result.Adds.ShouldBe(0);
        result.Updates.ShouldBe(0);
        result.Deletes.ShouldBe(0);
        (await ctx.Products.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task RefreshTable_CustomFetchWithIsSame_AddsNewItemsWhenOldDataExists()
    {
        await using var ctx = CreateContext();
        var existingId = Guid.NewGuid();
        ctx.Products.Add(new Product { Id = existingId, Name = "Existing" });
        await ctx.SaveChangesAsync();

        var newId = Guid.NewGuid();
        var newData = new[]
        {
            new ProductDto { Id = existingId, Name = "Updated" },
            new ProductDto { Id = newId, Name = "New" }
        };

        Task<List<Product>> CustomFetch(DbSet<Product> set) => set.ToListAsync();
        bool CustomIsSame(Product db, ProductDto dto) => db.Id == dto.Id;

        var result = await ctx.Products.RefreshTable(CustomFetch, newData, CustomIsSame, (db, dto) => db.Name = dto.Name);

        result.Adds.ShouldBe(1);
        result.Updates.ShouldBe(1);
        result.Deletes.ShouldBe(0);
    }

    [Fact]
    public async Task RefreshTable_CustomFetchWithIsSame_AddsMultipleNewItems()
    {
        await using var ctx = CreateContext();
        var existingId = Guid.NewGuid();
        ctx.Products.Add(new Product { Id = existingId, Name = "Existing" });
        await ctx.SaveChangesAsync();

        var newData = new[]
        {
            new ProductDto { Id = existingId, Name = "Updated" },
            new ProductDto { Id = Guid.NewGuid(), Name = "New1" },
            new ProductDto { Id = Guid.NewGuid(), Name = "New2" },
            new ProductDto { Id = Guid.NewGuid(), Name = "New3" }
        };

        Task<List<Product>> CustomFetch(DbSet<Product> set) => set.ToListAsync();
        bool CustomIsSame(Product db, ProductDto dto) => db.Id == dto.Id;

        var result = await ctx.Products.RefreshTable(CustomFetch, newData, CustomIsSame, (db, dto) => db.Name = dto.Name);

        result.Adds.ShouldBe(3);
        result.Updates.ShouldBe(1);
        result.Deletes.ShouldBe(0);
    }

    [Fact]
    public async Task RefreshTable_CustomFetchWithIsSame_AddsOnlyNewItems()
    {
        await using var ctx = CreateContext();
        ctx.Products.Add(new Product { Id = Guid.NewGuid(), Name = "Existing1" });
        ctx.Products.Add(new Product { Id = Guid.NewGuid(), Name = "Existing2" });
        await ctx.SaveChangesAsync();

        var newData = new[]
        {
            new ProductDto { Id = Guid.NewGuid(), Name = "New1" },
            new ProductDto { Id = Guid.NewGuid(), Name = "New2" }
        };

        Task<List<Product>> CustomFetch(DbSet<Product> set) => set.ToListAsync();
        bool CustomIsSame(Product db, ProductDto dto) => db.Id == dto.Id;

        var result = await ctx.Products.RefreshTable(CustomFetch, newData, CustomIsSame, (db, dto) => db.Name = dto.Name);

        result.Adds.ShouldBe(2);
        result.Updates.ShouldBe(0);
        result.Deletes.ShouldBe(0);
    }
}
