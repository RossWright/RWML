namespace RossWright.MetalCore.Tests.CloneAsExtension;

public class CloneAsPositionalRecordTests
{
    // ── Pure positional records ──────────────────────────────────────────────

    [Fact]
    public void CloneAs_PositionalSingleProp_ClonesViaConstructor()
    {
        var source = new BasicTypeOneProp { Value = 42 };
        var result = source.CloneAs<PositionalSingleProp>();

        result.Value.ShouldBe(42);
    }

    [Fact]
    public void CloneAs_PositionalTwoProps_ClonesAllParams()
    {
        var source = new { Value = 7, Name = "Alice" };
        var result = source.CloneAs<PositionalTwoProps>();

        result.Value.ShouldBe(7);
        result.Name.ShouldBe("Alice");
    }

    [Fact]
    public void CloneAs_PositionalWithNullable_NullValueMapsCorrectly()
    {
        var source = new { Id = 1, Description = (string?)null };
        var result = source.CloneAs<PositionalWithNullable>();

        result.Id.ShouldBe(1);
        result.Description.ShouldBeNull();
    }

    [Fact]
    public void CloneAs_PositionalWithDateOnly_ClonesDateOnlyParam()
    {
        var birthDate = new DateOnly(1990, 6, 15);
        var source = new { Id = 3, BirthDate = birthDate };
        var result = source.CloneAs<PositionalWithDateOnly>();

        result.Id.ShouldBe(3);
        result.BirthDate.ShouldBe(birthDate);
    }

    // ── Collection overloads ─────────────────────────────────────────────────

    [Fact]
    public void CloneAs_PositionalRecord_Collection_ClonesEachElement()
    {
        var sources = new[]
        {
            new BasicTypeOneProp { Value = 1 },
            new BasicTypeOneProp { Value = 2 },
        };
        var results = sources.CloneAs<PositionalSingleProp>();

        results.Length.ShouldBe(2);
        results[0].Value.ShouldBe(1);
        results[1].Value.ShouldBe(2);
    }

    // ── Hybrid record (positional ctor + extra init prop) ────────────────────

    [Fact]
    public void CloneAs_HybridRecord_SetsConstructorParamsAndExtraInitProp()
    {
        var source = new { Id = 5, Name = "Bob", Extra = "bonus" };
        var result = source.CloneAs<HybridRecord>();

        result.Id.ShouldBe(5);
        result.Name.ShouldBe("Bob");
        result.Extra.ShouldBe("bonus");
    }

    // ── Multiple constructors: prefer most-satisfiable ───────────────────────

    [Fact]
    public void CloneAs_MultipleConstructors_PrefersMostParamsSatisfied()
    {
        var source = new { Id = 10, Name = "Carol" };
        var result = source.CloneAs<TwoCtorClass>();

        result.Id.ShouldBe(10);
        result.Name.ShouldBe("Carol");
    }

    // ── init callback still works ────────────────────────────────────────────

    [Fact]
    public void CloneAs_PositionalRecord_WithInitCallback_CallbackRunsAfterConstruction()
    {
        var source = new { Value = 1, Name = "Alice" };
        var result = source.CloneAs<PositionalTwoProps>(r =>
        {
            r.Value.ShouldBe(1);
        });

        result.Value.ShouldBe(1);
    }

    // ── Error cases ──────────────────────────────────────────────────────────

    [Fact]
    public void CloneAs_PositionalRecord_MissingSourceProperty_ThrowsInvalidOperationException()
    {
        var source = new BasicTypeOneProp { Value = 99 };
        Should.Throw<InvalidOperationException>(() => source.CloneAs<MismatchedCtorRecord>());
    }

    [Fact]
    public void CloneAs_PositionalRecord_IncompatibleType_ThrowsInvalidCastException()
    {
        // source.Value is int; TypeMismatchRecord expects DateOnly — no TypeConverter exists for int->DateOnly
        var source = new BasicTypeOneProp { Value = 1 };
        Should.Throw<InvalidCastException>(() => source.CloneAs<TypeMismatchRecord>());
    }

    // CreateInstance matches ctor parameter names case-insensitively
    [Fact]
    public void CloneAs_PositionalRecord_CaseInsensitiveParamMatch()
    {
        // Source has "id" (lowercase); target ctor expects "Id" — should still match
        var source = new { id = 5, birthDate = new DateOnly(2000, 1, 1) };
        var result = source.CloneAs<PositionalWithDateOnly>();

        result.Id.ShouldBe(5);
        result.BirthDate.ShouldBe(new DateOnly(2000, 1, 1));
    }

    // Nullable ctor parameter receives a non-null source value
    [Fact]
    public void CloneAs_PositionalRecord_NullableCtorParam_WithValue_MapsCorrectly()
    {
        var source = new { Id = 7, Tag = "hello" };
        var result = source.CloneAs<PositionalWithNullableCtorParam>();

        result.Id.ShouldBe(7);
        result.Tag.ShouldBe("hello");
    }

    // Nullable ctor parameter receives a null source value
    [Fact]
    public void CloneAs_PositionalRecord_NullableCtorParam_WithNull_MapsCorrectly()
    {
        var source = new { Id = 3, Tag = (string?)null };
        var result = source.CloneAs<PositionalWithNullableCtorParam>();

        result.Id.ShouldBe(3);
        result.Tag.ShouldBeNull();
    }

    // When only the smaller of two constructors is fully satisfiable, that one is used
    [Fact]
    public void CloneAs_MultipleConstructors_FallsBackToSatisfiableSmaller()
    {
        // TwoCtorOnlySmallSatisfiable: ctor(int id) vs ctor(int id, string name, string extra)
        // Source has only 'Id' — the 1-param ctor is satisfiable; the 3-param one is not.
        var source = new { Id = 42 };
        var result = source.CloneAs<TwoCtorOnlySmallSatisfiable>();

        result.Id.ShouldBe(42);
        result.Name.ShouldBe(string.Empty);
    }

    // CloneAs on a positional record returns a distinct instance
    [Fact]
    public void CloneAs_PositionalRecord_ReturnsNewInstance()
    {
        var source = new { Value = 1, Name = "X" };
        var r1 = source.CloneAs<PositionalTwoProps>();
        var r2 = source.CloneAs<PositionalTwoProps>();

        r1.ShouldNotBeSameAs(r2);
    }
}
