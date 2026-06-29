using RossWright.MetalNexus.Schema.PathStrategies;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests;

public class TrimFixedPreamblePathStrategyTests
{
    [Fact]
    public void Constructor_WithSimplePreamble_StoresPreambleSegments()
    {
        // Arrange & Act
        var strategy = new TrimFixedPreamblePathStrategy("MyCorp.MyApp");

        // Assert
        strategy.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithEmptyString_CreatesStrategy()
    {
        // Arrange & Act
        var strategy = new TrimFixedPreamblePathStrategy("");

        // Assert
        strategy.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithSingleSegment_CreatesStrategy()
    {
        // Arrange & Act
        var strategy = new TrimFixedPreamblePathStrategy("MyApp");

        // Assert
        strategy.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithMultipleSegments_CreatesStrategy()
    {
        // Arrange & Act
        var strategy = new TrimFixedPreamblePathStrategy("MyCorp.MyApp.Endpoints.Users");

        // Assert
        strategy.ShouldNotBeNull();
    }

    [Fact]
    public void Trim_WithMatchingPreamble_ReturnsRemainingPath()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("RossWright.MetalNexus.Abstractions.UnitTests.TrimFixedPreamblePathStrategyTests.TestTypes.MyCorp.MyApp");
        var type = typeof(TestTypes.MyCorp.MyApp.Endpoints.GetUserRequest);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("Endpoints");
    }

    [Fact]
    public void Trim_WithPartialMatchingPreamble_ReturnsPathAfterMatch()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("RossWright.MetalNexus.Abstractions.UnitTests.TrimFixedPreamblePathStrategyTests.TestTypes.MyCorp.MyApp.Endpoints");
        var type = typeof(TestTypes.MyCorp.MyApp.Endpoints.Users.GetUserRequest);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("Users");
    }

    [Fact]
    public void Trim_WithNoMatchingPreamble_ReturnsFullNamespace()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("Other.Company");
        var type = typeof(TestTypes.MyCorp.MyApp.Endpoints.GetUserRequest);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("RossWright/MetalNexus/Abstractions/UnitTests/TrimFixedPreamblePathStrategyTests/TestTypes/MyCorp/MyApp/Endpoints");
    }

    [Fact]
    public void Trim_WithExactMatchingPreamble_ReturnsNull()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("RossWright.MetalNexus.Abstractions.UnitTests.TrimFixedPreamblePathStrategyTests.TestTypes.MyCorp.MyApp.Endpoints");
        var type = typeof(TestTypes.MyCorp.MyApp.Endpoints.GetUserRequest);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Trim_WithNestedType_HandlesNestedTypeCorrectly()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("RossWright.MetalNexus.Abstractions.UnitTests.TrimFixedPreamblePathStrategyTests.TestTypes.MyCorp");
        var type = typeof(TestTypes.MyCorp.OuterClass.InnerClass);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("OuterClass");
    }

    [Fact]
    public void Trim_WithTypeInTestNamespace_ReturnsNamespacePath()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("Other.Namespace");

        // Act
        var result = strategy.Trim(typeof(NoNamespaceClass));

        // Assert
        result.ShouldBe("RossWright/MetalNexus/Abstractions/UnitTests");
    }

    [Fact]
    public void Trim_WithEmptyPreamble_ReturnsFullNamespace()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("");
        var type = typeof(TestTypes.MyCorp.MyApp.Endpoints.GetUserRequest);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("RossWright/MetalNexus/Abstractions/UnitTests/TrimFixedPreamblePathStrategyTests/TestTypes/MyCorp/MyApp/Endpoints");
    }

    [Fact]
    public void Trim_WithPreambleLongerThanNamespace_ReturnsNull()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("RossWright.MetalNexus.Abstractions.UnitTests.TrimFixedPreamblePathStrategyTests.TestTypes.MyCorp.MyApp.Endpoints.Users.SubUsers");
        var type = typeof(TestTypes.MyCorp.MyApp.Endpoints.GetUserRequest);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Trim_WithPartialSegmentMatch_DoesNotTrimPartialMatch()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("RossWright.MetalNexus.Abstractions.UnitTests.TrimFixedPreamblePathStrategyTests.TestTypes.MyCorp.MyAppOther");
        var type = typeof(TestTypes.MyCorp.MyApp.Endpoints.GetUserRequest);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("MyApp/Endpoints");
    }

    [Fact]
    public void Trim_WithSingleSegmentPreambleMatching_ReturnsRemainingPath()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("RossWright.MetalNexus.Abstractions.UnitTests.TrimFixedPreamblePathStrategyTests.TestTypes");
        var type = typeof(TestTypes.MyCorp.MyApp.Endpoints.GetUserRequest);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("MyCorp/MyApp/Endpoints");
    }

    [Fact]
    public void Trim_WithDeeplyNestedType_HandlesMultiplePlusSignsCorrectly()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("RossWright.MetalNexus.Abstractions.UnitTests.TrimFixedPreamblePathStrategyTests.TestTypes");
        var type = typeof(TestTypes.OuterClass.MiddleClass.InnerClass);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("OuterClass/MiddleClass");
    }

    [Fact]
    public void Trim_WithNonGenericSystemType_TrimsMatchingPreamble()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("System");
        var type = typeof(System.DateTime);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Trim_WithCaseSensitivePreamble_RequiresExactCase()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("rosswright.metalnexus.abstractions.unittests");
        var type = typeof(TestTypes.MyCorp.MyApp.Endpoints.GetUserRequest);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("RossWright/MetalNexus/Abstractions/UnitTests/TrimFixedPreamblePathStrategyTests/TestTypes/MyCorp/MyApp/Endpoints");
    }

    [Fact]
    public void Trim_WithGenericType_HandlesBracketsInFullName()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("System.Collections");
        var type = typeof(System.Collections.Generic.List<int>);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldStartWith("Generic/List");
    }

    [Fact]
    public void Trim_WithFirstSegmentMismatch_ReturnsFullNamespace()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("Other");
        var type = typeof(TestTypes.MyCorp.MyApp.Endpoints.GetUserRequest);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("RossWright/MetalNexus/Abstractions/UnitTests/TrimFixedPreamblePathStrategyTests/TestTypes/MyCorp/MyApp/Endpoints");
    }

    [Fact]
    public void Trim_WithDottedPreambleContainingMultipleSegments_SplitsPreambleCorrectly()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("RossWright.MetalNexus");
        var type = typeof(TestTypes.MyCorp.MyApp.Endpoints.GetUserRequest);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("Abstractions/UnitTests/TrimFixedPreamblePathStrategyTests/TestTypes/MyCorp/MyApp/Endpoints");
    }

    [Fact]
    public void Trim_WithSingleDotInPreamble_HandlesSingleSegment()
    {
        // Arrange
        var strategy = new TrimFixedPreamblePathStrategy("RossWright");
        var type = typeof(TestTypes.MyCorp.MyApp.Endpoints.GetUserRequest);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("MetalNexus/Abstractions/UnitTests/TrimFixedPreamblePathStrategyTests/TestTypes/MyCorp/MyApp/Endpoints");
    }

    [Fact]
    public void Constructor_WithTrailingDot_CreatesStrategyWithEmptyLastSegment()
    {
        // Arrange & Act
        var strategy = new TrimFixedPreamblePathStrategy("MyCorp.MyApp.");

        // Assert
        strategy.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithLeadingDot_CreatesStrategyWithEmptyFirstSegment()
    {
        // Arrange & Act
        var strategy = new TrimFixedPreamblePathStrategy(".MyCorp.MyApp");

        // Assert
        strategy.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithMultipleDots_CreatesStrategyWithEmptySegments()
    {
        // Arrange & Act
        var strategy = new TrimFixedPreamblePathStrategy("MyCorp..MyApp");

        // Assert
        strategy.ShouldNotBeNull();
    }

    // Helper types for testing
    private class TestTypes
    {
        public class MyCorp
        {
            public class MyApp
            {
                public class Endpoints
                {
                    public class GetUserRequest { }
                    public class Users
                    {
                        public class GetUserRequest { }
                    }
                }
            }

            public class OuterClass
            {
                public class InnerClass { }
            }
        }

        public class OuterClass
        {
            public class MiddleClass
            {
                public class InnerClass { }
            }
        }
    }
}

// Type with no namespace for testing
public class NoNamespaceClass { }
