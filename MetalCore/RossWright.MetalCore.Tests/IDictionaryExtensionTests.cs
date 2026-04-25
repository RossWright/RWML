using Shouldly;

namespace RossWright;

public class IDictionaryExtensionTests
{
    // ── GetValueOrDefault ─────────────────────────────────────────────────────────
    [Fact] public void GetValueOrDefault_KeyPresent_ReturnsValue()
    {
        var dict = new Dictionary<string, int> { ["a"] = 42 };
        dict.GetValueOrDefault("a").ShouldBe(42);
    }

    [Fact] public void GetValueOrDefault_KeyAbsent_ReturnsDefaultDefault()
    {
        var dict = new Dictionary<string, int>();
        dict.GetValueOrDefault("missing").ShouldBe(0);
    }

    [Fact] public void GetValueOrDefault_KeyAbsent_ReturnsSuppliedDefault()
    {
        var dict = new Dictionary<string, int>();
        dict.GetValueOrDefault("missing", 99).ShouldBe(99);
    }

    // ── WithoutKey(TKey) ─────────────────────────────────────────────────────────
    [Fact] public void WithoutKeyTKey_RemovesKey()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var result = dict.WithoutKey("a");
        result.ShouldNotContainKey("a");
        result.ShouldContainKey("b");
    }

    [Fact] public void WithoutKeyTKey_KeyNotPresent_ReturnsOriginalContents()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };
        var result = dict.WithoutKey("z");
        result.Count.ShouldBe(1);
    }

    [Fact] public void WithoutKeyTKey_OriginalDictUnchanged()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        _ = dict.WithoutKey("a");
        dict.ShouldContainKey("a");
    }

    // ── WithoutKey(Func<TKey,bool>) ────────────────────────────────────────────────
    [Fact] public void WithoutKeyPredicate_RemovesMatchingKeys()
    {
        var dict = new Dictionary<string, int> { ["aa"] = 1, ["b"] = 2, ["cc"] = 3 };
        var result = dict.WithoutKey(k => k.Length > 1);
        result.Count.ShouldBe(1);
        result.ShouldContainKey("b");
    }

    [Fact] public void WithoutKeyPredicate_NoMatch_ReturnsCopy()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };
        var result = dict.WithoutKey(k => k.Length > 10);
        result.Count.ShouldBe(1);
    }

    // ── Without(Func<TKey,TValue,bool>) ──────────────────────────────────────────
    [Fact] public void Without_RemovesMatchingEntries()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 100, ["c"] = 3 };
        var result = dict.Without((k, v) => v > 50);
        result.Count.ShouldBe(2);
        result.ShouldNotContainKey("b");
    }

    // ── ToDictionary(IEnumerable<KVP>) ────────────────────────────────────────────
    [Fact] public void ToDictionary_FromKvpEnumerable_BuildsDictionary()
    {
        var kvps = new[]
        {
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("b", 2)
        };
        var result = kvps.ToDictionary();
        result["a"].ShouldBe(1);
        result["b"].ShouldBe(2);
    }

    // ── CopyTo ────────────────────────────────────────────────────────────────────
    [Fact] public void CopyTo_CopiesAllEntries()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        IDictionary<string, int> target = new Dictionary<string, int>();
        source.CopyTo(target);
        target["a"].ShouldBe(1);
        target["b"].ShouldBe(2);
    }

    [Fact] public void CopyTo_OverwritesExistingKeys()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { ["a"] = 99 };
        IDictionary<string, int> target = new Dictionary<string, int> { ["a"] = 1 };
        source.CopyTo(target);
        target["a"].ShouldBe(99);
    }

    // ── RemoveWhere ───────────────────────────────────────────────────────────────
    [Fact] public void RemoveWhere_RemovesMatchingEntries()
    {
        IDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
        dict.RemoveWhere((k, v) => v > 1);
        dict.Count.ShouldBe(1);
        dict.ShouldContainKey("a");
    }

    [Fact] public void RemoveWhere_NoMatch_DictUnchanged()
    {
        IDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1 };
        dict.RemoveWhere((k, v) => v > 100);
        dict.Count.ShouldBe(1);
    }

    // ── AddToList / GetList / RemoveFromList ───────────────────────────────────────
    [Fact] public void AddToList_NewKey_CreatesListWithValue()
    {
        IDictionary<string, IList<int>> dict = new Dictionary<string, IList<int>>();
        dict.AddToList("a", 1);
        dict["a"].ShouldBe(new[] { 1 });
    }

    [Fact] public void AddToList_ExistingKey_AppendsValue()
    {
        IDictionary<string, IList<int>> dict = new Dictionary<string, IList<int>>();
        dict.AddToList("a", 1);
        dict.AddToList("a", 2);
        dict["a"].ShouldBe(new[] { 1, 2 });
    }

    [Fact] public void GetList_KeyPresent_ReturnsList()
    {
        IDictionary<string, IList<int>> dict = new Dictionary<string, IList<int>>();
        dict.AddToList("a", 42);
        dict.GetList("a").ShouldBe(new[] { 42 });
    }

    [Fact] public void GetList_KeyAbsent_ReturnsNull()
    {
        IDictionary<string, IList<int>> dict = new Dictionary<string, IList<int>>();
        dict.GetList("missing").ShouldBeNull();
    }

    [Fact] public void RemoveFromList_RemovesValue()
    {
        IDictionary<string, IList<int>> dict = new Dictionary<string, IList<int>>();
        dict.AddToList("a", 1);
        dict.AddToList("a", 2);
        dict.RemoveFromList("a", 1);
        dict.GetList("a").ShouldBe(new[] { 2 });
    }

    [Fact] public void RemoveFromList_KeyAbsent_DoesNotThrow()
    {
        IDictionary<string, IList<int>> dict = new Dictionary<string, IList<int>>();
        dict.RemoveFromList("missing", 1);
    }

    [Fact] public void RemoveFromList_LastItem_GetListReturnsNull()
    {
        IDictionary<string, IList<int>> dict = new Dictionary<string, IList<int>>();
        dict.AddToList("a", 42);
        dict.RemoveFromList("a", 42);
        dict.GetList("a").ShouldBeNull();
    }
}

// ── P2-A-i: AnyInAnyList on IDictionary<TKey, IList<TValue>> ─────────────────
public class AnyInAnyListTests
{
    [Fact] public void AnyInAnyList_WhenAnyListHasValues_ReturnsTrue()
    {
        IDictionary<string, IList<int>> dict = new Dictionary<string, IList<int>> { ["a"] = new List<int> { 1 } };
        dict.AnyInAnyList().ShouldBeTrue();
    }

    [Fact] public void AnyInAnyList_WhenDictEmpty_ReturnsFalse()
    {
        IDictionary<string, IList<int>> dict = new Dictionary<string, IList<int>>();
        dict.AnyInAnyList().ShouldBeFalse();
    }

    [Fact] public void AnyInAnyList_WhenAllListsEmpty_ReturnsFalse()
    {
        IDictionary<string, IList<int>> dict = new Dictionary<string, IList<int>> { ["a"] = new List<int>() };
        dict.AnyInAnyList().ShouldBeFalse();
    }

    [Fact] public void AnyInAnyList_Predicate_MatchFound_ReturnsTrue()
    {
        IDictionary<string, IList<int>> dict = new Dictionary<string, IList<int>> { ["a"] = new List<int> { 1 } };
        dict.AnyInAnyList(x => x == 1).ShouldBeTrue();
    }

    [Fact] public void AnyInAnyList_Predicate_NoMatch_ReturnsFalse()
    {
        IDictionary<string, IList<int>> dict = new Dictionary<string, IList<int>> { ["a"] = new List<int> { 1 } };
        dict.AnyInAnyList(x => x == 99).ShouldBeFalse();
    }
}

// ── P2-A-ii: Dictionary<TKey, List<TValue>> ───────────────────────────────────
public class DictionaryListTests
{
    [Fact] public void AddToList_Dict_NewKey_CreatesListAndAddsItem()
    {
        var dict = new Dictionary<string, List<int>>();
        dict.AddToList("a", 1);
        dict["a"].ShouldBe(new[] { 1 });
    }

    [Fact] public void AddToList_Dict_ExistingKey_AppendsItem()
    {
        var dict = new Dictionary<string, List<int>> { ["a"] = new List<int> { 1 } };
        dict.AddToList("a", 2);
        dict["a"].ShouldBe(new[] { 1, 2 });
    }

    [Fact] public void GetList_Dict_KeyPresent_ReturnsValues()
    {
        var dict = new Dictionary<string, List<int>> { ["a"] = new List<int> { 1, 2 } };
        dict.GetList("a").ShouldBe(new[] { 1, 2 });
    }

    [Fact] public void GetList_Dict_KeyAbsent_ReturnsNull()
    {
        var dict = new Dictionary<string, List<int>>();
        dict.GetList("a").ShouldBeNull();
    }

    [Fact] public void RemoveFromList_Dict_ValueExists_RemovesItem()
    {
        var dict = new Dictionary<string, List<int>> { ["a"] = new List<int> { 1, 2 } };
        dict.RemoveFromList("a", 1);
        dict["a"].ShouldBe(new[] { 2 });
    }

    [Fact] public void RemoveFromList_Dict_ValueMissing_ListUnchanged()
    {
        var dict = new Dictionary<string, List<int>> { ["a"] = new List<int> { 1 } };
        dict.RemoveFromList("a", 2);
        dict["a"].ShouldBe(new[] { 1 });
    }

    [Fact] public void RemoveFromList_Dict_KeyMissing_DoesNotThrow()
    {
        var dict = new Dictionary<string, List<int>>();
        dict.RemoveFromList("b", 1);
        dict.ShouldBeEmpty();
    }

    [Fact] public void AnyInAnyList_Dict_AnyListHasValues_ReturnsTrue()
    {
        var dict = new Dictionary<string, List<int>> { ["a"] = new List<int> { 1 } };
        dict.AnyInAnyList().ShouldBeTrue();
    }

    [Fact] public void AnyInAnyList_Dict_Predicate_MatchFound_ReturnsTrue()
    {
        var dict = new Dictionary<string, List<int>> { ["a"] = new List<int> { 1 } };
        dict.AnyInAnyList(x => x == 1).ShouldBeTrue();
    }
}

// ── P2-A-iii: Dictionary<TKey, HashSet<TValue>> ───────────────────────────────
public class DictionaryHashSetTests
{
    [Fact] public void AddToSet_NewKey_CreatesSetAndAddsItem()
    {
        var dict = new Dictionary<string, HashSet<int>>();
        dict.AddToSet("a", 1);
        dict["a"].ShouldContain(1);
    }

    [Fact] public void AddToSet_ExistingKey_AddsItemToSet()
    {
        var dict = new Dictionary<string, HashSet<int>> { ["a"] = new HashSet<int> { 1 } };
        dict.AddToSet("a", 2);
        dict["a"].ShouldContain(1);
        dict["a"].ShouldContain(2);
    }

    [Fact] public void AddToSet_DuplicateValue_SetDoesNotGrow()
    {
        var dict = new Dictionary<string, HashSet<int>> { ["a"] = new HashSet<int> { 1 } };
        dict.AddToSet("a", 1);
        dict["a"].ShouldContain(1);
    }

    [Fact] public void GetSet_KeyPresent_ReturnsSet()
    {
        var dict = new Dictionary<string, HashSet<int>> { ["a"] = new HashSet<int> { 1, 2 } };
        var result = dict.GetSet("a")!;
        result.ShouldContain(1);
        result.ShouldContain(2);
    }

    [Fact] public void GetSet_KeyAbsent_ReturnsNull()
    {
        var dict = new Dictionary<string, HashSet<int>>();
        dict.GetSet("a").ShouldBeNull();
    }

    [Fact] public void RemoveFromSet_ValueExists_RemovesItem()
    {
        var dict = new Dictionary<string, HashSet<int>> { ["a"] = new HashSet<int> { 1, 2 } };
        var result = dict.RemoveFromSet("a", 1)!;
        result.ShouldNotContain(1);
        result.ShouldContain(2);
    }

    [Fact] public void RemoveFromSet_ValueMissing_SetUnchanged()
    {
        var dict = new Dictionary<string, HashSet<int>> { ["a"] = new HashSet<int> { 1 } };
        var result = dict.RemoveFromSet("a", 2)!;
        result.ShouldContain(1);
    }

    [Fact] public void RemoveFromSet_KeyMissing_ReturnsNull()
    {
        var dict = new Dictionary<string, HashSet<int>>();
        dict.RemoveFromSet("b", 1).ShouldBeNull();
    }

    [Fact] public void AnyInAnySet_AnySetHasValues_ReturnsTrue()
    {
        var dict = new Dictionary<string, HashSet<int>> { ["a"] = new HashSet<int> { 1 } };
        dict.AnyInAnySet().ShouldBeTrue();
    }

    [Fact] public void AnyInAnySet_DictEmpty_ReturnsFalse()
    {
        var dict = new Dictionary<string, HashSet<int>>();
        dict.AnyInAnySet().ShouldBeFalse();
    }
}

// ── P2-A-iv: CopyTo(Dictionary, Dictionary) ───────────────────────────────────
public class CopyToDictionaryTests
{
    [Fact] public void CopyTo_Dictionary_CopiesAllEntriesToDestination()
    {
        var source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var dest = new Dictionary<string, int>();
        source.CopyTo(dest);
        dest["a"].ShouldBe(1);
        dest["b"].ShouldBe(2);
    }

    [Fact] public void CopyTo_Dictionary_ExistingDestKey_OverwritesValue()
    {
        var source = new Dictionary<string, int> { ["a"] = 2 };
        var dest = new Dictionary<string, int> { ["a"] = 1 };
        source.CopyTo(dest);
        dest["a"].ShouldBe(2);
    }

    [Fact] public void CopyTo_Dictionary_EmptySource_DestinationUnchanged()
    {
        var source = new Dictionary<string, int>();
        var dest = new Dictionary<string, int> { ["a"] = 1 };
        source.CopyTo(dest);
        dest["a"].ShouldBe(1);
        dest.Count.ShouldBe(1);
    }
}
