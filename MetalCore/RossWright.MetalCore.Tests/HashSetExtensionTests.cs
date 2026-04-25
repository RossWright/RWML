namespace RossWright;

public class HashSetExtensionTests
{
    // ── AddRange ──────────────────────────────────────────────────────────────────
    [Fact] public void AddRange_AddsNewItems_ReturnsTrueWhenAnyAdded()
    {
        var set = new HashSet<int> { 1, 2 };
        var result = set.AddRange(new[] { 3, 4 });
        result.ShouldBeTrue();
        set.ShouldContain(3);
        set.ShouldContain(4);
    }

    [Fact] public void AddRange_AllItemsAlreadyExist_ReturnsFalse()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        var result = set.AddRange(new[] { 1, 2, 3 });
        result.ShouldBeFalse();
        set.Count.ShouldBe(3);
    }

    [Fact] public void AddRange_SomePreviouslyExist_ReturnsTrueAndAddsMissing()
    {
        var set = new HashSet<int> { 1 };
        var result = set.AddRange(new[] { 1, 2 });
        result.ShouldBeTrue();
        set.Count.ShouldBe(2);
    }

    // ── RemoveRange ───────────────────────────────────────────────────────────────
    [Fact] public void RemoveRange_RemovesPresentItems_ReturnsTrueWhenAnyRemoved()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        var result = set.RemoveRange(new[] { 1, 2 });
        result.ShouldBeTrue();
        set.ShouldNotContain(1);
        set.ShouldNotContain(2);
        set.ShouldContain(3);
    }

    [Fact] public void RemoveRange_NonePresent_ReturnsFalse()
    {
        var set = new HashSet<int> { 1, 2 };
        var result = set.RemoveRange(new[] { 9, 10 });
        result.ShouldBeFalse();
        set.Count.ShouldBe(2);
    }

    [Fact] public void RemoveRange_SomePresentSomeNot_ReturnsTrueAndRemovesPresent()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        var result = set.RemoveRange(new[] { 2, 99 });
        result.ShouldBeTrue();
        set.ShouldNotContain(2);
        set.ShouldContain(1);
        set.ShouldContain(3);
    }
}
