namespace RossWright;

public class IEnumerableExtensionsTests
{
    // ── WhereIf ───────────────────────────────────────────────────────────────────
    [Fact]
    public void WhereIf_FlagTrue_AppliesIfTruePredicate()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var result = source.WhereIf(true, x => x > 3).ToList();
        result.ShouldBe(new[] { 4, 5 });
    }

    [Fact]
    public void WhereIf_FlagFalseWithoutIfFalse_ReturnsOriginalSequence()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var result = source.WhereIf(false, x => x > 3).ToList();
        result.ShouldBe(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void WhereIf_FlagFalseWithIfFalse_AppliesIfFalsePredicate()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var result = source.WhereIf(false, x => x > 3, x => x < 3).ToList();
        result.ShouldBe(new[] { 1, 2 });
    }

    [Fact]
    public void WhereIf_EmptySource_ReturnsEmpty()
    {
        var source = Array.Empty<int>();
        var result = source.WhereIf(true, x => x > 0).ToList();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void WhereIf_NoMatches_ReturnsEmpty()
    {
        var source = new[] { 1, 2, 3 };
        var result = source.WhereIf(true, x => x > 10).ToList();
        result.ShouldBeEmpty();
    }

    // ── ConcatAllowNull ───────────────────────────────────────────────────────────
    [Fact]
    public void ConcatAllowNull_BothNonNull_ReturnsConcatenatedSequence()
    {
        var first = new[] { 1, 2, 3 };
        var second = new[] { 4, 5, 6 };
        var result = first.ConcatAllowNull(second)!.ToList();
        result.ShouldBe(new[] { 1, 2, 3, 4, 5, 6 });
    }

    [Fact]
    public void ConcatAllowNull_FirstNull_ReturnsSecond()
    {
        IEnumerable<int>? first = null;
        var second = new[] { 4, 5, 6 };
        var result = first.ConcatAllowNull(second);
        result.ShouldBe(second);
    }

    [Fact]
    public void ConcatAllowNull_SecondNull_ReturnsFirst()
    {
        var first = new[] { 1, 2, 3 };
        IEnumerable<int>? second = null;
        var result = first.ConcatAllowNull(second);
        result.ShouldBe(first);
    }

    [Fact]
    public void ConcatAllowNull_BothNull_ReturnsNull()
    {
        IEnumerable<int>? first = null;
        IEnumerable<int>? second = null;
        var result = first.ConcatAllowNull(second);
        result.ShouldBeNull();
    }

    [Fact]
    public void ConcatAllowNull_FirstEmpty_ReturnsSecond()
    {
        var first = Array.Empty<int>();
        var second = new[] { 4, 5, 6 };
        var result = first.ConcatAllowNull(second)!.ToList();
        result.ShouldBe(new[] { 4, 5, 6 });
    }

    [Fact]
    public void ConcatAllowNull_SecondEmpty_ReturnsFirst()
    {
        var first = new[] { 1, 2, 3 };
        var second = Array.Empty<int>();
        var result = first.ConcatAllowNull(second)!.ToList();
        result.ShouldBe(new[] { 1, 2, 3 });
    }

    // ── WithIndex ─────────────────────────────────────────────────────────────────
    [Fact]
    public void WithIndex_NonEmptySequence_ReturnsItemsWithIndices()
    {
        var source = new[] { "a", "b", "c" };
        var result = source.WithIndex().ToList();
        result.ShouldBe(new[] { ("a", 0), ("b", 1), ("c", 2) });
    }

    [Fact]
    public void WithIndex_EmptySequence_ReturnsEmpty()
    {
        var source = Array.Empty<string>();
        var result = source.WithIndex().ToList();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void WithIndex_NullSource_ReturnsEmptyArray()
    {
        IEnumerable<string>? source = null;
        var result = source!.WithIndex().ToList();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void WithIndex_SingleElement_ReturnsZeroIndex()
    {
        var source = new[] { "single" };
        var result = source.WithIndex().ToList();
        result.ShouldBe(new[] { ("single", 0) });
    }

    // ── ForEach ───────────────────────────────────────────────────────────────────
    [Fact]
    public void ForEach_InvokesActionForEveryElement()
    {
        var source = new[] { 1, 2, 3 };
        var collected = new List<int>();
        source.ForEach(x => collected.Add(x));
        collected.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void ForEach_EmptySequence_DoesNotInvokeAction()
    {
        var source = Array.Empty<int>();
        var collected = new List<int>();
        source.ForEach(x => collected.Add(x));
        collected.ShouldBeEmpty();
    }

    [Fact]
    public void ForEach_ActionWithSideEffect_SideEffectOccurs()
    {
        var source = new[] { 1, 2, 3 };
        var sum = 0;
        source.ForEach(x => sum += x);
        sum.ShouldBe(6);
    }

    // ── Without ───────────────────────────────────────────────────────────────────
    [Fact]
    public void Without_RemovesSpecifiedValues()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var result = source.Without(2, 4).ToList();
        result.ShouldBe(new[] { 1, 3, 5 });
    }

    [Fact]
    public void Without_NoExclusions_ReturnsAllElements()
    {
        var source = new[] { 1, 2, 3 };
        var result = source.Without().ToList();
        result.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Without_NoMatchingExclusions_ReturnsAllElements()
    {
        var source = new[] { 1, 2, 3 };
        var result = source.Without(4, 5).ToList();
        result.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Without_EmptySource_ReturnsEmpty()
    {
        var source = Array.Empty<int>();
        var result = source.Without(1, 2).ToList();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Without_AllElementsExcluded_ReturnsEmpty()
    {
        var source = new[] { 1, 2, 3 };
        var result = source.Without(1, 2, 3).ToList();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Without_DuplicateValues_RemovesAllOccurrences()
    {
        var source = new[] { 1, 2, 2, 3, 2, 4 };
        var result = source.Without(2).ToList();
        result.ShouldBe(new[] { 1, 3, 4 });
    }

    [Fact]
    public void Without_NullExclusion_RemovesNullValues()
    {
        var source = new string?[] { "a", null, "b", null, "c" };
        var result = source.Without((string?)null).ToList();
        result.ShouldBe(new[] { "a", "b", "c" });
    }

    [Fact]
    public void Without_NullItemAndNonNullExclusion_KeepsNull()
    {
        var source = new string?[] { "a", null, "b" };
        var result = source.Without("a").ToList();
        result.ShouldBe(new string?[] { null, "b" });
    }

    // ── OrderBy ───────────────────────────────────────────────────────────────────
    [Fact]
    public void OrderBy_IsAscendingTrue_OrdersAscending()
    {
        var source = new[] { 3, 1, 4, 1, 5, 9, 2 };
        var result = source.OrderBy(x => x, true).ToList();
        result.ShouldBe(new[] { 1, 1, 2, 3, 4, 5, 9 });
    }

    [Fact]
    public void OrderBy_IsAscendingFalse_OrdersDescending()
    {
        var source = new[] { 3, 1, 4, 1, 5, 9, 2 };
        var result = source.OrderBy(x => x, false).ToList();
        result.ShouldBe(new[] { 9, 5, 4, 3, 2, 1, 1 });
    }

    [Fact]
    public void OrderBy_EmptySource_ReturnsEmpty()
    {
        var source = Array.Empty<int>();
        var result = source.OrderBy(x => x, true).ToList();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void OrderBy_SingleElement_ReturnsSingleElement()
    {
        var source = new[] { 42 };
        var result = source.OrderBy(x => x, true).ToList();
        result.ShouldBe(new[] { 42 });
    }

    [Fact]
    public void OrderBy_ComplexKeySelector_OrdersByKey()
    {
        var source = new[] { "apple", "pie", "banana", "cat" };
        var result = source.OrderBy(x => x.Length, true).ToList();
        result.ShouldBe(new[] { "pie", "cat", "apple", "banana" });
    }

    // ── ThenBy ────────────────────────────────────────────────────────────────────
    [Fact]
    public void ThenBy_IsAscendingTrue_ThenOrdersAscending()
    {
        var source = new[] { ("a", 2), ("b", 1), ("a", 1), ("b", 2) };
        var result = source.OrderBy(x => x.Item1).ThenBy(x => x.Item2, true).ToList();
        result.ShouldBe(new[] { ("a", 1), ("a", 2), ("b", 1), ("b", 2) });
    }

    [Fact]
    public void ThenBy_IsAscendingFalse_ThenOrdersDescending()
    {
        var source = new[] { ("a", 2), ("b", 1), ("a", 1), ("b", 2) };
        var result = source.OrderBy(x => x.Item1).ThenBy(x => x.Item2, false).ToList();
        result.ShouldBe(new[] { ("a", 2), ("a", 1), ("b", 2), ("b", 1) });
    }

    [Fact]
    public void ThenBy_EmptySource_ReturnsEmpty()
    {
        var source = Array.Empty<(string, int)>();
        var result = source.OrderBy(x => x.Item1).ThenBy(x => x.Item2, true).ToList();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void ThenBy_SingleElement_ReturnsSingleElement()
    {
        var source = new[] { ("a", 1) };
        var result = source.OrderBy(x => x.Item1).ThenBy(x => x.Item2, true).ToList();
        result.ShouldBe(new[] { ("a", 1) });
    }

    // ── FirstIndexWhere ───────────────────────────────────────────────────────────
    [Fact]
    public void FirstIndexWhere_PredicateMatchesFirst_ReturnsZero()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var result = source.FirstIndexWhere(x => x == 1);
        result.ShouldBe(0);
    }

    [Fact]
    public void FirstIndexWhere_PredicateMatchesMiddle_ReturnsIndex()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var result = source.FirstIndexWhere(x => x == 3);
        result.ShouldBe(2);
    }

    [Fact]
    public void FirstIndexWhere_PredicateMatchesLast_ReturnsLastIndex()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var result = source.FirstIndexWhere(x => x == 5);
        result.ShouldBe(4);
    }

    [Fact]
    public void FirstIndexWhere_PredicateNoMatch_ReturnsMinusOne()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var result = source.FirstIndexWhere(x => x > 10);
        result.ShouldBe(-1);
    }

    [Fact]
    public void FirstIndexWhere_EmptySource_ReturnsMinusOne()
    {
        var source = Array.Empty<int>();
        var result = source.FirstIndexWhere(x => x > 0);
        result.ShouldBe(-1);
    }

    [Fact]
    public void FirstIndexWhere_MultipleMatches_ReturnsFirstMatchIndex()
    {
        var source = new[] { 1, 2, 3, 2, 5 };
        var result = source.FirstIndexWhere(x => x == 2);
        result.ShouldBe(1);
    }

    // ── ScrambledEquals ───────────────────────────────────────────────────────────
    [Fact]
    public void ScrambledEquals_IdenticalLists_ReturnsTrue()
    {
        var list1 = new[] { 1, 2, 3 };
        var list2 = new[] { 1, 2, 3 };
        var result = list1.ScrambledEquals(list2);
        result.ShouldBeTrue();
    }

    [Fact]
    public void ScrambledEquals_ScrambledOrder_ReturnsTrue()
    {
        var list1 = new[] { 1, 2, 3 };
        var list2 = new[] { 3, 1, 2 };
        var result = list1.ScrambledEquals(list2);
        result.ShouldBeTrue();
    }

    [Fact]
    public void ScrambledEquals_DifferentElements_ReturnsFalse()
    {
        var list1 = new[] { 1, 2, 3 };
        var list2 = new[] { 1, 2, 4 };
        var result = list1.ScrambledEquals(list2);
        result.ShouldBeFalse();
    }

    [Fact]
    public void ScrambledEquals_DifferentCounts_ReturnsFalse()
    {
        var list1 = new[] { 1, 2, 3 };
        var list2 = new[] { 1, 2, 3, 4 };
        var result = list1.ScrambledEquals(list2);
        result.ShouldBeFalse();
    }

    [Fact]
    public void ScrambledEquals_EmptyLists_ReturnsTrue()
    {
        var list1 = Array.Empty<int>();
        var list2 = Array.Empty<int>();
        var result = list1.ScrambledEquals(list2);
        result.ShouldBeTrue();
    }

    [Fact]
    public void ScrambledEquals_OneEmptyOneNot_ReturnsFalse()
    {
        var list1 = new[] { 1, 2, 3 };
        var list2 = Array.Empty<int>();
        var result = list1.ScrambledEquals(list2);
        result.ShouldBeFalse();
    }

    [Fact]
    public void ScrambledEquals_WithDuplicates_ReturnsTrue()
    {
        var list1 = new[] { 1, 2, 2, 3 };
        var list2 = new[] { 2, 3, 1, 2 };
        var result = list1.ScrambledEquals(list2);
        result.ShouldBeTrue();
    }

    [Fact]
    public void ScrambledEquals_DifferentDuplicateCounts_ReturnsFalse()
    {
        var list1 = new[] { 1, 2, 2, 3 };
        var list2 = new[] { 1, 2, 3, 3 };
        var result = list1.ScrambledEquals(list2);
        result.ShouldBeFalse();
    }

    [Fact]
    public void ScrambledEquals_List2HasExtra_ReturnsFalse()
    {
        var list1 = new[] { 1, 2 };
        var list2 = new[] { 1, 2, 3 };
        var result = list1.ScrambledEquals(list2);
        result.ShouldBeFalse();
    }

    // ── WhereNotNull ──────────────────────────────────────────────────────────────
    [Fact]
    public void WhereNotNull_MixedNullsAndValues_ReturnsOnlyNonNulls()
    {
        var source = new string?[] { "a", null, "b", null, "c" };
        var result = source.WhereNotNull().ToList();
        result.ShouldBe(new[] { "a", "b", "c" });
    }

    [Fact]
    public void WhereNotNull_AllNulls_ReturnsEmpty()
    {
        var source = new string?[] { null, null, null };
        var result = source.WhereNotNull().ToList();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void WhereNotNull_AllNonNulls_ReturnsAll()
    {
        var source = new string?[] { "a", "b", "c" };
        var result = source.WhereNotNull().ToList();
        result.ShouldBe(new[] { "a", "b", "c" });
    }

    [Fact]
    public void WhereNotNull_EmptySource_ReturnsEmpty()
    {
        var source = Array.Empty<string?>();
        var result = source.WhereNotNull().ToList();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void WhereNotNull_ReferenceTypes_FiltersNulls()
    {
        var source = new string?[] { "a", null, "b", null, "c" };
        var result = source.WhereNotNull().ToList();
        result.ShouldBe(new[] { "a", "b", "c" });
    }

    // ── GetAggregateHashCode ──────────────────────────────────────────────────────
    [Fact]
    public void GetAggregateHashCode_NullList_ReturnsZero()
    {
        IEnumerable<int>? list = null;
        var result = list!.GetAggregateHashCode();
        result.ShouldBe(0);
    }

    [Fact]
    public void GetAggregateHashCode_EmptyList_ReturnsSeedValue()
    {
        var list = Array.Empty<int>();
        var result = list.GetAggregateHashCode();
        result.ShouldBe(unchecked((int)0x2D2816FE));
    }

    [Fact]
    public void GetAggregateHashCode_SingleItem_ReturnsExpectedHashCode()
    {
        var list = new[] { 42 };
        var result = list.GetAggregateHashCode();
        var expected = unchecked((int)0x2D2816FE * 397) + 42.GetHashCode();
        result.ShouldBe(expected);
    }

    [Fact]
    public void GetAggregateHashCode_MultipleItems_ReturnsCombinedHashCode()
    {
        var list = new[] { 1, 2, 3 };
        var result = list.GetAggregateHashCode();
        result.ShouldNotBe(0);
        result.ShouldNotBe(unchecked((int)0x2D2816FE));
    }

    [Fact]
    public void GetAggregateHashCode_SameItemsDifferentOrder_ReturnsDifferentHashCode()
    {
        var list1 = new[] { 1, 2, 3 };
        var list2 = new[] { 3, 2, 1 };
        var result1 = list1.GetAggregateHashCode();
        var result2 = list2.GetAggregateHashCode();
        result1.ShouldNotBe(result2);
    }

    [Fact]
    public void GetAggregateHashCode_IdenticalLists_ReturnsSameHashCode()
    {
        var list1 = new[] { 1, 2, 3 };
        var list2 = new[] { 1, 2, 3 };
        var result1 = list1.GetAggregateHashCode();
        var result2 = list2.GetAggregateHashCode();
        result1.ShouldBe(result2);
    }

    [Fact]
    public void GetAggregateHashCode_WithDefaultValue_TreatsAsZero()
    {
        var list = new[] { 1, 0, 2 };
        var result = list.GetAggregateHashCode();
        result.ShouldNotBe(0);
    }

    [Fact]
    public void GetAggregateHashCode_WithNullReferenceType_TreatsAsZero()
    {
        var list = new string?[] { "a", null, "b" };
        var result = list.GetAggregateHashCode();
        result.ShouldNotBe(0);
    }

    [Fact]
    public void GetAggregateHashCode_AllDefaultValues_ReturnsExpectedHashCode()
    {
        var list = new[] { 0, 0, 0 };
        var result = list.GetAggregateHashCode();
        var expected = unchecked((int)0x2D2816FE);
        expected = unchecked((expected * 397) + 0);
        expected = unchecked((expected * 397) + 0);
        expected = unchecked((expected * 397) + 0);
        result.ShouldBe(expected);
    }

    // ── AllSame ───────────────────────────────────────────────────────────────────
    [Fact]
    public void AllSame_EmptyCollection_ReturnsTrue()
    {
        var items = Array.Empty<int>();
        var result = items.AllSame(x => x);
        result.ShouldBeTrue();
    }

    [Fact]
    public void AllSame_SingleItem_ReturnsTrue()
    {
        var items = new[] { 42 };
        var result = items.AllSame(x => x);
        result.ShouldBeTrue();
    }

    [Fact]
    public void AllSame_AllItemsSame_ReturnsTrue()
    {
        var items = new[] { 5, 5, 5, 5 };
        var result = items.AllSame(x => x);
        result.ShouldBeTrue();
    }

    [Fact]
    public void AllSame_DifferentItems_ReturnsFalse()
    {
        var items = new[] { 1, 2, 3 };
        var result = items.AllSame(x => x);
        result.ShouldBeFalse();
    }

    [Fact]
    public void AllSame_FirstTwoSameThirdDifferent_ReturnsFalse()
    {
        var items = new[] { 1, 1, 2 };
        var result = items.AllSame(x => x);
        result.ShouldBeFalse();
    }

    [Fact]
    public void AllSame_WithPredicate_AllSamePredicateValue_ReturnsTrue()
    {
        var items = new[] { "apple", "apricot", "avocado" };
        var result = items.AllSame(x => x[0]);
        result.ShouldBeTrue();
    }

    [Fact]
    public void AllSame_WithPredicate_DifferentPredicateValues_ReturnsFalse()
    {
        var items = new[] { "apple", "banana", "apricot" };
        var result = items.AllSame(x => x[0]);
        result.ShouldBeFalse();
    }

    [Fact]
    public void AllSame_ComplexPredicate_AllSameLength_ReturnsTrue()
    {
        var items = new[] { "cat", "dog", "bat" };
        var result = items.AllSame(x => x.Length);
        result.ShouldBeTrue();
    }

    [Fact]
    public void AllSame_ComplexPredicate_DifferentLengths_ReturnsFalse()
    {
        var items = new[] { "cat", "elephant", "dog" };
        var result = items.AllSame(x => x.Length);
        result.ShouldBeFalse();
    }

    [Fact]
    public void AllSame_PredicateReturnsNull_AllNull_ReturnsTrue()
    {
        var items = new[] { 1, 2, 3 };
        var result = items.AllSame(x => (object?)null);
        result.ShouldBeTrue();
    }

    [Fact]
    public void AllSame_PredicateReturnsNull_MixedWithNonNull_ReturnsFalse()
    {
        var items = new[] { 1, 2, 3 };
        var callCount = 0;
        var result = items.AllSame(x =>
        {
            callCount++;
            return callCount == 1 ? null : (object?)"value";
        });
        result.ShouldBeFalse();
    }

    [Fact]
    public void AllSame_TwoItems_Same_ReturnsTrue()
    {
        var items = new[] { 7, 7 };
        var result = items.AllSame(x => x);
        result.ShouldBeTrue();
    }

    [Fact]
    public void AllSame_TwoItems_Different_ReturnsFalse()
    {
        var items = new[] { 7, 8 };
        var result = items.AllSame(x => x);
        result.ShouldBeFalse();
    }

    // ── ToArray ───────────────────────────────────────────────────────────────────
    [Fact]
    public void ToArray_WithPredicate_TransformsAndReturnsArray()
    {
        var items = new[] { 1, 2, 3 };
        var result = items.ToArray(x => x * 2);
        result.ShouldBe(new[] { 2, 4, 6 });
    }

    [Fact]
    public void ToArray_WithPredicate_EmptySource_ReturnsEmptyArray()
    {
        var items = Array.Empty<int>();
        var result = items.ToArray(x => x * 2);
        result.ShouldBeEmpty();
    }

    [Fact]
    public void ToArray_WithPredicate_SingleItem_ReturnsTransformedSingleItem()
    {
        var items = new[] { 5 };
        var result = items.ToArray(x => x.ToString());
        result.ShouldBe(new[] { "5" });
    }

    [Fact]
    public void ToArray_WithPredicate_DifferentTypes_ReturnsArrayOfNewType()
    {
        var items = new[] { "1", "2", "3" };
        var result = items.ToArray(int.Parse);
        result.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void ToArray_WithPredicate_ComplexTransformation_ReturnsTransformedArray()
    {
        var items = new[] { "hello", "world", "test" };
        var result = items.ToArray(x => x.Length);
        result.ShouldBe(new[] { 5, 5, 4 });
    }

    // ── ToList ────────────────────────────────────────────────────────────────────
    [Fact]
    public void ToList_WithPredicate_TransformsAndReturnsList()
    {
        var items = new[] { 1, 2, 3 };
        var result = items.ToList(x => x * 2);
        result.ShouldBe(new[] { 2, 4, 6 });
    }

    [Fact]
    public void ToList_WithPredicate_EmptySource_ReturnsEmptyList()
    {
        var items = Array.Empty<int>();
        var result = items.ToList(x => x * 2);
        result.ShouldBeEmpty();
    }

    [Fact]
    public void ToList_WithPredicate_SingleItem_ReturnsTransformedSingleItem()
    {
        var items = new[] { 5 };
        var result = items.ToList(x => x.ToString());
        result.ShouldBe(new[] { "5" });
    }

    [Fact]
    public void ToList_WithPredicate_DifferentTypes_ReturnsListOfNewType()
    {
        var items = new[] { "1", "2", "3" };
        var result = items.ToList(int.Parse);
        result.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void ToList_WithPredicate_ComplexTransformation_ReturnsTransformedList()
    {
        var items = new[] { "hello", "world", "test" };
        var result = items.ToList(x => x.Length);
        result.ShouldBe(new[] { 5, 5, 4 });
    }
}
