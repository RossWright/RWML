namespace RossWright.MetalCore.Tests;

public class HashSetExtensionsTests
{
    // ── AddRange ──────────────────────────────────────────────────────────────────
    [Fact]
    public void AddRange_AddsNewItems_ReturnsTrueWhenAnyAdded()
    {
        var set = new HashSet<int> { 1, 2 };
        var result = set.AddRange(new[] { 3, 4 });
        result.ShouldBeTrue();
        set.ShouldContain(3);
        set.ShouldContain(4);
    }

    [Fact]
    public void AddRange_AllItemsAlreadyExist_ReturnsFalse()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        var result = set.AddRange(new[] { 1, 2, 3 });
        result.ShouldBeFalse();
        set.Count.ShouldBe(3);
    }

    [Fact]
    public void AddRange_SomePreviouslyExist_ReturnsTrueAndAddsMissing()
    {
        var set = new HashSet<int> { 1 };
        var result = set.AddRange(new[] { 1, 2 });
        result.ShouldBeTrue();
        set.Count.ShouldBe(2);
    }

    [Fact]
    public void AddRange_EmptyItems_ReturnsFalse()
    {
        var set = new HashSet<int> { 1, 2 };
        var result = set.AddRange(Array.Empty<int>());
        result.ShouldBeFalse();
        set.Count.ShouldBe(2);
    }

    [Fact]
    public void AddRange_EmptySet_AddsAllItemsAndReturnsTrue()
    {
        var set = new HashSet<int>();
        var result = set.AddRange(new[] { 1, 2, 3 });
        result.ShouldBeTrue();
        set.Count.ShouldBe(3);
    }

    [Fact]
    public void AddRange_WithStrings_AddsNewItemsCorrectly()
    {
        var set = new HashSet<string> { "a", "b" };
        var result = set.AddRange(new[] { "c", "d" });
        result.ShouldBeTrue();
        set.ShouldContain("c");
        set.ShouldContain("d");
    }

    [Fact]
    public void AddRange_WithDuplicatesInInput_ReturnsTrue()
    {
        var set = new HashSet<int> { 1 };
        var result = set.AddRange(new[] { 2, 2, 2 });
        result.ShouldBeTrue();
        set.Count.ShouldBe(2);
        set.ShouldContain(2);
    }

    [Fact]
    public void AddRange_WithSingleItem_AddsCorrectly()
    {
        var set = new HashSet<int> { 1 };
        var result = set.AddRange(new[] { 2 });
        result.ShouldBeTrue();
        set.Count.ShouldBe(2);
    }

    // ── RemoveRange ───────────────────────────────────────────────────────────────
    [Fact]
    public void RemoveRange_RemovesPresentItems_ReturnsTrueWhenAnyRemoved()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        var result = set.RemoveRange(new[] { 1, 2 });
        result.ShouldBeTrue();
        set.ShouldNotContain(1);
        set.ShouldNotContain(2);
        set.ShouldContain(3);
    }

    [Fact]
    public void RemoveRange_NonePresent_ReturnsFalse()
    {
        var set = new HashSet<int> { 1, 2 };
        var result = set.RemoveRange(new[] { 9, 10 });
        result.ShouldBeFalse();
        set.Count.ShouldBe(2);
    }

    [Fact]
    public void RemoveRange_SomePresentSomeNot_ReturnsTrueAndRemovesPresent()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        var result = set.RemoveRange(new[] { 2, 99 });
        result.ShouldBeTrue();
        set.ShouldNotContain(2);
        set.ShouldContain(1);
        set.ShouldContain(3);
    }

    [Fact]
    public void RemoveRange_EmptyItems_ReturnsFalse()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        var result = set.RemoveRange(Array.Empty<int>());
        result.ShouldBeFalse();
        set.Count.ShouldBe(3);
    }

    [Fact]
    public void RemoveRange_EmptySet_ReturnsFalse()
    {
        var set = new HashSet<int>();
        var result = set.RemoveRange(new[] { 1, 2 });
        result.ShouldBeFalse();
        set.Count.ShouldBe(0);
    }

    [Fact]
    public void RemoveRange_WithStrings_RemovesPresentItemsCorrectly()
    {
        var set = new HashSet<string> { "a", "b", "c" };
        var result = set.RemoveRange(new[] { "a", "b" });
        result.ShouldBeTrue();
        set.ShouldNotContain("a");
        set.ShouldNotContain("b");
        set.ShouldContain("c");
    }

    [Fact]
    public void RemoveRange_WithDuplicatesInInput_ReturnsTrue()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        var result = set.RemoveRange(new[] { 2, 2, 2 });
        result.ShouldBeTrue();
        set.Count.ShouldBe(2);
        set.ShouldNotContain(2);
    }

    [Fact]
    public void RemoveRange_WithSingleItem_RemovesCorrectly()
    {
        var set = new HashSet<int> { 1, 2 };
        var result = set.RemoveRange(new[] { 1 });
        result.ShouldBeTrue();
        set.Count.ShouldBe(1);
        set.ShouldNotContain(1);
    }
}
