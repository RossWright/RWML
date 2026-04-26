using Shouldly;

namespace RossWright.MetalCore.Tests;

public class IDictionaryExtensionsTests
{
    [Fact]
    public void GetValueOrDefault_KeyPresent_ReturnsValue()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["key1"] = 42 };

        // Act
        var result = dict.GetValueOrDefault("key1");

        // Assert
        result.ShouldBe(42);
    }

    [Fact]
    public void GetValueOrDefault_KeyAbsent_ReturnsDefaultValue()
    {
        // Arrange
        var dict = new Dictionary<string, int>();

        // Act
        var result = dict.GetValueOrDefault("missing");

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public void GetValueOrDefault_KeyAbsent_ReturnsSuppliedDefault()
    {
        // Arrange
        var dict = new Dictionary<string, int>();

        // Act
        var result = dict.GetValueOrDefault("missing", 99);

        // Assert
        result.ShouldBe(99);
    }

    [Fact]
    public void GetValueOrDefault_NullableType_KeyPresent_ReturnsValue()
    {
        // Arrange
        var dict = new Dictionary<string, int?> { ["key1"] = 42 };

        // Act
        var result = dict.GetValueOrDefault("key1");

        // Assert
        result.ShouldBe(42);
    }

    [Fact]
    public void GetValueOrDefault_NullableType_KeyAbsent_ReturnsNull()
    {
        // Arrange
        var dict = new Dictionary<string, int?>();

        // Act
        var result = dict.GetValueOrDefault("missing");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetValueOrDefault_ReferenceType_KeyPresent_ReturnsValue()
    {
        // Arrange
        var dict = new Dictionary<string, string> { ["key1"] = "value1" };

        // Act
        var result = dict.GetValueOrDefault("key1");

        // Assert
        result.ShouldBe("value1");
    }

    [Fact]
    public void GetValueOrDefault_ReferenceType_KeyAbsent_ReturnsNull()
    {
        // Arrange
        var dict = new Dictionary<string, string>();

        // Act
        var result = dict.GetValueOrDefault("missing");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetValueOrDefault_ReferenceType_KeyPresentWithNullValue_ReturnsNull()
    {
        // Arrange
        var dict = new Dictionary<string, string?> { ["key1"] = null };

        // Act
        var result = dict.GetValueOrDefault("key1");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void WithoutKey_RemovesSpecifiedKey()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

        // Act
        var result = dict.WithoutKey("b");

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContainKey("a");
        result.ShouldNotContainKey("b");
        result.ShouldContainKey("c");
    }

    [Fact]
    public void WithoutKey_KeyNotPresent_ReturnsAllEntries()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        // Act
        var result = dict.WithoutKey("z");

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContainKey("a");
        result.ShouldContainKey("b");
    }

    [Fact]
    public void WithoutKey_OriginalDictUnmodified()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        // Act
        var result = dict.WithoutKey("a");

        // Assert
        dict.Count.ShouldBe(2);
        dict.ShouldContainKey("a");
        dict.ShouldContainKey("b");
    }

    [Fact]
    public void WithoutKey_EmptyDict_ReturnsEmptyDict()
    {
        // Arrange
        var dict = new Dictionary<string, int>();

        // Act
        var result = dict.WithoutKey("any");

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void WithoutKey_RemovingOnlyKey_ReturnsEmptyDict()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1 };

        // Act
        var result = dict.WithoutKey("a");

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void WithoutKey_Predicate_RemovesMatchingKeys()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["aa"] = 1, ["b"] = 2, ["cc"] = 3, ["ddd"] = 4 };

        // Act
        var result = dict.WithoutKey(k => k.Length > 1);

        // Assert
        result.Count.ShouldBe(1);
        result.ShouldContainKey("b");
        result.ShouldNotContainKey("aa");
        result.ShouldNotContainKey("cc");
        result.ShouldNotContainKey("ddd");
    }

    [Fact]
    public void WithoutKey_Predicate_NoMatch_ReturnsAllEntries()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        // Act
        var result = dict.WithoutKey(k => k.Length > 10);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContainKey("a");
        result.ShouldContainKey("b");
    }

    [Fact]
    public void WithoutKey_Predicate_AllMatch_ReturnsEmptyDict()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        // Act
        var result = dict.WithoutKey(k => k.Length == 1);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void WithoutKey_Predicate_EmptyDict_ReturnsEmptyDict()
    {
        // Arrange
        var dict = new Dictionary<string, int>();

        // Act
        var result = dict.WithoutKey(k => true);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Without_RemovesMatchingEntries()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 100, ["c"] = 3, ["d"] = 50 };

        // Act
        var result = dict.Without((k, v) => v >= 50);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContainKey("a");
        result.ShouldNotContainKey("b");
        result.ShouldContainKey("c");
        result.ShouldNotContainKey("d");
    }

    [Fact]
    public void Without_NoMatch_ReturnsAllEntries()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        // Act
        var result = dict.Without((k, v) => v > 100);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContainKey("a");
        result.ShouldContainKey("b");
    }

    [Fact]
    public void Without_AllMatch_ReturnsEmptyDict()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        // Act
        var result = dict.Without((k, v) => v > 0);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Without_EmptyDict_ReturnsEmptyDict()
    {
        // Arrange
        var dict = new Dictionary<string, int>();

        // Act
        var result = dict.Without((k, v) => true);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Without_PredicateUsesKeyAndValue()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

        // Act
        var result = dict.Without((k, v) => k == "b" && v == 2);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContainKey("a");
        result.ShouldNotContainKey("b");
        result.ShouldContainKey("c");
    }

    [Fact]
    public void ToDictionary_FromKvpEnumerable_BuildsDictionary()
    {
        // Arrange
        var kvps = new[]
        {
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("b", 2),
            new KeyValuePair<string, int>("c", 3)
        };

        // Act
        var result = kvps.ToDictionary();

        // Assert
        result.Count.ShouldBe(3);
        result["a"].ShouldBe(1);
        result["b"].ShouldBe(2);
        result["c"].ShouldBe(3);
    }

    [Fact]
    public void ToDictionary_EmptySequence_ReturnsEmptyDictionary()
    {
        // Arrange
        var kvps = Array.Empty<KeyValuePair<string, int>>();

        // Act
        var result = kvps.ToDictionary();

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void ToDictionary_DuplicateKeys_ThrowsArgumentException()
    {
        // Arrange
        var kvps = new[]
        {
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("a", 2)
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => kvps.ToDictionary());
    }

    [Fact]
    public void ToDictionary_IntKeys_BuildsDictionary()
    {
        // Arrange
        var kvps = new[]
        {
            new KeyValuePair<int, string>(1, "one"),
            new KeyValuePair<int, string>(2, "two")
        };

        // Act
        var result = kvps.ToDictionary();

        // Assert
        result.Count.ShouldBe(2);
        result[1].ShouldBe("one");
        result[2].ShouldBe("two");
    }

    [Fact]
    public void ToDictionary_FromIDictionary_CreatesNewDictionary()
    {
        // Arrange
        IDictionary<string, int> source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

        // Act
        var result = source.ToDictionary();

        // Assert
        result.Count.ShouldBe(3);
        result["a"].ShouldBe(1);
        result["b"].ShouldBe(2);
        result["c"].ShouldBe(3);
    }

    [Fact]
    public void ToDictionary_FromIDictionary_EmptyDictionary_ReturnsEmptyDictionary()
    {
        // Arrange
        IDictionary<string, int> source = new Dictionary<string, int>();

        // Act
        var result = source.ToDictionary();

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void ToDictionary_FromIDictionary_OriginalUnmodified()
    {
        // Arrange
        IDictionary<string, int> source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        // Act
        var result = source.ToDictionary();
        result["a"] = 100;

        // Assert
        source["a"].ShouldBe(1);
    }

    [Fact]
    public void CopyTo_IDictionary_CopiesAllEntries()
    {
        // Arrange
        IDictionary<string, int> source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
        IDictionary<string, int> target = new Dictionary<string, int>();

        // Act
        source.CopyTo(target);

        // Assert
        target.Count.ShouldBe(3);
        target["a"].ShouldBe(1);
        target["b"].ShouldBe(2);
        target["c"].ShouldBe(3);
    }

    [Fact]
    public void CopyTo_IDictionary_EmptySource_TargetUnchanged()
    {
        // Arrange
        IDictionary<string, int> source = new Dictionary<string, int>();
        IDictionary<string, int> target = new Dictionary<string, int> { ["x"] = 99 };

        // Act
        source.CopyTo(target);

        // Assert
        target.Count.ShouldBe(1);
        target["x"].ShouldBe(99);
    }

    [Fact]
    public void CopyTo_IDictionary_TargetHasEntries_AddsNew()
    {
        // Arrange
        IDictionary<string, int> source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        IDictionary<string, int> target = new Dictionary<string, int> { ["x"] = 99 };

        // Act
        source.CopyTo(target);

        // Assert
        target.Count.ShouldBe(3);
        target["a"].ShouldBe(1);
        target["b"].ShouldBe(2);
        target["x"].ShouldBe(99);
    }

    [Fact]
    public void CopyTo_IDictionary_OverlappingKeys_OverwritesValues()
    {
        // Arrange
        IDictionary<string, int> source = new Dictionary<string, int> { ["a"] = 100, ["b"] = 200 };
        IDictionary<string, int> target = new Dictionary<string, int> { ["a"] = 1, ["c"] = 3 };

        // Act
        source.CopyTo(target);

        // Assert
        target.Count.ShouldBe(3);
        target["a"].ShouldBe(100);
        target["b"].ShouldBe(200);
        target["c"].ShouldBe(3);
    }

    [Fact]
    public void CopyTo_Dictionary_CopiesAllEntries()
    {
        // Arrange
        Dictionary<string, int> source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
        Dictionary<string, int> target = new Dictionary<string, int>();

        // Act
        source.CopyTo(target);

        // Assert
        target.Count.ShouldBe(3);
        target["a"].ShouldBe(1);
        target["b"].ShouldBe(2);
        target["c"].ShouldBe(3);
    }

    [Fact]
    public void CopyTo_Dictionary_EmptySource_TargetUnchanged()
    {
        // Arrange
        Dictionary<string, int> source = new Dictionary<string, int>();
        Dictionary<string, int> target = new Dictionary<string, int> { ["x"] = 99 };

        // Act
        source.CopyTo(target);

        // Assert
        target.Count.ShouldBe(1);
        target["x"].ShouldBe(99);
    }

    [Fact]
    public void CopyTo_Dictionary_TargetHasEntries_AddsNew()
    {
        // Arrange
        Dictionary<string, int> source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        Dictionary<string, int> target = new Dictionary<string, int> { ["x"] = 99 };

        // Act
        source.CopyTo(target);

        // Assert
        target.Count.ShouldBe(3);
        target["a"].ShouldBe(1);
        target["b"].ShouldBe(2);
        target["x"].ShouldBe(99);
    }

    [Fact]
    public void CopyTo_Dictionary_OverlappingKeys_OverwritesValues()
    {
        // Arrange
        Dictionary<string, int> source = new Dictionary<string, int> { ["a"] = 100, ["b"] = 200 };
        Dictionary<string, int> target = new Dictionary<string, int> { ["a"] = 1, ["c"] = 3 };

        // Act
        source.CopyTo(target);

        // Assert
        target.Count.ShouldBe(3);
        target["a"].ShouldBe(100);
        target["b"].ShouldBe(200);
        target["c"].ShouldBe(3);
    }

    [Fact]
    public void RemoveWhere_RemovesMatchingEntries()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 100, ["c"] = 3, ["d"] = 50 };

        // Act
        dict.RemoveWhere((k, v) => v >= 50);

        // Assert
        dict.Count.ShouldBe(2);
        dict.ShouldContainKey("a");
        dict.ShouldNotContainKey("b");
        dict.ShouldContainKey("c");
        dict.ShouldNotContainKey("d");
    }

    [Fact]
    public void RemoveWhere_NoMatches_DictionaryUnchanged()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

        // Act
        dict.RemoveWhere((k, v) => v > 100);

        // Assert
        dict.Count.ShouldBe(3);
        dict.ShouldContainKey("a");
        dict.ShouldContainKey("b");
        dict.ShouldContainKey("c");
    }

    [Fact]
    public void RemoveWhere_AllMatch_EmptiesDictionary()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

        // Act
        dict.RemoveWhere((k, v) => v > 0);

        // Assert
        dict.Count.ShouldBe(0);
    }

    [Fact]
    public void RemoveWhere_EmptyDictionary_NoEffect()
    {
        // Arrange
        var dict = new Dictionary<string, int>();

        // Act
        dict.RemoveWhere((k, v) => true);

        // Assert
        dict.Count.ShouldBe(0);
    }

    [Fact]
    public void RemoveWhere_PredicateUsesKeyAndValue()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

        // Act
        dict.RemoveWhere((k, v) => k == "b" && v == 2);

        // Assert
        dict.Count.ShouldBe(2);
        dict.ShouldContainKey("a");
        dict.ShouldNotContainKey("b");
        dict.ShouldContainKey("c");
    }

    [Fact]
    public void AddToList_NewKey_CreatesListWithValue()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>>();

        // Act
        dict.AddToList("a", 1);

        // Assert
        dict.Count.ShouldBe(1);
        dict["a"].Count.ShouldBe(1);
        dict["a"][0].ShouldBe(1);
    }

    [Fact]
    public void AddToList_ExistingKey_AppendsToList()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>> { ["a"] = new List<int> { 1 } };

        // Act
        dict.AddToList("a", 2);

        // Assert
        dict.Count.ShouldBe(1);
        dict["a"].Count.ShouldBe(2);
        dict["a"][0].ShouldBe(1);
        dict["a"][1].ShouldBe(2);
    }

    [Fact]
    public void AddToList_MultipleAdditionsToSameKey_AppendsAll()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>>();

        // Act
        dict.AddToList("a", 1);
        dict.AddToList("a", 2);
        dict.AddToList("a", 3);

        // Assert
        dict.Count.ShouldBe(1);
        dict["a"].Count.ShouldBe(3);
        dict["a"][0].ShouldBe(1);
        dict["a"][1].ShouldBe(2);
        dict["a"][2].ShouldBe(3);
    }

    [Fact]
    public void AddToList_DifferentKeys_CreatesSeparateLists()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>>();

        // Act
        dict.AddToList("a", 1);
        dict.AddToList("b", 2);
        dict.AddToList("a", 3);

        // Assert
        dict.Count.ShouldBe(2);
        dict["a"].Count.ShouldBe(2);
        dict["a"][0].ShouldBe(1);
        dict["a"][1].ShouldBe(3);
        dict["b"].Count.ShouldBe(1);
        dict["b"][0].ShouldBe(2);
    }

    [Fact]
    public void GetList_KeyPresent_ReturnsListWithElements()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>> { ["a"] = new List<int> { 1, 2, 3 } };

        // Act
        var result = dict.GetList("a");

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result[0].ShouldBe(1);
        result[1].ShouldBe(2);
        result[2].ShouldBe(3);
    }

    [Fact]
    public void ToDictionary_FromIDictionary_WithNullableValues_PreservesNulls()
    {
        // Arrange
        IDictionary<string, string?> source = new Dictionary<string, string?> { ["a"] = "value", ["b"] = null };

        // Act
        var result = source.ToDictionary();

        // Assert
        result.Count.ShouldBe(2);
        result["a"].ShouldBe("value");
        result["b"].ShouldBeNull();
    }

    [Fact]
    public void ToDictionary_FromIDictionary_WithIntKeys_CreatesCopy()
    {
        // Arrange
        IDictionary<int, string> source = new Dictionary<int, string> { [1] = "one", [2] = "two" };

        // Act
        var result = source.ToDictionary();

        // Assert
        result.Count.ShouldBe(2);
        result[1].ShouldBe("one");
        result[2].ShouldBe("two");
    }

    [Fact]
    public void CopyTo_IDictionary_SourceUnmodified()
    {
        // Arrange
        IDictionary<string, int> source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        IDictionary<string, int> target = new Dictionary<string, int>();

        // Act
        source.CopyTo(target);

        // Assert
        source.Count.ShouldBe(2);
        source["a"].ShouldBe(1);
        source["b"].ShouldBe(2);
    }

    [Fact]
    public void CopyTo_IDictionary_SameInstance_UpdatesValues()
    {
        // Arrange
        IDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        // Act
        dict.CopyTo(dict);

        // Assert
        dict.Count.ShouldBe(2);
        dict["a"].ShouldBe(1);
        dict["b"].ShouldBe(2);
    }

    [Fact]
    public void CopyTo_Dictionary_SourceUnmodified()
    {
        // Arrange
        Dictionary<string, int> source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        Dictionary<string, int> target = new Dictionary<string, int>();

        // Act
        source.CopyTo(target);

        // Assert
        source.Count.ShouldBe(2);
        source["a"].ShouldBe(1);
        source["b"].ShouldBe(2);
    }

    [Fact]
    public void CopyTo_Dictionary_SameInstance_UpdatesValues()
    {
        // Arrange
        Dictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        // Act
        dict.CopyTo(dict);

        // Assert
        dict.Count.ShouldBe(2);
        dict["a"].ShouldBe(1);
        dict["b"].ShouldBe(2);
    }

    [Fact]
    public void RemoveWhere_RemovesFirstEntry()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

        // Act
        dict.RemoveWhere((k, v) => k == "a");

        // Assert
        dict.Count.ShouldBe(2);
        dict.ShouldNotContainKey("a");
        dict.ShouldContainKey("b");
        dict.ShouldContainKey("c");
    }

    [Fact]
    public void RemoveWhere_RemovesLastEntry()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

        // Act
        dict.RemoveWhere((k, v) => k == "c");

        // Assert
        dict.Count.ShouldBe(2);
        dict.ShouldContainKey("a");
        dict.ShouldContainKey("b");
        dict.ShouldNotContainKey("c");
    }

    [Fact]
    public void RemoveWhere_RemovesMiddleEntry()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

        // Act
        dict.RemoveWhere((k, v) => k == "b");

        // Assert
        dict.Count.ShouldBe(2);
        dict.ShouldContainKey("a");
        dict.ShouldNotContainKey("b");
        dict.ShouldContainKey("c");
    }

    [Fact]
    public void RemoveWhere_PredicateBasedOnValueOnly()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 5, ["b"] = 10, ["c"] = 15 };

        // Act
        dict.RemoveWhere((k, v) => v == 10);

        // Assert
        dict.Count.ShouldBe(2);
        dict.ShouldContainKey("a");
        dict.ShouldNotContainKey("b");
        dict.ShouldContainKey("c");
    }

    [Fact]
    public void RemoveWhere_PredicateBasedOnKeyOnly()
    {
        // Arrange
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

        // Act
        dict.RemoveWhere((k, v) => k == "a" || k == "c");

        // Assert
        dict.Count.ShouldBe(1);
        dict.ShouldNotContainKey("a");
        dict.ShouldContainKey("b");
        dict.ShouldNotContainKey("c");
    }

    [Fact]
    public void AddToList_WithReferenceType_CreatesListWithValue()
    {
        // Arrange
        var dict = new Dictionary<int, IList<string>>();

        // Act
        dict.AddToList(1, "first");

        // Assert
        dict.Count.ShouldBe(1);
        dict[1].Count.ShouldBe(1);
        dict[1][0].ShouldBe("first");
    }

    [Fact]
    public void AddToList_WithNullableReferenceType_HandlesNull()
    {
        // Arrange
        var dict = new Dictionary<string, IList<string?>>();

        // Act
        dict.AddToList("a", null);

        // Assert
        dict.Count.ShouldBe(1);
        dict["a"].Count.ShouldBe(1);
        dict["a"][0].ShouldBeNull();
    }

    [Fact]
    public void AddToList_EmptyDictionary_AddsFirstKey()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>>();

        // Act
        dict.AddToList("key", 42);

        // Assert
        dict.Count.ShouldBe(1);
        dict["key"].ShouldNotBeNull();
        dict["key"].Count.ShouldBe(1);
        dict["key"][0].ShouldBe(42);
    }

    [Fact]
    public void GetList_KeyPresentWithEmptyList_ReturnsNull()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>> { ["a"] = new List<int>() };

        // Act
        var result = dict.GetList("a");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetList_KeyAbsent_ReturnsNull()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>> { ["a"] = new List<int> { 1, 2 } };

        // Act
        var result = dict.GetList("missing");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void RemoveFromList_KeyPresentValueExists_RemovesValue()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>> { ["a"] = new List<int> { 1, 2, 3 } };

        // Act
        dict.RemoveFromList("a", 2);

        // Assert
        dict.Count.ShouldBe(1);
        dict["a"].Count.ShouldBe(2);
        dict["a"].ShouldContain(1);
        dict["a"].ShouldContain(3);
        dict["a"].ShouldNotContain(2);
    }

    [Fact]
    public void RemoveFromList_KeyPresentValueExistsListBecomesEmpty_RemovesKey()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>> { ["a"] = new List<int> { 1 } };

        // Act
        dict.RemoveFromList("a", 1);

        // Assert
        dict.Count.ShouldBe(0);
        dict.ShouldNotContainKey("a");
    }

    [Fact]
    public void RemoveFromList_KeyPresentValueDoesNotExist_NoChange()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>> { ["a"] = new List<int> { 1, 2 } };

        // Act
        dict.RemoveFromList("a", 99);

        // Assert
        dict.Count.ShouldBe(1);
        dict["a"].Count.ShouldBe(2);
        dict["a"].ShouldContain(1);
        dict["a"].ShouldContain(2);
    }

    [Fact]
    public void RemoveFromList_KeyAbsent_NoChange()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>> { ["a"] = new List<int> { 1, 2 } };

        // Act
        dict.RemoveFromList("missing", 1);

        // Assert
        dict.Count.ShouldBe(1);
        dict["a"].Count.ShouldBe(2);
    }

    [Fact]
    public void AnyInAnyList_EmptyDictionary_ReturnsFalse()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>>();

        // Act
        var result = dict.AnyInAnyList();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AnyInAnyList_DictionaryWithEmptyLists_ReturnsFalse()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>>
        {
            ["a"] = new List<int>(),
            ["b"] = new List<int>()
        };

        // Act
        var result = dict.AnyInAnyList();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AnyInAnyList_DictionaryWithNonEmptyList_ReturnsTrue()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>>
        {
            ["a"] = new List<int>(),
            ["b"] = new List<int> { 1 }
        };

        // Act
        var result = dict.AnyInAnyList();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void AnyInAnyList_DictionaryWithMultipleNonEmptyLists_ReturnsTrue()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>>
        {
            ["a"] = new List<int> { 1, 2 },
            ["b"] = new List<int> { 3, 4 }
        };

        // Act
        var result = dict.AnyInAnyList();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void AnyInAnyList_Predicate_EmptyDictionary_ReturnsFalse()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>>();

        // Act
        var result = dict.AnyInAnyList(x => x > 5);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AnyInAnyList_Predicate_NoMatchingElements_ReturnsFalse()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>>
        {
            ["a"] = new List<int> { 1, 2, 3 },
            ["b"] = new List<int> { 4, 5 }
        };

        // Act
        var result = dict.AnyInAnyList(x => x > 10);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AnyInAnyList_Predicate_MatchingElement_ReturnsTrue()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>>
        {
            ["a"] = new List<int> { 1, 2, 3 },
            ["b"] = new List<int> { 4, 5, 15 }
        };

        // Act
        var result = dict.AnyInAnyList(x => x > 10);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void AnyInAnyList_Predicate_MultipleMatchingElements_ReturnsTrue()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>>
        {
            ["a"] = new List<int> { 1, 11, 3 },
            ["b"] = new List<int> { 14, 5, 15 }
        };

        // Act
        var result = dict.AnyInAnyList(x => x > 10);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void AnyInAnyList_Predicate_EmptyLists_ReturnsFalse()
    {
        // Arrange
        var dict = new Dictionary<string, IList<int>>
        {
            ["a"] = new List<int>(),
            ["b"] = new List<int>()
        };

        // Act
        var result = dict.AnyInAnyList(x => x > 0);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AddToList_DictionaryListOverload_NewKey_CreatesListWithValue()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>();

        // Act
        dict.AddToList("a", 1);

        // Assert
        dict.Count.ShouldBe(1);
        dict["a"].Count.ShouldBe(1);
        dict["a"][0].ShouldBe(1);
    }

    [Fact]
    public void AddToList_DictionaryListOverload_ExistingKey_AppendsToList()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>> { ["a"] = new List<int> { 1 } };

        // Act
        dict.AddToList("a", 2);

        // Assert
        dict.Count.ShouldBe(1);
        dict["a"].Count.ShouldBe(2);
        dict["a"][0].ShouldBe(1);
        dict["a"][1].ShouldBe(2);
    }

    [Fact]
    public void AddToList_DictionaryListOverload_MultipleAdditionsToSameKey_AppendsAll()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>();

        // Act
        dict.AddToList("a", 1);
        dict.AddToList("a", 2);
        dict.AddToList("a", 3);

        // Assert
        dict.Count.ShouldBe(1);
        dict["a"].Count.ShouldBe(3);
        dict["a"][0].ShouldBe(1);
        dict["a"][1].ShouldBe(2);
        dict["a"][2].ShouldBe(3);
    }

    [Fact]
    public void AddToList_DictionaryListOverload_DifferentKeys_CreatesSeparateLists()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>();

        // Act
        dict.AddToList("a", 1);
        dict.AddToList("b", 2);
        dict.AddToList("a", 3);

        // Assert
        dict.Count.ShouldBe(2);
        dict["a"].Count.ShouldBe(2);
        dict["a"][0].ShouldBe(1);
        dict["a"][1].ShouldBe(3);
        dict["b"].Count.ShouldBe(1);
        dict["b"][0].ShouldBe(2);
    }

    [Fact]
    public void AddToList_DictionaryListOverload_WithReferenceType_CreatesListWithValue()
    {
        // Arrange
        var dict = new Dictionary<int, List<string>>();

        // Act
        dict.AddToList(1, "first");

        // Assert
        dict.Count.ShouldBe(1);
        dict[1].Count.ShouldBe(1);
        dict[1][0].ShouldBe("first");
    }

    [Fact]
    public void AddToList_DictionaryListOverload_EmptyDictionary_AddsFirstKey()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>();

        // Act
        dict.AddToList("key", 42);

        // Assert
        dict.Count.ShouldBe(1);
        dict["key"].ShouldNotBeNull();
        dict["key"].Count.ShouldBe(1);
        dict["key"][0].ShouldBe(42);
    }

    [Fact]
    public void GetList_DictOfListKeyPresentWithItems_ReturnsList()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>
        {
            ["key1"] = [1, 2, 3]
        };

        // Act
        var result = dict.GetList("key1");

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.ShouldBe(new List<int> { 1, 2, 3 });
    }

    [Fact]
    public void GetList_DictOfListKeyPresentWithEmptyList_ReturnsNull()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>
        {
            ["key1"] = []
        };

        // Act
        var result = dict.GetList("key1");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetList_DictOfListKeyAbsent_ReturnsNull()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>();

        // Act
        var result = dict.GetList("missing");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void RemoveFromList_KeyPresentValuePresent_RemovesValue()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>
        {
            ["key1"] = [1, 2, 3]
        };

        // Act
        dict.RemoveFromList("key1", 2);

        // Assert
        dict["key1"].Count.ShouldBe(2);
        dict["key1"].ShouldBe(new List<int> { 1, 3 });
    }

    [Fact]
    public void RemoveFromList_KeyPresentLastValue_RemovesValueAndKey()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>
        {
            ["key1"] = [1]
        };

        // Act
        dict.RemoveFromList("key1", 1);

        // Assert
        dict.ShouldNotContainKey("key1");
    }

    [Fact]
    public void RemoveFromList_KeyPresentValueAbsent_DoesNothing()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>
        {
            ["key1"] = [1, 2, 3]
        };

        // Act
        dict.RemoveFromList("key1", 99);

        // Assert
        dict["key1"].Count.ShouldBe(3);
        dict["key1"].ShouldBe(new List<int> { 1, 2, 3 });
    }

    [Fact]
    public void RemoveFromList_KeyAbsent_DoesNothing()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>();

        // Act
        dict.RemoveFromList("missing", 1);

        // Assert
        dict.ShouldBeEmpty();
    }

    [Fact]
    public void AnyInAnyList_DictionaryHasItemsInLists_ReturnsTrue()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>
        {
            ["key1"] = [1, 2, 3]
        };

        // Act
        var result = dict.AnyInAnyList();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void AnyInAnyList_DictionaryHasOnlyEmptyLists_ReturnsFalse()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>
        {
            ["key1"] = [],
            ["key2"] = []
        };

        // Act
        var result = dict.AnyInAnyList();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AnyInAnyList_DictOfListEmptyDictionary_ReturnsFalse()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>();

        // Act
        var result = dict.AnyInAnyList();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AnyInAnyList_WithPredicate_MatchingItemExists_ReturnsTrue()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>
        {
            ["key1"] = [1, 2, 3],
            ["key2"] = [4, 5, 6]
        };

        // Act
        var result = dict.AnyInAnyList(x => x > 5);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void AnyInAnyList_WithPredicate_NoMatchingItems_ReturnsFalse()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>
        {
            ["key1"] = [1, 2, 3],
            ["key2"] = [4, 5, 6]
        };

        // Act
        var result = dict.AnyInAnyList(x => x > 10);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AnyInAnyList_WithPredicate_EmptyDictionary_ReturnsFalse()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>();

        // Act
        var result = dict.AnyInAnyList(x => x > 0);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AnyInAnyList_WithPredicate_EmptyLists_ReturnsFalse()
    {
        // Arrange
        var dict = new Dictionary<string, List<int>>
        {
            ["key1"] = []
        };

        // Act
        var result = dict.AnyInAnyList(x => x > 0);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AddToSet_KeyAbsent_CreatesSetAndAddsValue()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>();

        // Act
        var result = dict.AddToSet("key1", 1);

        // Assert
        result.ShouldBeTrue();
        dict.ShouldContainKey("key1");
        dict["key1"].Count.ShouldBe(1);
        dict["key1"].ShouldContain(1);
    }

    [Fact]
    public void AddToSet_KeyPresentValueAbsent_AddsValueAndReturnsTrue()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>
        {
            ["key1"] = [1, 2]
        };

        // Act
        var result = dict.AddToSet("key1", 3);

        // Assert
        result.ShouldBeTrue();
        dict["key1"].Count.ShouldBe(3);
        dict["key1"].ShouldContain(3);
    }

    [Fact]
    public void AddToSet_KeyPresentValuePresent_ReturnsFalse()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>
        {
            ["key1"] = [1, 2, 3]
        };

        // Act
        var result = dict.AddToSet("key1", 2);

        // Assert
        result.ShouldBeFalse();
        dict["key1"].Count.ShouldBe(3);
    }

    [Fact]
    public void GetSet_KeyPresent_ReturnsSet()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>
        {
            ["key1"] = new HashSet<int> { 1, 2, 3 }
        };

        // Act
        var result = dict.GetSet("key1");

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.ShouldContain(1);
        result.ShouldContain(2);
        result.ShouldContain(3);
    }

    [Fact]
    public void GetSet_KeyPresentWithEmptySet_ReturnsNull()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>
        {
            ["key1"] = new HashSet<int>()
        };

        // Act
        var result = dict.GetSet("key1");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetSet_KeyAbsent_ReturnsNull()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>();

        // Act
        var result = dict.GetSet("missing");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void RemoveFromSet_KeyPresentValueInSet_RemovesValueAndReturnsSet()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>
        {
            ["key1"] = new HashSet<int> { 1, 2, 3 }
        };

        // Act
        var result = dict.RemoveFromSet("key1", 2);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.ShouldContain(1);
        result.ShouldContain(3);
        result.ShouldNotContain(2);
    }

    [Fact]
    public void RemoveFromSet_KeyPresentValueInSetRemoveEmptySetTrue_SetNotEmptyAfterRemoval_KeepsKey()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>
        {
            ["key1"] = new HashSet<int> { 1, 2, 3 }
        };

        // Act
        var result = dict.RemoveFromSet("key1", 2, removeEmptySet: true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        dict.ContainsKey("key1").ShouldBeTrue();
    }

    [Fact]
    public void RemoveFromSet_KeyPresentValueInSetRemoveEmptySetTrue_SetEmptyAfterRemoval_RemovesKey()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>
        {
            ["key1"] = new HashSet<int> { 1 }
        };

        // Act
        var result = dict.RemoveFromSet("key1", 1, removeEmptySet: true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        dict.ContainsKey("key1").ShouldBeFalse();
    }

    [Fact]
    public void RemoveFromSet_KeyPresentValueNotInSet_ReturnsSetUnchanged()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>
        {
            ["key1"] = new HashSet<int> { 1, 2, 3 }
        };

        // Act
        var result = dict.RemoveFromSet("key1", 99);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.ShouldContain(1);
        result.ShouldContain(2);
        result.ShouldContain(3);
    }

    [Fact]
    public void RemoveFromSet_KeyAbsent_ReturnsNull()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>();

        // Act
        var result = dict.RemoveFromSet("missing", 1);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void AnyInAnySet_DictionaryHasSetsWithItems_ReturnsTrue()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>
        {
            ["key1"] = new HashSet<int> { 1, 2, 3 }
        };

        // Act
        var result = dict.AnyInAnySet();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void AnyInAnySet_DictionaryHasOnlyEmptySets_ReturnsFalse()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>
        {
            ["key1"] = new HashSet<int>()
        };

        // Act
        var result = dict.AnyInAnySet();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AnyInAnySet_DictionaryEmpty_ReturnsFalse()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>();

        // Act
        var result = dict.AnyInAnySet();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AnyInAnySet_WithPredicate_DictionaryHasSetsWithMatchingItems_ReturnsTrue()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>
        {
            ["key1"] = new HashSet<int> { 1, 2, 3 },
            ["key2"] = new HashSet<int> { 4, 5, 6 }
        };

        // Act
        var result = dict.AnyInAnySet(x => x > 5);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void AnyInAnySet_WithPredicate_DictionaryHasSetsButNoMatchingItems_ReturnsFalse()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>
        {
            ["key1"] = new HashSet<int> { 1, 2, 3 },
            ["key2"] = new HashSet<int> { 4, 5, 6 }
        };

        // Act
        var result = dict.AnyInAnySet(x => x > 10);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AnyInAnySet_WithPredicate_DictionaryEmpty_ReturnsFalse()
    {
        // Arrange
        var dict = new Dictionary<string, HashSet<int>>();

        // Act
        var result = dict.AnyInAnySet(x => x > 0);

        // Assert
        result.ShouldBeFalse();
    }
}
