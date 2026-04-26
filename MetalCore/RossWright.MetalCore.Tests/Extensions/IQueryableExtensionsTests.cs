namespace RossWright.MetalCore.Tests;

public class IQueryableExtensionsTests
{
    #region Skip Tests

    [Fact]
    public void Skip_NullCount_ReturnsOriginalSource()
    {
        // Arrange
        var source = new[] { 1, 2, 3 }.AsQueryable();

        // Act
        var result = source.Skip((int?)null);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Skip_ZeroCount_ReturnsOriginalSource()
    {
        // Arrange
        var source = new[] { 1, 2, 3 }.AsQueryable();

        // Act
        var result = source.Skip(0);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Skip_NegativeCount_ReturnsOriginalSource()
    {
        // Arrange
        var source = new[] { 1, 2, 3 }.AsQueryable();

        // Act
        var result = source.Skip(-1);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Skip_PositiveCount_SkipsElements()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 }.AsQueryable();

        // Act
        var result = source.Skip(2);

        // Assert
        result.ToList().ShouldBe(new[] { 3, 4, 5 });
    }

    [Fact]
    public void Skip_CountExceedsLength_ReturnsEmpty()
    {
        // Arrange
        var source = new[] { 1, 2, 3 }.AsQueryable();

        // Act
        var result = source.Skip(10);

        // Assert
        result.ToList().ShouldBeEmpty();
    }

    #endregion

    #region Take Tests

    [Fact]
    public void Take_NullCount_ReturnsOriginalSource()
    {
        // Arrange
        var source = new[] { 1, 2, 3 }.AsQueryable();

        // Act
        var result = source.Take((int?)null);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Take_ZeroCount_ReturnsOriginalSource()
    {
        // Arrange
        var source = new[] { 1, 2, 3 }.AsQueryable();

        // Act
        var result = source.Take(0);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Take_NegativeCount_ReturnsOriginalSource()
    {
        // Arrange
        var source = new[] { 1, 2, 3 }.AsQueryable();

        // Act
        var result = source.Take(-1);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Take_PositiveCount_TakesElements()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 }.AsQueryable();

        // Act
        var result = source.Take(2);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2 });
    }

    [Fact]
    public void Take_CountExceedsLength_ReturnsAll()
    {
        // Arrange
        var source = new[] { 1, 2, 3 }.AsQueryable();

        // Act
        var result = source.Take(10);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2, 3 });
    }

    #endregion

    #region WhereIf Tests

    [Fact]
    public void WhereIf_TrueFlag_AppliesPredicate()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 }.AsQueryable();

        // Act
        var result = source.WhereIf(true, x => x > 3);

        // Assert
        result.ToList().ShouldBe(new[] { 4, 5 });
    }

    [Fact]
    public void WhereIf_FalseFlag_ReturnsOriginalSource()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 }.AsQueryable();

        // Act
        var result = source.WhereIf(false, x => x > 3);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void WhereIf_FalseFlagWithElsePredicate_AppliesElsePredicate()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 }.AsQueryable();

        // Act
        var result = source.WhereIf(false, x => x > 3, x => x < 3);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2 });
    }

    [Fact]
    public void WhereIf_TrueFlagWithElsePredicate_AppliesPrimaryPredicate()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 }.AsQueryable();

        // Act
        var result = source.WhereIf(true, x => x > 3, x => x < 3);

        // Assert
        result.ToList().ShouldBe(new[] { 4, 5 });
    }

    #endregion

    #region WhereIfNotNull Tests

    [Fact]
    public void WhereIfNotNull_NullField_ReturnsOriginalSource()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 }.AsQueryable();

        // Act
        var result = source.WhereIfNotNull(null, x => x > 3);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void WhereIfNotNull_NonNullField_AppliesPredicate()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 }.AsQueryable();

        // Act
        var result = source.WhereIfNotNull("non-null", x => x > 3);

        // Assert
        result.ToList().ShouldBe(new[] { 4, 5 });
    }

    [Fact]
    public void WhereIfNotNull_NonNullFieldWithComplexObject_AppliesPredicate()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 }.AsQueryable();
        var complexObject = new { Value = 42 };

        // Act
        var result = source.WhereIfNotNull(complexObject, x => x < 3);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2 });
    }

    #endregion

    #region WhereIfNotNullOrEmpty Tests

    [Fact]
    public void WhereIfNotNullOrEmpty_NullCollection_ReturnsOriginalSource()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 }.AsQueryable();

        // Act
        var result = source.WhereIfNotNullOrEmpty(null!, x => x > 3);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void WhereIfNotNullOrEmpty_EmptyCollection_ReturnsOriginalSource()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 }.AsQueryable();
        var emptyCollection = new List<string>();

        // Act
        var result = source.WhereIfNotNullOrEmpty(emptyCollection, x => x > 3);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void WhereIfNotNullOrEmpty_NonEmptyCollection_AppliesPredicate()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 }.AsQueryable();
        var nonEmptyCollection = new List<string> { "item" };

        // Act
        var result = source.WhereIfNotNullOrEmpty(nonEmptyCollection, x => x > 3);

        // Assert
        result.ToList().ShouldBe(new[] { 4, 5 });
    }

    [Fact]
    public void WhereIfNotNullOrEmpty_CollectionWithMultipleItems_AppliesPredicate()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 }.AsQueryable();
        var collection = new List<int> { 1, 2, 3 };

        // Act
        var result = source.WhereIfNotNullOrEmpty(collection, x => x < 3);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2 });
    }

    #endregion

    #region OrderBy Tests

    [Fact]
    public void OrderBy_AscendingTrue_OrdersAscending()
    {
        // Arrange
        var source = new[] { 3, 1, 2 }.AsQueryable();

        // Act
        var result = source.OrderBy(x => x, true);

        // Assert
        result.ToList().ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void OrderBy_AscendingFalse_OrdersDescending()
    {
        // Arrange
        var source = new[] { 3, 1, 2 }.AsQueryable();

        // Act
        var result = source.OrderBy(x => x, false);

        // Assert
        result.ToList().ShouldBe(new[] { 3, 2, 1 });
    }

    [Fact]
    public void OrderBy_WithStrings_AscendingTrue_OrdersAscending()
    {
        // Arrange
        var source = new[] { "charlie", "alpha", "bravo" }.AsQueryable();

        // Act
        var result = source.OrderBy(x => x, true);

        // Assert
        result.ToList().ShouldBe(new[] { "alpha", "bravo", "charlie" });
    }

    [Fact]
    public void OrderBy_WithStrings_AscendingFalse_OrdersDescending()
    {
        // Arrange
        var source = new[] { "charlie", "alpha", "bravo" }.AsQueryable();

        // Act
        var result = source.OrderBy(x => x, false);

        // Assert
        result.ToList().ShouldBe(new[] { "charlie", "bravo", "alpha" });
    }

    [Fact]
    public void OrderBy_WithKeySelector_AscendingTrue_OrdersAscending()
    {
        // Arrange
        var source = new[]
        {
            new { Id = 3, Name = "charlie" },
            new { Id = 1, Name = "alpha" },
            new { Id = 2, Name = "bravo" }
        }.AsQueryable();

        // Act
        var result = source.OrderBy(x => x.Id, true);

        // Assert
        result.ToList().Select(x => x.Id).ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void OrderBy_WithKeySelector_AscendingFalse_OrdersDescending()
    {
        // Arrange
        var source = new[]
        {
            new { Id = 3, Name = "charlie" },
            new { Id = 1, Name = "alpha" },
            new { Id = 2, Name = "bravo" }
        }.AsQueryable();

        // Act
        var result = source.OrderBy(x => x.Id, false);

        // Assert
        result.ToList().Select(x => x.Id).ShouldBe(new[] { 3, 2, 1 });
    }

    #endregion

    #region ThenBy Tests

    [Fact]
    public void ThenBy_AscendingTrue_AppliesSecondaryAscendingSort()
    {
        // Arrange
        var source = new[]
        {
            new { Category = "A", Value = 3 },
            new { Category = "B", Value = 1 },
            new { Category = "A", Value = 1 },
            new { Category = "B", Value = 2 }
        }.AsQueryable();

        // Act
        var result = source.OrderBy(x => x.Category).ThenBy(x => x.Value, true);

        // Assert
        var list = result.ToList();
        list[0].Value.ShouldBe(1);
        list[1].Value.ShouldBe(3);
        list[2].Value.ShouldBe(1);
        list[3].Value.ShouldBe(2);
    }

    [Fact]
    public void ThenBy_AscendingFalse_AppliesSecondaryDescendingSort()
    {
        // Arrange
        var source = new[]
        {
            new { Category = "A", Value = 3 },
            new { Category = "B", Value = 1 },
            new { Category = "A", Value = 1 },
            new { Category = "B", Value = 2 }
        }.AsQueryable();

        // Act
        var result = source.OrderBy(x => x.Category).ThenBy(x => x.Value, false);

        // Assert
        var list = result.ToList();
        list[0].Value.ShouldBe(3);
        list[1].Value.ShouldBe(1);
        list[2].Value.ShouldBe(2);
        list[3].Value.ShouldBe(1);
    }

    [Fact]
    public void ThenBy_WithStrings_AscendingTrue_AppliesSecondaryAscendingSort()
    {
        // Arrange
        var source = new[]
        {
            new { Id = 1, Name = "charlie" },
            new { Id = 2, Name = "alpha" },
            new { Id = 1, Name = "alpha" },
            new { Id = 2, Name = "bravo" }
        }.AsQueryable();

        // Act
        var result = source.OrderBy(x => x.Id).ThenBy(x => x.Name, true);

        // Assert
        var names = result.ToList().Select(x => x.Name).ToList();
        names.ShouldBe(new[] { "alpha", "charlie", "alpha", "bravo" });
    }

    [Fact]
    public void ThenBy_WithStrings_AscendingFalse_AppliesSecondaryDescendingSort()
    {
        // Arrange
        var source = new[]
        {
            new { Id = 1, Name = "charlie" },
            new { Id = 2, Name = "alpha" },
            new { Id = 1, Name = "alpha" },
            new { Id = 2, Name = "bravo" }
        }.AsQueryable();

        // Act
        var result = source.OrderBy(x => x.Id).ThenBy(x => x.Name, false);

        // Assert
        var names = result.ToList().Select(x => x.Name).ToList();
        names.ShouldBe(new[] { "charlie", "alpha", "bravo", "alpha" });
    }

    [Fact]
    public void ThenBy_ChainedMultipleTimes_AppliesMultipleSortLevels()
    {
        // Arrange
        var source = new[]
        {
            new { A = 1, B = 2, C = 3 },
            new { A = 1, B = 1, C = 2 },
            new { A = 1, B = 1, C = 1 },
            new { A = 2, B = 1, C = 1 }
        }.AsQueryable();

        // Act
        var result = source.OrderBy(x => x.A).ThenBy(x => x.B, true).ThenBy(x => x.C, false);

        // Assert
        var list = result.ToList();
        list[0].C.ShouldBe(2);
        list[1].C.ShouldBe(1);
        list[2].C.ShouldBe(3);
        list[3].C.ShouldBe(1);
    }

    #endregion
}
