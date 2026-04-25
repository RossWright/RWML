namespace RossWright;

public class IEnumerableExtensionTests
{
    // ── WhereIf ───────────────────────────────────────────────────────────────────
    [Fact] public void WhereIf_FlagTrue_AppliesIfTrue()
    {
        var result = new[] { 1, 2, 3, 4 }.WhereIf(true, x => x > 2).ToList();
        result.ShouldBe(new[] { 3, 4 });
    }

    [Fact] public void WhereIf_FlagFalse_NoIfFalse_PassesAll()
    {
        var result = new[] { 1, 2, 3, 4 }.WhereIf(false, x => x > 2).ToList();
        result.ShouldBe(new[] { 1, 2, 3, 4 });
    }

    [Fact] public void WhereIf_FlagFalse_WithIfFalse_AppliesIfFalse()
    {
        var result = new[] { 1, 2, 3, 4 }.WhereIf(false, x => x > 3, x => x < 2).ToList();
        result.ShouldBe(new[] { 1 });
    }

    // ── ConcatAllowNull ───────────────────────────────────────────────────────────
    [Fact] public void ConcatAllowNull_BothNonNull_Concatenates()
    {
        var result = new[] { 1, 2 }.ConcatAllowNull(new[] { 3, 4 })!.ToList();
        result.ShouldBe(new[] { 1, 2, 3, 4 });
    }

    [Fact] public void ConcatAllowNull_FirstNull_ReturnsSecond()
    {
        var result = ((IEnumerable<int>?)null).ConcatAllowNull(new[] { 3, 4 })!.ToList();
        result.ShouldBe(new[] { 3, 4 });
    }

    [Fact] public void ConcatAllowNull_SecondNull_ReturnsFirst()
    {
        var result = new[] { 1, 2 }.ConcatAllowNull(null)!.ToList();
        result.ShouldBe(new[] { 1, 2 });
    }

    [Fact] public void ConcatAllowNull_BothNull_ReturnsNull()
    {
        var result = ((IEnumerable<int>?)null).ConcatAllowNull(null);
        result.ShouldBeNull();
    }

    // ── WithIndex ─────────────────────────────────────────────────────────────────
    [Fact] public void WithIndex_CorrectIndexPairs()
    {
        var result = new[] { "a", "b", "c" }.WithIndex().ToList();
        result[0].ShouldBe(("a", 0));
        result[1].ShouldBe(("b", 1));
        result[2].ShouldBe(("c", 2));
    }

    [Fact] public void WithIndex_EmptySequence_ReturnsEmpty()
        => Array.Empty<string>().WithIndex().ShouldBeEmpty();

    [Fact] public void WithIndex_NullSource_ReturnsEmpty()
        => ((IEnumerable<string>?)null)!.WithIndex().ShouldBeEmpty();

    // ── ForEach ───────────────────────────────────────────────────────────────────
    [Fact] public void ForEach_ActionInvokedForEveryElement()
    {
        var calls = new List<int>();
        new[] { 1, 2, 3 }.ForEach(x => calls.Add(x));
        calls.ShouldBe(new[] { 1, 2, 3 });
    }

    // ── WithEach ──────────────────────────────────────────────────────────────────
    [Fact] public void WithEach_ActionInvokedAndItemsYieldedUnchanged()
    {
        var sideEffect = new List<int>();
        var result = new[] { 1, 2, 3 }.WithEach(x => sideEffect.Add(x * 10)).ToList();
        result.ShouldBe(new[] { 1, 2, 3 });
        sideEffect.ShouldBe(new[] { 10, 20, 30 });
    }

    // ── ForEachAsync ──────────────────────────────────────────────────────────────
    [Fact] public async Task ForEachAsync_ActionCalledSequentially()
    {
        var calls = new List<int>();
        await new[] { 1, 2, 3 }.ForEachAsync(async x =>
        {
            await Task.Yield();
            calls.Add(x);
        });
        calls.ShouldBe(new[] { 1, 2, 3 });
    }

    // ── SelectDeep ────────────────────────────────────────────────────────────────
    private class Node(int value, IEnumerable<Node>? children = null)
    {
        public int Value { get; } = value;
        public IEnumerable<Node>? Children { get; } = children;
    }

    [Fact] public void SelectDeep_FlatList_ReturnsAllItems()
    {
        var nodes = new[] { new Node(1), new Node(2), new Node(3) };
        var result = nodes.SelectDeep(n => n.Children).Select(n => n.Value).ToList();
        result.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact] public void SelectDeep_SingleLevelNesting_FlattensDepthFirst()
    {
        var nodes = new[] {
            new Node(1, new[] { new Node(2), new Node(3) }),
            new Node(4)
        };
        var result = nodes.SelectDeep(n => n.Children).Select(n => n.Value).ToList();
        result.ShouldBe(new[] { 1, 2, 3, 4 });
    }

    [Fact] public void SelectDeep_MultiLevelNesting_FlattensDepthFirst()
    {
        var nodes = new[] {
            new Node(1, new[] {
                new Node(2, new[] { new Node(3) }),
                new Node(4)
            })
        };
        var result = nodes.SelectDeep(n => n.Children).Select(n => n.Value).ToList();
        result.ShouldBe(new[] { 1, 2, 3, 4 });
    }

    [Fact] public void SelectDeep_LeafNodesWithNoChildren_ReturnsLeaf()
    {
        var nodes = new[] { new Node(42) };
        var result = nodes.SelectDeep(n => n.Children).Select(n => n.Value).ToList();
        result.ShouldBe(new[] { 42 });
    }

    // ── Without ───────────────────────────────────────────────────────────────────
    [Fact] public void Without_RemovesMatchingValues()
    {
        var result = new[] { 1, 2, 3, 2, 1 }.Without(1, 2).ToList();
        result.ShouldBe(new[] { 3 });
    }

    [Fact] public void Without_NoMatchingValues_ReturnsOriginal()
    {
        var result = new[] { 1, 2, 3 }.Without(9).ToList();
        result.ShouldBe(new[] { 1, 2, 3 });
    }

    // ── OrderBy(bool) / ThenBy(bool) ─────────────────────────────────────────────
    [Fact] public void OrderBy_Ascending_SortsAscending()
    {
        var result = new[] { 3, 1, 2 }.OrderBy(x => x, true).ToList();
        result.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact] public void OrderBy_Descending_SortsDescending()
    {
        var result = new[] { 3, 1, 2 }.OrderBy(x => x, false).ToList();
        result.ShouldBe(new[] { 3, 2, 1 });
    }

    [Fact] public void ThenBy_Ascending_AppliesSecondarySort()
    {
        var result = new[] { (2, 'b'), (2, 'a'), (1, 'c') }
            .OrderBy(x => x.Item1, true)
            .ThenBy(x => x.Item2, true)
            .ToList();
        result[1].ShouldBe((2, 'a'));
        result[2].ShouldBe((2, 'b'));
    }

    [Fact] public void ThenBy_Descending_AppliesSecondarySort()
    {
        var result = new[] { (2, 'a'), (2, 'b'), (1, 'c') }
            .OrderBy(x => x.Item1, true)
            .ThenBy(x => x.Item2, false)
            .ToList();
        result[1].ShouldBe((2, 'b'));
        result[2].ShouldBe((2, 'a'));
    }

    // ── FirstIndexWhere ───────────────────────────────────────────────────────────
    [Fact] public void FirstIndexWhere_Found_ReturnsIndex()
        => new[] { 10, 20, 30, 40 }.FirstIndexWhere(x => x == 30).ShouldBe(2);

    [Fact] public void FirstIndexWhere_NotFound_ReturnsMinusOne()
        => new[] { 1, 2, 3 }.FirstIndexWhere(x => x == 99).ShouldBe(-1);

    [Fact] public void FirstIndexWhere_EmptySequence_ReturnsMinusOne()
        => Array.Empty<int>().FirstIndexWhere(x => x == 1).ShouldBe(-1);

    // ── ScrambledEquals ───────────────────────────────────────────────────────────
    [Fact] public void ScrambledEquals_SameElementsDifferentOrder_ReturnsTrue()
        => new[] { 3, 1, 2 }.ScrambledEquals(new[] { 1, 2, 3 }).ShouldBeTrue();

    [Fact] public void ScrambledEquals_DifferentElements_ReturnsFalse()
        => new[] { 1, 2, 3 }.ScrambledEquals(new[] { 1, 2, 4 }).ShouldBeFalse();

    [Fact] public void ScrambledEquals_DifferentCounts_ReturnsFalse()
        => new[] { 1, 2, 3 }.ScrambledEquals(new[] { 1, 2 }).ShouldBeFalse();
}

// ── P2-C: WhereNotNull ────────────────────────────────────────────────────────
public class WhereNotNullTests
{
    [Fact] public void WhereNotNull_RemovesNullEntries()
        => new string?[] { "a", null, "b" }.WhereNotNull().ShouldBe(new[] { "a", "b" });

    [Fact] public void WhereNotNull_AllNull_ReturnsEmpty()
        => new string?[] { null, null }.WhereNotNull().ShouldBeEmpty();

    [Fact] public void WhereNotNull_NoNulls_ReturnsAll()
        => new[] { "a", "b" }.WhereNotNull().ShouldBe(new[] { "a", "b" });
}

// ── P2-C: GetAggregateHashCode ────────────────────────────────────────────────
public class GetAggregateHashCodeTests
{
    [Fact] public void GetAggregateHashCode_SameElements_ReturnsSameValue()
    {
        var a = new int[] { 1, 2, 3 }.GetAggregateHashCode();
        var b = new int[] { 1, 2, 3 }.GetAggregateHashCode();
        a.ShouldBe(b);
    }

    [Fact] public void GetAggregateHashCode_DifferentElements_ReturnsDifferentValue()
    {
        var a = new int[] { 1, 2, 3 }.GetAggregateHashCode();
        var b = new int[] { 1, 2, 4 }.GetAggregateHashCode();
        a.ShouldNotBe(b);
    }

    [Fact] public void GetAggregateHashCode_EmptySequence_ReturnsWithoutThrowing()
    {
        var result = new List<int>().GetAggregateHashCode();
        result.ShouldBeOfType<int>();
    }
}

// ── P2-C: AllSame ─────────────────────────────────────────────────────────────
public class AllSameTests
{
    [Fact] public void AllSame_AllItemsHaveSameValue_ReturnsTrue()
        => new[] { 1, 1, 1 }.AllSame(x => x).ShouldBeTrue();

    [Fact] public void AllSame_ItemsDiffer_ReturnsFalse()
        => new[] { 1, 2, 1 }.AllSame(x => x).ShouldBeFalse();

    [Fact] public void AllSame_SingleItem_ReturnsTrue()
        => new[] { 42 }.AllSame(x => x).ShouldBeTrue();

    [Fact] public void AllSame_EmptySequence_ReturnsTrue()
        => new int[] { }.AllSame(x => x).ShouldBeTrue();

    [Fact] public void AllSame_WithSelectorProjection_ComparesProjectedValues()
        => new[] { "ab", "ac" }.AllSame(s => s[0]).ShouldBeTrue();
}

// ── P2-C: ToArray<TIn,TOut> / ToList<TIn,TOut> ───────────────────────────────
public class ProjectionToArrayListTests
{
    [Fact] public void ToArray_WithSelector_ReturnsProjectedArray()
        => new[] { 1, 2, 3 }.ToArray(x => x.ToString()).ShouldBe(new[] { "1", "2", "3" });

    [Fact] public void ToArray_EmptySource_ReturnsEmptyArray()
        => new int[] { }.ToArray(x => x.ToString()).ShouldBeEmpty();

    [Fact] public void ToList_WithSelector_ReturnsProjectedList()
        => new[] { 1, 2, 3 }.ToList(x => x * 2).ShouldBe(new[] { 2, 4, 6 });

    [Fact] public void ToList_EmptySource_ReturnsEmptyList()
        => new int[] { }.ToList(x => x * 2).ShouldBeEmpty();
}
