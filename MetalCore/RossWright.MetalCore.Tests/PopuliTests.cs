using RossWright;

namespace RossWright.MetalCore.Tests;

public class PopuliTests
{
    [Fact]
    public void NextBool_Default_ReturnsBoolean()
    {
        // Act
        var result = Populi.NextBool();

        // Assert
        result.ShouldBeOfType<bool>();
    }

    [Fact]
    public void NextBool_WithZeroBias_ReturnsFalse()
    {
        // Arrange
        const int attempts = 100;
        var trueCount = 0;

        // Act
        for (int i = 0; i < attempts; i++)
        {
            if (Populi.NextBool(0))
                trueCount++;
        }

        // Assert
        trueCount.ShouldBe(0);
    }

    [Fact]
    public void NextBool_With100Bias_ReturnsTrue()
    {
        // Arrange
        const int attempts = 100;
        var trueCount = 0;

        // Act
        for (int i = 0; i < attempts; i++)
        {
            if (Populi.NextBool(100))
                trueCount++;
        }

        // Assert
        trueCount.ShouldBe(attempts);
    }

    [Fact]
    public void NextBool_With50Bias_ReturnsBalancedResults()
    {
        // Arrange
        const int attempts = 1000;
        var trueCount = 0;

        // Act
        for (int i = 0; i < attempts; i++)
        {
            if (Populi.NextBool(50))
                trueCount++;
        }

        // Assert - Should be roughly half, allowing some variance
        trueCount.ShouldBeInRange(300, 700);
    }

    [Fact]
    public void NextBool_With75Bias_ReturnsMostlyTrue()
    {
        // Arrange
        const int attempts = 1000;
        var trueCount = 0;

        // Act
        for (int i = 0; i < attempts; i++)
        {
            if (Populi.NextBool(75))
                trueCount++;
        }

        // Assert - Should be around 75%, allowing variance
        trueCount.ShouldBeInRange(600, 900);
    }

    [Fact]
    public void NextInt_WithMaxValue_ReturnsNonNegativeInteger()
    {
        // Act
        var result = Populi.NextInt(100);

        // Assert
        result.ShouldBeInRange(0, 99);
    }

    [Fact]
    public void NextInt_Default_ReturnsNonNegativeInteger()
    {
        // Act
        var result = Populi.NextInt();

        // Assert
        result.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void NextInt_WithMaxValue_RespectsBound()
    {
        // Arrange
        const int max = 10;
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextInt(max);
            result.ShouldBeInRange(0, max - 1);
        }
    }

    [Fact]
    public void NextInt_WithMinAndMax_ReturnsValueInRange()
    {
        // Arrange
        const int min = 10;
        const int max = 20;

        // Act
        var result = Populi.NextInt(min, max);

        // Assert
        result.ShouldBeInRange(min, max - 1);
    }

    [Fact]
    public void NextInt_WithMinAndMax_RespectsBounds()
    {
        // Arrange
        const int min = 50;
        const int max = 60;
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextInt(min, max);
            result.ShouldBeInRange(min, max - 1);
        }
    }

    [Fact]
    public void NextInt_WithMinAndMax_ProducesVariedResults()
    {
        // Arrange
        const int min = 0;
        const int max = 100;
        const int attempts = 100;
        var results = new HashSet<int>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.NextInt(min, max));
        }

        // Assert - Should produce at least some variety
        results.Count.ShouldBeGreaterThan(10);
    }

    [Fact]
    public void NextDouble_WithMaxValue_ReturnsNonNegativeDouble()
    {
        // Act
        var result = Populi.NextDouble(100.0);

        // Assert
        result.ShouldBeInRange(0.0, 100.0);
    }

    [Fact]
    public void NextDouble_Default_ReturnsNonNegativeDouble()
    {
        // Act
        var result = Populi.NextDouble();

        // Assert
        result.ShouldBeGreaterThanOrEqualTo(0.0);
    }

    [Fact]
    public void NextDouble_WithMaxValue_RespectsBound()
    {
        // Arrange
        const double max = 10.0;
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextDouble(max);
            result.ShouldBeInRange(0.0, max);
        }
    }

    [Fact]
    public void NextDouble_WithMinAndMax_ReturnsValueInRange()
    {
        // Arrange
        const double min = 10.0;
        const double max = 20.0;

        // Act
        var result = Populi.NextDouble(min, max);

        // Assert
        result.ShouldBeInRange(min, max);
    }

    [Fact]
    public void NextDouble_WithMinAndMax_RespectsBounds()
    {
        // Arrange
        const double min = 50.0;
        const double max = 60.0;
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextDouble(min, max);
            result.ShouldBeInRange(min, max);
        }
    }

    [Fact]
    public void NextDouble_WithMinAndMax_ProducesVariedResults()
    {
        // Arrange
        const double min = 0.0;
        const double max = 100.0;
        const int attempts = 100;
        var results = new HashSet<double>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.NextDouble(min, max));
        }

        // Assert - Should produce varied results
        results.Count.ShouldBeGreaterThan(90);
    }

    [Fact]
    public void NextDouble_WithNegativeRange_HandlesCorrectly()
    {
        // Arrange
        const double min = -50.0;
        const double max = 50.0;
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextDouble(min, max);
            result.ShouldBeInRange(min, max);
        }
    }

    [Fact]
    public void NextInt_WithNegativeRange_HandlesCorrectly()
    {
        // Arrange
        const int min = -50;
        const int max = 50;
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextInt(min, max);
            result.ShouldBeInRange(min, max - 1);
        }
    }

    [Fact]
    public void NextPrice_WithDefaultMax_ReturnsNonNegativePrice()
    {
        // Act
        var result = Populi.NextPrice();

        // Assert
        result.ShouldBeGreaterThanOrEqualTo(0.0);
    }

    [Fact]
    public void NextPrice_WithMaxValue_ReturnsValueWithinBounds()
    {
        // Arrange
        const double max = 100.0;

        // Act
        var result = Populi.NextPrice(max);

        // Assert
        result.ShouldBeInRange(0.0, max);
    }

    [Fact]
    public void NextPrice_WithMaxValue_HasTwoDecimalPlaces()
    {
        // Arrange
        const double max = 100.0;
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextPrice(max);
            var rounded = Math.Round(result, 2);
            result.ShouldBe(rounded);
        }
    }

    [Fact]
    public void NextPrice_WithMinAndMax_ReturnsValueInRange()
    {
        // Arrange
        const double min = 10.0;
        const double max = 50.0;

        // Act
        var result = Populi.NextPrice(min, max);

        // Assert
        result.ShouldBeInRange(min, max);
    }

    [Fact]
    public void NextPrice_WithMinAndMax_RespectsBounds()
    {
        // Arrange
        const double min = 20.0;
        const double max = 100.0;
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextPrice(min, max);
            result.ShouldBeInRange(min, max);
        }
    }

    [Fact]
    public void NextPrice_WithMinAndMax_ProducesVariedResults()
    {
        // Arrange
        const double min = 0.0;
        const double max = 100.0;
        const int attempts = 100;
        var results = new HashSet<double>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.NextPrice(min, max));
        }

        // Assert - Should produce varied results
        results.Count.ShouldBeGreaterThan(10);
    }

    [Fact]
    public void NextLetter_ReturnsASCIILetter()
    {
        // Act
        var result = Populi.NextLetter();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.Length.ShouldBe(1);
        char c = result[0];
        bool isLetter = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        isLetter.ShouldBeTrue();
    }

    [Fact]
    public void NextLetter_ReturnsVariedResults()
    {
        // Arrange
        const int attempts = 100;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.NextLetter());
        }

        // Assert - Should produce at least some variety
        results.Count.ShouldBeGreaterThan(5);
    }

    [Fact]
    public void NextLetter_ProducesBothUpperAndLowerCase()
    {
        // Arrange
        const int attempts = 1000;
        var hasUpper = false;
        var hasLower = false;

        // Act
        for (int i = 0; i < attempts; i++)
        {
            var letter = Populi.NextLetter();
            if (char.IsUpper(letter[0]))
                hasUpper = true;
            if (char.IsLower(letter[0]))
                hasLower = true;

            if (hasUpper && hasLower)
                break;
        }

        // Assert
        hasUpper.ShouldBeTrue();
        hasLower.ShouldBeTrue();
    }

    [Fact]
    public void Next_WithEnum_ReturnsEnumValue()
    {
        // Act
        var result = Populi.Next<DayOfWeek>();

        // Assert
        Enum.IsDefined(typeof(DayOfWeek), result).ShouldBeTrue();
    }

    [Fact]
    public void Next_WithEnum_ReturnsVariedResults()
    {
        // Arrange
        const int attempts = 100;
        var results = new HashSet<DayOfWeek>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.Next<DayOfWeek>());
        }

        // Assert - Should produce at least some variety
        results.Count.ShouldBeGreaterThan(1);
    }

    [Fact]
    public void Next_WithSingleValueEnum_ReturnsThatValue()
    {
        // Arrange & Act
        var result = Populi.Next<SingleValueEnum>();

        // Assert
        result.ShouldBe(SingleValueEnum.OnlyValue);
    }

    [Fact]
    public void Next_WithSmallEnum_ReturnsAllValuesEventually()
    {
        // Arrange
        const int attempts = 50;
        var results = new HashSet<ConsoleColor>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.Next<ConsoleColor>());
        }

        // Assert - Should produce variety from ConsoleColor enum
        results.Count.ShouldBeGreaterThan(3);
    }

    private enum SingleValueEnum
    {
        OnlyValue
    }

    [Fact]
    public void OneOf_WithEnumerable_ReturnsElementFromCollection()
    {
        // Arrange
        var choices = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = Populi.OneOf((IEnumerable<int>)choices);

        // Assert
        choices.Contains(result).ShouldBeTrue();
    }

    [Fact]
    public void OneOf_WithEnumerable_ReturnsVariedResults()
    {
        // Arrange
        var choices = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        const int attempts = 100;
        var results = new HashSet<int>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.OneOf((IEnumerable<int>)choices));
        }

        // Assert - Should produce some variety
        results.Count.ShouldBeGreaterThan(3);
    }

    [Fact]
    public void OneOf_WithSingleElementEnumerable_ReturnsThatElement()
    {
        // Arrange
        var choices = new List<string> { "only" };

        // Act
        var result = Populi.OneOf((IEnumerable<string>)choices);

        // Assert
        result.ShouldBe("only");
    }

    [Fact]
    public void OneOf_WithStringEnumerable_ReturnsStringFromCollection()
    {
        // Arrange
        var choices = new List<string> { "apple", "banana", "cherry" };

        // Act
        var result = Populi.OneOf((IEnumerable<string>)choices);

        // Assert
        choices.Contains(result).ShouldBeTrue();
    }

    [Fact]
    public void OneOf_WithEnumerable_TwoElements_ReturnsBothEventually()
    {
        // Arrange
        var choices = new List<int> { 1, 2 };
        const int attempts = 50;
        var results = new HashSet<int>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.OneOf((IEnumerable<int>)choices));
        }

        // Assert - Should eventually get both values
        results.Count.ShouldBe(2);
    }

    [Fact]
    public void OneOf_WithEnumerable_WorksWithArray()
    {
        // Arrange
        var choices = new[] { "first", "second", "third" };

        // Act
        var result = Populi.OneOf((IEnumerable<string>)choices);

        // Assert
        choices.ShouldContain(result);
    }

    [Fact]
    public void OneOf_WithEnumerable_WorksWithHashSet()
    {
        // Arrange
        var choices = new HashSet<int> { 10, 20, 30 };

        // Act
        var result = Populi.OneOf((IEnumerable<int>)choices);

        // Assert
        choices.ShouldContain(result);
    }

    [Fact]
    public void OneOf_WithParamsArray_ReturnsElementFromArray()
    {
        // Arrange
        var choice1 = "apple";
        var choice2 = "banana";
        var choice3 = "cherry";

        // Act
        var result = Populi.OneOf(choice1, choice2, choice3);

        // Assert
        new[] { choice1, choice2, choice3 }.ShouldContain(result);
    }

    [Fact]
    public void OneOf_WithParamsArray_SingleElement_ReturnsThatElement()
    {
        // Arrange
        var choice = "only";

        // Act
        var result = Populi.OneOf(choice);

        // Assert
        result.ShouldBe(choice);
    }

    [Fact]
    public void OneOf_WithParamsArray_ReturnsVariedResults()
    {
        // Arrange
        var choices = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        const int attempts = 100;
        var results = new HashSet<int>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.OneOf(choices));
        }

        // Assert - Should produce some variety
        results.Count.ShouldBeGreaterThan(3);
    }

    [Fact]
    public void OneOf_WithParamsArray_TwoElements_ReturnsBothEventually()
    {
        // Arrange
        const int attempts = 50;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.OneOf("first", "second"));
        }

        // Assert - Should eventually get both values
        results.Count.ShouldBe(2);
    }

    [Fact]
    public void OneOf_WithParamsArray_WorksWithDoubles()
    {
        // Arrange
        var choices = new[] { 1.5, 2.5, 3.5 };

        // Act
        var result = Populi.OneOf(1.5, 2.5, 3.5);

        // Assert
        choices.ShouldContain(result);
    }

    [Fact]
    public void OneOf_WithParamsArray_WorksWithObjects()
    {
        // Arrange
        var obj1 = new { Name = "first" };
        var obj2 = new { Name = "second" };

        // Act
        var result = Populi.OneOf(obj1, obj2);

        // Assert
        (result == obj1 || result == obj2).ShouldBeTrue();
    }

    [Fact]
    public void OneOf_WithWeightedChoices_ReturnsElementFromChoices()
    {
        // Arrange
        var choice1 = ("apple", 1);
        var choice2 = ("banana", 1);
        var choice3 = ("cherry", 1);

        // Act
        var result = Populi.OneOf(choice1, choice2, choice3);

        // Assert
        new[] { "apple", "banana", "cherry" }.ShouldContain(result);
    }

    [Fact]
    public void OneOf_WithWeightedChoices_SingleChoice_ReturnsThatChoice()
    {
        // Arrange
        var choice = ("only", 10);

        // Act
        var result = Populi.OneOf(choice);

        // Assert
        result.ShouldBe("only");
    }

    [Fact]
    public void OneOf_WithWeightedChoices_ZeroWeight_CanStillBeSelected()
    {
        // Arrange
        var choice1 = ("zero", 0);
        var choice2 = ("positive", 10);
        const int attempts = 100;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.OneOf(choice1, choice2));
        }

        // Assert - Should mostly get "positive" but "zero" might appear
        results.ShouldContain("positive");
    }

    [Fact]
    public void OneOf_WithWeightedChoices_NegativeWeightTreatedAsZero()
    {
        // Arrange
        var choice1 = ("negative", -5);
        var choice2 = ("positive", 10);
        const int attempts = 100;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.OneOf(choice1, choice2));
        }

        // Assert - Should mostly get "positive"
        results.ShouldContain("positive");
    }

    [Fact]
    public void OneOf_WithWeightedChoices_HigherWeightMoreLikely()
    {
        // Arrange
        var choice1 = ("rare", 1);
        var choice2 = ("common", 99);
        const int attempts = 1000;
        var commonCount = 0;

        // Act
        for (int i = 0; i < attempts; i++)
        {
            if (Populi.OneOf(choice1, choice2) == "common")
                commonCount++;
        }

        // Assert - Should be significantly biased toward "common"
        commonCount.ShouldBeGreaterThan(800);
    }

    [Fact]
    public void OneOf_WithWeightedChoices_AllZeroWeights_ReturnsLastChoice()
    {
        // Arrange
        var choice1 = ("first", 0);
        var choice2 = ("second", 0);
        var choice3 = ("third", 0);

        // Act
        var result = Populi.OneOf(choice1, choice2, choice3);

        // Assert
        new[] { "first", "second", "third" }.ShouldContain(result);
    }

    [Fact]
    public void OneOf_WithWeightedChoices_AllNegativeWeights_ReturnsFirstChoice()
    {
        // Arrange
        var choice1 = ("first", -5);
        var choice2 = ("second", -10);

        // Act
        var result = Populi.OneOf(choice1, choice2);

        // Assert
        new[] { "first", "second" }.ShouldContain(result);
    }

    [Fact]
    public void OneOf_WithWeightedChoices_MixedWeights_ReturnsValidChoice()
    {
        // Arrange
        var choice1 = ("negative", -5);
        var choice2 = ("zero", 0);
        var choice3 = ("positive", 10);
        const int attempts = 100;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.OneOf(choice1, choice2, choice3));
        }

        // Assert
        results.ShouldContain("positive");
    }

    [Fact]
    public void OneOf_WithWeightedChoices_MultipleChoicesSameWeight_DistributesEvenly()
    {
        // Arrange
        var choice1 = ("A", 10);
        var choice2 = ("B", 10);
        var choice3 = ("C", 10);
        const int attempts = 300;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.OneOf(choice1, choice2, choice3));
        }

        // Assert - Should eventually get all three choices
        results.Count.ShouldBe(3);
    }

    [Fact]
    public void OneOf_WithWeightedChoices_LargeWeights_HandlesCorrectly()
    {
        // Arrange
        var choice1 = ("small", 1);
        var choice2 = ("large", 1000000);
        const int attempts = 100;
        var largeCount = 0;

        // Act
        for (int i = 0; i < attempts; i++)
        {
            if (Populi.OneOf(choice1, choice2) == "large")
                largeCount++;
        }

        // Assert - Should almost always get "large"
        largeCount.ShouldBeGreaterThan(95);
    }

    [Fact]
    public void SomeOf_CountLessThanChoices_ReturnsCorrectCount()
    {
        // Arrange
        var choices = new[] { 1, 2, 3, 4, 5 };
        const int count = 3;

        // Act
        var result = Populi.SomeOf(count, choices);

        // Assert
        result.Length.ShouldBe(count);
        result.ShouldBeSubsetOf(choices);
    }

    [Fact]
    public void SomeOf_CountEqualToChoices_ReturnsAllChoices()
    {
        // Arrange
        var choices = new[] { 1, 2, 3, 4, 5 };
        const int count = 5;

        // Act
        var result = Populi.SomeOf(count, choices);

        // Assert
        result.Length.ShouldBe(count);
        result.ShouldBe(choices, ignoreOrder: true);
    }

    [Fact]
    public void SomeOf_CountGreaterThanChoices_ReturnsAllChoices()
    {
        // Arrange
        var choices = new[] { 1, 2, 3 };
        const int count = 10;

        // Act
        var result = Populi.SomeOf(count, choices);

        // Assert
        result.Length.ShouldBe(choices.Length);
        result.ShouldBe(choices, ignoreOrder: true);
    }

    [Fact]
    public void SomeOf_CountZero_ReturnsEmptyArray()
    {
        // Arrange
        var choices = new[] { 1, 2, 3, 4, 5 };
        const int count = 0;

        // Act
        var result = Populi.SomeOf(count, choices);

        // Assert
        result.Length.ShouldBe(0);
    }

    [Fact]
    public void SomeOf_ReturnsUniqueElements()
    {
        // Arrange
        var choices = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        const int count = 5;

        // Act
        var result = Populi.SomeOf(count, choices);

        // Assert
        result.Distinct().Count().ShouldBe(count);
    }

    [Fact]
    public void SomeOf_ReturnsVariedResults()
    {
        // Arrange
        var choices = new[] { 1, 2, 3, 4, 5 };
        const int count = 3;
        const int attempts = 10;
        var uniqueResults = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.SomeOf(count, choices);
            Array.Sort(result);
            uniqueResults.Add(string.Join(",", result));
        }

        // Assert - Should produce some variety
        uniqueResults.Count.ShouldBeGreaterThan(1);
    }

    [Fact]
    public void SomeOf_CountOne_ReturnsSingleElement()
    {
        // Arrange
        var choices = new[] { 1, 2, 3, 4, 5 };
        const int count = 1;

        // Act
        var result = Populi.SomeOf(count, choices);

        // Assert
        result.Length.ShouldBe(1);
        choices.ShouldContain(result[0]);
    }

    [Fact]
    public void SomeOf_LargeArray_ReturnsCorrectSubset()
    {
        // Arrange
        var choices = Enumerable.Range(1, 20).ToArray();
        const int count = 10;

        // Act
        var result = Populi.SomeOf(count, choices);

        // Assert
        result.Length.ShouldBe(count);
        result.ShouldBeSubsetOf(choices);
        result.Distinct().Count().ShouldBe(count);
    }

    [Fact]
    public void SomeOf_WithStrings_ReturnsCorrectCount()
    {
        // Arrange
        var choices = new[] { "apple", "banana", "cherry", "date", "elderberry" };
        const int count = 3;

        // Act
        var result = Populi.SomeOf(count, choices);

        // Assert
        result.Length.ShouldBe(count);
        result.ShouldBeSubsetOf(choices);
    }

    [Fact]
    public void SomeOf_WithSingleChoice_ReturnsThatChoice()
    {
        // Arrange
        var choices = new[] { "only" };
        const int count = 1;

        // Act
        var result = Populi.SomeOf(count, choices);

        // Assert
        result.Length.ShouldBe(1);
        result[0].ShouldBe("only");
    }

    [Fact]
    public void OutOf_WithICollection_ReturnsElementFromCollection()
    {
        // Arrange
        ICollection<string> choices = new List<string> { "apple", "banana", "cherry" };

        // Act
        var result = Populi.OutOf(choices);

        // Assert
        choices.ShouldContain(result);
    }

    [Fact]
    public void OutOf_WithICollection_SingleElement_ReturnsThatElement()
    {
        // Arrange
        ICollection<int> choices = new List<int> { 42 };

        // Act
        var result = Populi.OutOf(choices);

        // Assert
        result.ShouldBe(42);
    }

    [Fact]
    public void OutOf_WithICollection_ReturnsVariedResults()
    {
        // Arrange
        ICollection<int> choices = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        const int attempts = 100;
        var results = new HashSet<int>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.OutOf(choices));
        }

        // Assert - Should produce some variety
        results.Count.ShouldBeGreaterThan(3);
    }

    [Fact]
    public void OutOf_WithIList_ReturnsElementFromList()
    {
        // Arrange
        IList<string> choices = new List<string> { "apple", "banana", "cherry" };

        // Act
        var result = Populi.OutOf(choices);

        // Assert
        choices.ShouldContain(result);
    }

    [Fact]
    public void OutOf_WithIList_SingleElement_ReturnsThatElement()
    {
        // Arrange
        IList<int> choices = new List<int> { 42 };

        // Act
        var result = Populi.OutOf(choices);

        // Assert
        result.ShouldBe(42);
    }

    [Fact]
    public void OutOf_WithIList_ReturnsVariedResults()
    {
        // Arrange
        IList<int> choices = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        const int attempts = 100;
        var results = new HashSet<int>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.OutOf(choices));
        }

        // Assert - Should produce some variety
        results.Count.ShouldBeGreaterThan(3);
    }

    [Fact]
    public void OutOf_WithIList_WorksWithArray()
    {
        // Arrange
        IList<string> choices = new[] { "apple", "banana", "cherry" };

        // Act
        var result = Populi.OutOf(choices);

        // Assert
        choices.ShouldContain(result);
    }

    [Fact]
    public void OneOf_WithParamsArray_WorksWithIntegers()
    {
        // Arrange
        var choices = new[] { 10, 20, 30, 40, 50 };

        // Act
        var result = Populi.OneOf(10, 20, 30, 40, 50);

        // Assert
        choices.ShouldContain(result);
    }

    [Fact]
    public void OutOf_WithICollection_WorksWithHashSet()
    {
        // Arrange
        ICollection<string> choices = new HashSet<string> { "apple", "banana", "cherry" };

        // Act
        var result = Populi.OutOf(choices);

        // Assert
        choices.ShouldContain(result);
    }

    [Fact]
    public void OneOf_WithWeightedChoices_EqualWeights_DistributesEvenly()
    {
        // Arrange
        var choice1 = ("a", 10);
        var choice2 = ("b", 10);
        var choice3 = ("c", 10);
        const int attempts = 1000;
        var counts = new Dictionary<string, int> { { "a", 0 }, { "b", 0 }, { "c", 0 } };

        // Act
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.OneOf(choice1, choice2, choice3);
            counts[result]++;
        }

        // Assert - Each should get roughly 1/3, allowing variance
        counts["a"].ShouldBeInRange(200, 500);
        counts["b"].ShouldBeInRange(200, 500);
        counts["c"].ShouldBeInRange(200, 500);
    }

    [Fact]
    public void SomeOf_WithStrings_ReturnsCorrectSubset()
    {
        // Arrange
        var choices = new[] { "apple", "banana", "cherry", "date", "elderberry" };
        const int count = 3;

        // Act
        var result = Populi.SomeOf(count, choices);

        // Assert
        result.Length.ShouldBe(count);
        result.ShouldBeSubsetOf(choices);
    }

    [Fact]
    public void OneOf_WithWeightedChoices_AllZeroWeights_ReturnsElement()
    {
        // Arrange
        var choice1 = ("a", 0);
        var choice2 = ("b", 0);
        var choice3 = ("c", 0);

        // Act
        var result = Populi.OneOf(choice1, choice2, choice3);

        // Assert
        new[] { "a", "b", "c" }.ShouldContain(result);
    }

    [Fact]
    public void OneOf_WithWeightedChoices_MixedPositiveAndNegativeWeights()
    {
        // Arrange
        var choice1 = ("negative", -10);
        var choice2 = ("zero", 0);
        var choice3 = ("positive", 10);
        const int attempts = 100;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.OneOf(choice1, choice2, choice3));
        }

        // Assert - Should contain at least "positive"
        results.ShouldContain("positive");
    }

    [Fact]
    public void OneOf_WithParamsArray_WithTwoChoices()
    {
        // Arrange
        var choice1 = "first";
        var choice2 = "second";

        // Act
        var result = Populi.OneOf(choice1, choice2);

        // Assert
        new[] { choice1, choice2 }.ShouldContain(result);
    }

    [Fact]
    public void OutOf_WithICollection_WorksWithSortedSet()
    {
        // Arrange
        ICollection<int> choices = new SortedSet<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = Populi.OutOf(choices);

        // Assert
        choices.ShouldContain(result);
    }

    [Fact]
    public void OutOf_WithArray_ReturnsElementFromArray()
    {
        // Arrange
        var choices = new[] { "apple", "banana", "cherry" };

        // Act
        var result = Populi.OutOf(choices);

        // Assert
        choices.ShouldContain(result);
    }

    [Fact]
    public void OutOf_WithArray_SingleElement_ReturnsThatElement()
    {
        // Arrange
        var choices = new[] { 42 };

        // Act
        var result = Populi.OutOf(choices);

        // Assert
        result.ShouldBe(42);
    }

    [Fact]
    public void OutOf_WithArray_ReturnsVariedResults()
    {
        // Arrange
        var choices = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        const int attempts = 100;
        var results = new HashSet<int>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.OutOf(choices));
        }

        // Assert - Should produce some variety
        results.Count.ShouldBeGreaterThan(3);
    }

    [Fact]
    public void OutOf_WithArray_WorksWithStrings()
    {
        // Arrange
        var choices = new[] { "red", "green", "blue" };

        // Act
        var result = Populi.OutOf(choices);

        // Assert
        choices.ShouldContain(result);
    }

    [Fact]
    public void NextName_ReturnsFullName()
    {
        // Act
        var result = Populi.NextName();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain(" ");
    }

    [Fact]
    public void NextName_ReturnsVariedResults()
    {
        // Arrange
        const int attempts = 50;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.NextName());
        }

        // Assert - Should produce some variety
        results.Count.ShouldBeGreaterThan(10);
    }

    [Fact]
    public void NextName_ContainsSpaceBetweenFirstAndLast()
    {
        // Arrange
        const int attempts = 10;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextName();
            var parts = result.Split(' ');
            parts.Length.ShouldBeGreaterThan(1);
        }
    }

    [Fact]
    public void NextFirstName_ReturnsNonEmptyString()
    {
        // Act
        var result = Populi.NextFirstName();

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void NextFirstName_ReturnsVariedResults()
    {
        // Arrange
        const int attempts = 50;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.NextFirstName());
        }

        // Assert - Should produce some variety
        results.Count.ShouldBeGreaterThan(10);
    }

    [Fact]
    public void NextFirstName_ConsistentlyReturnsValidNames()
    {
        // Arrange
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextFirstName();
            result.ShouldNotBeNullOrEmpty();
            result.Length.ShouldBeGreaterThan(0);
        }
    }

    [Fact]
    public void NextSurname_ReturnsNonEmptyString()
    {
        // Act
        var result = Populi.NextSurname();

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void NextSurname_ReturnsVariedResults()
    {
        // Arrange
        const int attempts = 50;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.NextSurname());
        }

        // Assert - Should produce some variety
        results.Count.ShouldBeGreaterThan(10);
    }

    [Fact]
    public void NextSurname_ConsistentlyReturnsValidSurnames()
    {
        // Arrange
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextSurname();
            result.ShouldNotBeNullOrEmpty();
            result.Length.ShouldBeGreaterThan(0);
        }
    }

    [Fact]
    public void NextPlace_ReturnsNonEmptyString()
    {
        // Act
        var result = Populi.NextPlace();

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void NextPlace_ReturnsVariedResults()
    {
        // Arrange
        const int attempts = 50;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.NextPlace());
        }

        // Assert - Should produce some variety
        results.Count.ShouldBeGreaterThan(10);
    }

    [Fact]
    public void NextPlace_ConsistentlyReturnsValidPlaces()
    {
        // Arrange
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextPlace();
            result.ShouldNotBeNullOrEmpty();
            result.Length.ShouldBeGreaterThan(0);
        }
    }

    [Fact]
    public void NextFullState_ReturnsNonEmptyString()
    {
        // Act
        var result = Populi.NextFullState();

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void NextFullState_ReturnsVariedResults()
    {
        // Arrange
        const int attempts = 50;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.NextFullState());
        }

        // Assert - Should produce some variety
        results.Count.ShouldBeGreaterThan(1);
    }

    [Fact]
    public void NextFullState_ConsistentlyReturnsValidStateNames()
    {
        // Arrange
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextFullState();
            result.ShouldNotBeNullOrEmpty();
            result.Length.ShouldBeGreaterThan(0);
        }
    }

    [Fact]
    public void NextShortState_ReturnsNonEmptyString()
    {
        // Act
        var result = Populi.NextShortState();

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void NextShortState_ReturnsVariedResults()
    {
        // Arrange
        const int attempts = 50;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.NextShortState());
        }

        // Assert - Should produce some variety
        results.Count.ShouldBeGreaterThan(1);
    }

    [Fact]
    public void NextShortState_ConsistentlyReturnsValidAbbreviations()
    {
        // Arrange
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextShortState();
            result.ShouldNotBeNullOrEmpty();
            result.Length.ShouldBeGreaterThan(0);
        }
    }

    [Fact]
    public void NextAddress_ReturnsNonEmptyString()
    {
        // Act
        var result = Populi.NextAddress();

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void NextAddress_ReturnsVariedResults()
    {
        // Arrange
        const int attempts = 50;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.NextAddress());
        }

        // Assert - Should produce some variety
        results.Count.ShouldBeGreaterThan(10);
    }

    [Fact]
    public void NextAddress_ContainsMultipleParts()
    {
        // Arrange
        const int attempts = 10;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextAddress();
            var parts = result.Split(' ');
            parts.Length.ShouldBeGreaterThan(1);
        }
    }

    [Fact]
    public void NextAddress_StartsWithNumber()
    {
        // Arrange
        const int attempts = 10;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextAddress();
            var firstPart = result.Split(' ')[0];
            char.IsDigit(firstPart[0]).ShouldBeTrue();
        }
    }

    [Fact]
    public void NextEmail_WithName_ReturnsValidEmailFormat()
    {
        // Arrange
        const string name = "John Doe";

        // Act
        var result = Populi.NextEmail(name);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("@");
        result.ShouldContain("John.Doe@");
    }

    [Fact]
    public void NextEmail_WithNullName_ReturnsValidEmailFormat()
    {
        // Act
        var result = Populi.NextEmail((string)null!);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("@");
    }

    [Fact]
    public void NextEmail_WithName_ReplacesSpacesWithDots()
    {
        // Arrange
        const string name = "Jane Smith Wilson";

        // Act
        var result = Populi.NextEmail(name);

        // Assert
        result.ShouldContain("Jane.Smith.Wilson@");
    }

    [Fact]
    public void NextEmail_WithName_ReturnsVariedDomains()
    {
        // Arrange
        const string name = "Test User";
        const int attempts = 20;
        var domains = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextEmail(name);
            var domain = result.Split('@')[1];
            domains.Add(domain);
        }

        // Assert - Should produce some variety in domains
        domains.Count.ShouldBeGreaterThan(1);
    }

    [Fact]
    public void NextEmail_WithFirstAndLastName_ReturnsValidEmailFormat()
    {
        // Arrange
        const string firstName = "John";
        const string surname = "Doe";

        // Act
        var result = Populi.NextEmail(firstName, surname);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("@");
        result.ShouldContain("John.Doe@");
    }

    [Fact]
    public void NextEmail_WithFirstAndLastName_ReturnsVariedDomains()
    {
        // Arrange
        const string firstName = "Jane";
        const string surname = "Smith";
        const int attempts = 20;
        var domains = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextEmail(firstName, surname);
            var domain = result.Split('@')[1];
            domains.Add(domain);
        }

        // Assert - Should produce some variety in domains
        domains.Count.ShouldBeGreaterThan(1);
    }

    [Fact]
    public void NextEmail_WithFirstAndLastName_UsesCorrectFormat()
    {
        // Arrange
        const string firstName = "Alice";
        const string surname = "Johnson";

        // Act
        var result = Populi.NextEmail(firstName, surname);

        // Assert
        result.ShouldStartWith("Alice.Johnson@");
    }

    [Fact]
    public void NextCompany_Default_ReturnsNonEmptyString()
    {
        // Act
        var result = Populi.NextCompany();

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void NextCompany_Default_ContainsCompanySuffix()
    {
        // Arrange
        var validSuffixes = new[] { "Co", "Company", "Corp", "Inc", "Incorporated", "Group Holdings", "LLC" };

        // Act
        var result = Populi.NextCompany();

        // Assert
        validSuffixes.ShouldContain(suffix => result.EndsWith(suffix));
    }

    [Fact]
    public void NextCompany_Default_ContainsSpaceBetweenNameAndSuffix()
    {
        // Act
        var result = Populi.NextCompany();

        // Assert
        result.ShouldContain(" ");
    }

    [Fact]
    public void NextCompany_MultipleCalls_ReturnsVariedResults()
    {
        // Arrange
        const int attempts = 50;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.NextCompany());
        }

        // Assert - Should produce some variety
        results.Count.ShouldBeGreaterThan(1);
    }

    [Fact]
    public void NextNumberNoLeadZero_WithSingleDigit_ReturnsNonZeroDigit()
    {
        // Act
        var result = Populi.NextNumberNoLeadZero(1);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.Length.ShouldBe(1);
        int.Parse(result).ShouldBeInRange(1, 9);
    }

    [Fact]
    public void NextNumberNoLeadZero_WithMultipleDigits_ReturnsCorrectLength()
    {
        // Arrange
        const int digits = 5;

        // Act
        var result = Populi.NextNumberNoLeadZero(digits);

        // Assert
        result.Length.ShouldBe(digits);
    }

    [Fact]
    public void NextNumberNoLeadZero_WithMultipleDigits_DoesNotStartWithZero()
    {
        // Arrange
        const int attempts = 100;

        // Act
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextNumberNoLeadZero(5);

            // Assert
            result[0].ShouldNotBe('0');
        }
    }

    [Fact]
    public void NextNumberNoLeadZero_WithMultipleDigits_AllCharactersAreDigits()
    {
        // Act
        var result = Populi.NextNumberNoLeadZero(10);

        // Assert
        result.ShouldAllBe(c => char.IsDigit(c));
    }

    [Fact]
    public void NextNumber_WithZeroDigits_ReturnsEmptyString()
    {
        // Act
        var result = Populi.NextNumber(0);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void NextNumber_WithSingleDigit_ReturnsSingleCharacter()
    {
        // Act
        var result = Populi.NextNumber(1);

        // Assert
        result.Length.ShouldBe(1);
        char.IsDigit(result[0]).ShouldBeTrue();
    }

    [Fact]
    public void NextNumber_WithMultipleDigits_ReturnsCorrectLength()
    {
        // Arrange
        const int digits = 10;

        // Act
        var result = Populi.NextNumber(digits);

        // Assert
        result.Length.ShouldBe(digits);
    }

    [Fact]
    public void NextNumber_WithMultipleDigits_AllCharactersAreDigits()
    {
        // Act
        var result = Populi.NextNumber(15);

        // Assert
        result.ShouldAllBe(c => char.IsDigit(c));
    }

    [Fact]
    public void NextNumber_WithMultipleDigits_CanStartWithZero()
    {
        // Arrange
        const int attempts = 1000;
        var foundLeadingZero = false;

        // Act
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextNumber(5);
            if (result[0] == '0')
            {
                foundLeadingZero = true;
                break;
            }
        }

        // Assert - After 1000 attempts, should have found at least one with leading zero
        foundLeadingZero.ShouldBeTrue();
    }

    [Fact]
    public void NextNumber_WithSeparator_EmptyGroups_ReturnsDefaultNineDigits()
    {
        // Act
        var result = Populi.NextNumber("-");

        // Assert
        result.Length.ShouldBe(9); // No separators, just 9 digits
        result.ShouldAllBe(c => char.IsDigit(c));
    }

    [Fact]
    public void NextNumber_WithSeparator_SingleGroup_ReturnsCorrectFormat()
    {
        // Arrange
        const string separator = "-";

        // Act
        var result = Populi.NextNumber(separator, 3);

        // Assert
        result.Length.ShouldBe(3);
        result.ShouldAllBe(c => char.IsDigit(c));
    }

    [Fact]
    public void NextNumber_WithSeparator_MultipleGroups_ReturnsCorrectFormat()
    {
        // Arrange
        const string separator = "-";

        // Act
        var result = Populi.NextNumber(separator, 3, 2, 4);

        // Assert
        var parts = result.Split('-');
        parts.Length.ShouldBe(3);
        parts[0].Length.ShouldBe(3);
        parts[1].Length.ShouldBe(2);
        parts[2].Length.ShouldBe(4);
    }

    [Fact]
    public void NextNumber_WithSeparator_MultipleGroups_AllPartsAreDigits()
    {
        // Act
        var result = Populi.NextNumber("-", 3, 3, 4);

        // Assert
        var parts = result.Split('-');
        foreach (var part in parts)
        {
            part.ShouldAllBe(c => char.IsDigit(c));
        }
    }

    [Fact]
    public void NextNumber_WithSeparator_DifferentSeparators_UsesCorrectSeparator()
    {
        // Act
        var result = Populi.NextNumber(".", 2, 2);

        // Assert
        result.ShouldContain(".");
        var parts = result.Split('.');
        parts.Length.ShouldBe(2);
    }

    [Fact]
    public void NextNumber_WithSeparator_EmptySeparator_ConcatenatesGroups()
    {
        // Act
        var result = Populi.NextNumber("", 3, 3, 3);

        // Assert
        result.Length.ShouldBe(9);
        result.ShouldAllBe(c => char.IsDigit(c));
    }

    [Fact]
    public void AsWebsite_WithSimpleName_ReturnsValidFormat()
    {
        // Arrange
        const string name = "Acme";

        // Act
        var result = Populi.AsWebsite(name);

        // Assert
        result.ShouldStartWith("http://www.");
        result.ShouldContain("Acme");
    }

    [Fact]
    public void AsWebsite_WithSpaces_RemovesSpaces()
    {
        // Arrange
        const string name = "Acme Corp";

        // Act
        var result = Populi.AsWebsite(name);

        // Assert
        result.ShouldContain("AcmeCorp");
        result.ShouldNotContain(" ");
    }

    [Fact]
    public void AsWebsite_WithMultipleSpaces_RemovesAllSpaces()
    {
        // Arrange
        const string name = "Big Tech Company Holdings";

        // Act
        var result = Populi.AsWebsite(name);

        // Assert
        result.ShouldContain("BigTechCompanyHoldings");
        result.ShouldNotContain(" ");
    }

    [Fact]
    public void AsWebsite_Default_ContainsValidDomain()
    {
        // Arrange
        const string name = "TestCompany";
        var validDomains = new[] { ".com", ".net", ".us", ".biz", ".org" };

        // Act
        var result = Populi.AsWebsite(name);

        // Assert
        validDomains.ShouldContain(domain => result.EndsWith(domain));
    }

    [Fact]
    public void AsWebsite_MultipleCalls_ReturnsVariedDomains()
    {
        // Arrange
        const string name = "TestCompany";
        const int attempts = 50;
        var domains = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.AsWebsite(name);
            var domain = result.Substring(result.LastIndexOf('.'));
            domains.Add(domain);
        }

        // Assert - Should produce some variety in domains
        domains.Count.ShouldBeGreaterThan(1);
    }

    [Fact]
    public void AsWebsite_WithLeadingAndTrailingSpaces_RemovesSpaces()
    {
        // Arrange
        const string name = "  Acme Corp  ";

        // Act
        var result = Populi.AsWebsite(name);

        // Assert
        result.ShouldContain("AcmeCorp");
        result.ShouldNotContain(" ");
    }

    [Fact]
    public void NextUsLat_Default_ReturnsValueInUsLatitudeRange()
    {
        // Arrange
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextUsLat();
            result.ShouldBeInRange(24.7433195, 49.3457868);
        }
    }

    [Fact]
    public void NextUsLong_Default_ReturnsValueInUsLongitudeRange()
    {
        // Arrange
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextUsLong();
            result.ShouldBeInRange(-124.7844079, -66.9513812);
        }
    }

    [Fact]
    public void NextFutureDate_Default_ReturnsDateInDefaultRange()
    {
        // Arrange
        var today = DateTime.Today;
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextFutureDate();
            result.ShouldBeGreaterThanOrEqualTo(today);
            result.ShouldBeLessThan(today.AddDays(1826));
        }
    }

    [Fact]
    public void NextFutureDate_WithMaxDays_ReturnsDateInSpecifiedRange()
    {
        // Arrange
        var today = DateTime.Today;
        const int maxDays = 30;
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextFutureDate(maxDays);
            result.ShouldBeGreaterThanOrEqualTo(today);
            result.ShouldBeLessThan(today.AddDays(maxDays));
        }
    }

    [Fact]
    public void NextPastDate_WithMaxDays_ReturnsDateInSpecifiedRange()
    {
        // Arrange
        var today = DateTime.Today;
        const int maxDays = 100;
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextPastDate(maxDays);
            result.ShouldBeGreaterThan(today.AddDays(-maxDays));
            result.ShouldBeLessThanOrEqualTo(today);
        }
    }

    [Fact]
    public void NextPastDate_WithMinAndMaxDays_ReturnsDateInSpecifiedRange()
    {
        // Arrange
        var today = DateTime.Today;
        const int minDays = 10;
        const int maxDays = 100;
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextPastDate(minDays, maxDays);
            result.ShouldBeGreaterThan(today.AddDays(-maxDays));
            result.ShouldBeLessThanOrEqualTo(today.AddDays(-minDays));
        }
    }

    [Fact]
    public void NextPastDate_WithDefaultParameters_ReturnsDateInDefaultRange()
    {
        // Arrange
        var today = DateTime.Today;
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextPastDate();
            result.ShouldBeGreaterThan(today.AddDays(-1826));
            result.ShouldBeLessThanOrEqualTo(today.AddDays(-1));
        }
    }

    [Fact]
    public void NextPastDateAfter_WithAfterDate_ReturnsPastDateAfterSpecifiedDate()
    {
        // Arrange
        var after = DateTime.Today.AddDays(-100);
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextPastDateAfter(after);
            result.ShouldBeGreaterThanOrEqualTo(after);
            result.ShouldBeLessThanOrEqualTo(DateTime.Today);
        }
    }

    [Fact]
    public void NextPastDateAfter_WithRecentDate_ReturnsPastDateAfterSpecifiedDate()
    {
        // Arrange
        var after = DateTime.Today.AddDays(-10);
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextPastDateAfter(after);
            result.ShouldBeGreaterThanOrEqualTo(after);
            result.ShouldBeLessThanOrEqualTo(DateTime.Today);
        }
    }

    [Fact]
    public void NextBirthdate_Default_ReturnsDateForAge22()
    {
        // Arrange
        var today = DateTime.Today;
        var earliestDate = today.AddYears(-23);
        var latestDate = today.AddYears(-22);
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextBirthdate();
            result.ShouldBeGreaterThanOrEqualTo(earliestDate);
            result.ShouldBeLessThan(latestDate);
        }
    }

    [Fact]
    public void NextBirthdate_WithSpecificAge_ReturnsDateForSpecifiedAge()
    {
        // Arrange
        var today = DateTime.Today;
        const int age = 30;
        var earliestDate = today.AddYears(-age - 1);
        var latestDate = today.AddYears(-age);
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextBirthdate(age);
            result.ShouldBeGreaterThanOrEqualTo(earliestDate);
            result.ShouldBeLessThan(latestDate);
        }
    }

    [Fact]
    public void NextBirthdate_WithZeroAge_ReturnsDateForZeroAge()
    {
        // Arrange
        var today = DateTime.Today;
        var earliestDate = today.AddYears(-1);
        var latestDate = today;
        const int attempts = 100;

        // Act & Assert
        for (int i = 0; i < attempts; i++)
        {
            var result = Populi.NextBirthdate(0);
            result.ShouldBeGreaterThanOrEqualTo(earliestDate);
            result.ShouldBeLessThan(latestDate);
        }
    }

    [Fact]
    public void LoremIpsum_Default_Returns69Words()
    {
        // Act
        var result = Populi.LoremIpsum();

        // Assert
        var words = result.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        words.Length.ShouldBe(69);
    }

    [Fact]
    public void LoremIpsum_WithSpecificWordCount_ReturnsCorrectNumberOfWords()
    {
        // Arrange
        const int wordCount = 20;

        // Act
        var result = Populi.LoremIpsum(wordCount);

        // Assert
        var words = result.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        words.Length.ShouldBe(wordCount);
    }

    [Fact]
    public void LoremIpsum_WithZeroWords_ReturnsEmptyString()
    {
        // Act
        var result = Populi.LoremIpsum(0);

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void LoremIpsum_WithOneWord_ReturnsSingleWord()
    {
        // Act
        var result = Populi.LoremIpsum(1);

        // Assert
        var words = result.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        words.Length.ShouldBe(1);
        words[0].ShouldBe("Lorem");
    }

    [Fact]
    public void LoremIpsum_WithWordCountExceedingArray_CyclesThrough()
    {
        // Arrange
        const int wordCount = 150;

        // Act
        var result = Populi.LoremIpsum(wordCount);

        // Assert
        var words = result.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        words.Length.ShouldBe(wordCount);
        words[0].ShouldBe("Lorem");
        words[1].ShouldBe("ipsum");
    }

    [Fact]
    public void NextWord_Default_ReturnsNonEmptyString()
    {
        // Act
        var result = Populi.NextWord();

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void NextWord_MultipleCalls_ReturnsWords()
    {
        // Arrange
        const int attempts = 100;
        var results = new HashSet<string>();

        // Act
        for (int i = 0; i < attempts; i++)
        {
            results.Add(Populi.NextWord());
        }

        // Assert
        results.Count.ShouldBeGreaterThan(1);
    }

    [Fact]
    public void NextWords_WithCount_ReturnsCorrectNumberOfWords()
    {
        // Arrange
        const int count = 5;

        // Act
        var result = Populi.NextWords(count);

        // Assert
        var words = result.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        words.Length.ShouldBe(count);
    }

    [Fact]
    public void NextWords_WithZeroCount_ReturnsEmptyString()
    {
        // Act
        var result = Populi.NextWords(0);

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void NextWords_WithOneCount_ReturnsSingleWord()
    {
        // Act
        var result = Populi.NextWords(1);

        // Assert
        var words = result.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        words.Length.ShouldBe(1);
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void NextWords_WithMultipleCount_ReturnsSpaceSeparatedWords()
    {
        // Arrange
        const int count = 10;

        // Act
        var result = Populi.NextWords(count);

        // Assert
        result.ShouldContain(" ");
        var words = result.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        words.Length.ShouldBe(count);
    }
}
