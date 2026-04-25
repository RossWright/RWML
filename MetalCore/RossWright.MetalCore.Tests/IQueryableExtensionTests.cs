namespace RossWright;

public class IQueryableExtensionTests
{
    private sealed record TwoField(int A, int B);

    [Fact]
    public void Skip_WithNullCount_ReturnsAllItems()
    {
        var result = new[] { 1, 2, 3 }.AsQueryable().Skip((int?)null).ToList();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public void Skip_WithCount_SkipsItems()
    {
        var result = new[] { 1, 2, 3 }.AsQueryable().Skip((int?)2).ToList();
        result.ShouldBe(new[] { 3 });
    }

    [Fact]
    public void Skip_WithZeroCount_ReturnsAllItems()
    {
        var result = new[] { 1, 2, 3 }.AsQueryable().Skip((int?)0).ToList();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public void Take_WithNullCount_ReturnsAllItems()
    {
        var result = new[] { 1, 2, 3 }.AsQueryable().Take((int?)null).ToList();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public void Take_WithCount_TakesItems()
    {
        var result = new[] { 1, 2, 3 }.AsQueryable().Take((int?)2).ToList();
        result.ShouldBe(new[] { 1, 2 });
    }

    [Fact]
    public void WhereIf_WhenConditionTrue_FiltersItems()
    {
        var result = new[] { 1, 2, 3 }.AsQueryable().WhereIf(true, x => x > 1).ToList();
        result.ShouldBe(new[] { 2, 3 });
    }

    [Fact]
    public void WhereIf_WhenConditionFalse_ReturnsAllItems()
    {
        var result = new[] { 1, 2, 3 }.AsQueryable().WhereIf(false, x => x > 1).ToList();
        result.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void WhereIfNotNull_WhenValueIsNull_ReturnsAllItems()
    {
        var result = new[] { 1, 2, 3 }.AsQueryable().WhereIfNotNull(null, x => x > 1).ToList();
        result.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void WhereIfNotNull_WhenValueIsNotNull_FiltersItems()
    {
        var result = new[] { 1, 2, 3 }.AsQueryable().WhereIfNotNull("any", x => x > 1).ToList();
        result.ShouldBe(new[] { 2, 3 });
    }

    [Fact]
    public void WhereIfNotNullOrEmpty_WhenValueIsNull_ReturnsAllItems()
    {
        var result = new[] { 1, 2, 3 }.AsQueryable().WhereIfNotNullOrEmpty(null!, x => x > 1).ToList();
        result.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void WhereIfNotNullOrEmpty_WhenValueIsEmpty_ReturnsAllItems()
    {
        var result = new[] { 1, 2, 3 }.AsQueryable().WhereIfNotNullOrEmpty(new List<int>(), x => x > 1).ToList();
        result.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void WhereIfNotNullOrEmpty_WhenValueIsNotEmpty_FiltersItems()
    {
        var result = new[] { 1, 2, 3 }.AsQueryable().WhereIfNotNullOrEmpty(new List<string> { "x" }, x => x > 1).ToList();
        result.ShouldBe(new[] { 2, 3 });
    }

    [Fact]
    public void OrderBy_WhenAscendingTrue_SortsAscending()
    {
        var result = new[] { 3, 1, 2 }.AsQueryable().OrderBy(x => x, true).ToList();
        result.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void OrderBy_WhenAscendingFalse_SortsDescending()
    {
        var result = new[] { 3, 1, 2 }.AsQueryable().OrderBy(x => x, false).ToList();
        result.ShouldBe(new[] { 3, 2, 1 });
    }

    [Fact]
    public void ThenBy_WhenAscendingTrue_SortsSecondaryAscending()
    {
        var items = new[] { new TwoField(1, 3), new TwoField(1, 1), new TwoField(1, 2) }.AsQueryable();
        var result = items.OrderBy(x => x.A, true).ThenBy(x => x.B, true).ToList();
        result.Select(x => x.B).ToArray().ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void ThenBy_WhenAscendingFalse_SortsSecondaryDescending()
    {
        var items = new[] { new TwoField(1, 3), new TwoField(1, 1), new TwoField(1, 2) }.AsQueryable();
        var result = items.OrderBy(x => x.A, true).ThenBy(x => x.B, false).ToList();
        result.Select(x => x.B).ToArray().ShouldBe(new[] { 3, 2, 1 });
    }
}
