namespace RossWright.MetalCore.Tests;

public class CloneAsExtensionsTests
{
    // ── Clone<T> tests ──────────────────────────────────────────────────────────
    
    [Fact]
    public void Clone_BasicObject_CreatesShallowCopy()
    {
        var source = new TestClass { Value = 42, Name = "Test" };
        var result = source.Clone();
        
        result.ShouldNotBeSameAs(source);
        result.Value.ShouldBe(42);
        result.Name.ShouldBe("Test");
    }

    [Fact]
    public void Clone_WithInit_InvokesCallbackAfterCopying()
    {
        var source = new TestClass { Value = 42 };
        TestClass? callbackParameter = null;
        var result = source.Clone(clone =>
        {
            callbackParameter = clone;
            clone.Value.ShouldBe(42);
            clone.Value = 99;
        });
        
        result.ShouldNotBeSameAs(source);
        result.Value.ShouldBe(99);
        callbackParameter.ShouldBeSameAs(result);
    }

    [Fact]
    public void Clone_WithNullInit_ClonesWithoutCallback()
    {
        var source = new TestClass { Value = 42 };
        var result = source.Clone(init: null);
        
        result.ShouldNotBeSameAs(source);
        result.Value.ShouldBe(42);
    }

    // ── CloneAs<T>(IEnumerable<object>) tests ──────────────────────────────────

    [Fact]
    public void CloneAs_EmptyCollection_ReturnsEmptyArray()
    {
        var source = Array.Empty<object>();
        var result = source.CloneAs<TestClass>();
        
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void CloneAs_SingleItem_ClonesItem()
    {
        var source = new object[] { new TestClass { Value = 42, Name = "Test" } };
        var result = source.CloneAs<TestClass>();
        
        result.Length.ShouldBe(1);
        result[0].Value.ShouldBe(42);
        result[0].Name.ShouldBe("Test");
    }

    [Fact]
    public void CloneAs_MultipleItems_ClonesAllItems()
    {
        var source = new object[]
        {
            new TestClass { Value = 1, Name = "First" },
            new TestClass { Value = 2, Name = "Second" },
            new TestClass { Value = 3, Name = "Third" }
        };
        var result = source.CloneAs<TestClass>();
        
        result.Length.ShouldBe(3);
        result[0].Value.ShouldBe(1);
        result[0].Name.ShouldBe("First");
        result[1].Value.ShouldBe(2);
        result[1].Name.ShouldBe("Second");
        result[2].Value.ShouldBe(3);
        result[2].Name.ShouldBe("Third");
    }

    [Fact]
    public void CloneAs_DifferentSourceType_MapsMatchingProperties()
    {
        var source = new object[]
        {
            new SourceClass { Value = 10, Extra = "ignored" }
        };
        var result = source.CloneAs<TestClass>();
        
        result.Length.ShouldBe(1);
        result[0].Value.ShouldBe(10);
        result[0].Name.ShouldBeNull();
    }

    // ── CloneAs<T>(IEnumerable<object>, Action<T>) tests ──────────────────────

    [Fact]
    public void CloneAs_WithInitAction_InvokesCallbackForEachItem()
    {
        var source = new object[]
        {
            new TestClass { Value = 1 },
            new TestClass { Value = 2 }
        };
        var callbackCount = 0;
        var result = source.CloneAs<TestClass>(clone =>
        {
            callbackCount++;
            clone.Name = $"Modified{clone.Value}";
        });
        
        result.Length.ShouldBe(2);
        callbackCount.ShouldBe(2);
        result[0].Value.ShouldBe(1);
        result[0].Name.ShouldBe("Modified1");
        result[1].Value.ShouldBe(2);
        result[1].Name.ShouldBe("Modified2");
    }

    [Fact]
    public void CloneAs_WithInitAction_EmptyCollection_ReturnsEmptyArray()
    {
        var source = Array.Empty<object>();
        var callbackCount = 0;
        var result = source.CloneAs<TestClass>(clone => callbackCount++);
        
        result.ShouldBeEmpty();
        callbackCount.ShouldBe(0);
    }

    [Fact]
    public void CloneAs_WithInitAction_SingleItem_InvokesCallback()
    {
        var source = new object[] { new TestClass { Value = 42 } };
        TestClass? callbackParameter = null;
        var result = source.CloneAs<TestClass>(clone =>
        {
            callbackParameter = clone;
            clone.Name = "Modified";
        });
        
        result.Length.ShouldBe(1);
        result[0].Name.ShouldBe("Modified");
        callbackParameter.ShouldBeSameAs(result[0]);
    }

    // ── CloneAs<T>(IEnumerable<object>, Action<object, T>) tests ──────────────

    [Fact]
    public void CloneAs_WithSourceAndCloneAction_PassesBothToCallback()
    {
        var source = new object[]
        {
            new TestClass { Value = 1, Name = "First" },
            new TestClass { Value = 2, Name = "Second" }
        };
        var sourcesReceived = new List<object>();
        var clonesReceived = new List<TestClass>();
        
        var result = source.CloneAs<TestClass>((src, clone) =>
        {
            sourcesReceived.Add(src);
            clonesReceived.Add(clone);
            clone.Name = $"{((TestClass)src).Name}-Modified";
        });
        
        result.Length.ShouldBe(2);
        sourcesReceived.Count.ShouldBe(2);
        clonesReceived.Count.ShouldBe(2);
        sourcesReceived[0].ShouldBeSameAs(source.ElementAt(0));
        sourcesReceived[1].ShouldBeSameAs(source.ElementAt(1));
        clonesReceived[0].ShouldBeSameAs(result[0]);
        clonesReceived[1].ShouldBeSameAs(result[1]);
        result[0].Name.ShouldBe("First-Modified");
        result[1].Name.ShouldBe("Second-Modified");
    }

    [Fact]
    public void CloneAs_WithSourceAndCloneAction_EmptyCollection_ReturnsEmptyArray()
    {
        var source = Array.Empty<object>();
        var callbackCount = 0;
        var result = source.CloneAs<TestClass>((src, clone) => callbackCount++);
        
        result.ShouldBeEmpty();
        callbackCount.ShouldBe(0);
    }

    [Fact]
    public void CloneAs_WithSourceAndCloneAction_SingleItem_InvokesCallback()
    {
        var sourceItem = new TestClass { Value = 42, Name = "Original" };
        var source = new object[] { sourceItem };
        object? receivedSource = null;
        TestClass? receivedClone = null;
        
        var result = source.CloneAs<TestClass>((src, clone) =>
        {
            receivedSource = src;
            receivedClone = clone;
            clone.Name = ((TestClass)src).Name + "-Copy";
        });
        
        result.Length.ShouldBe(1);
        receivedSource.ShouldBeSameAs(sourceItem);
        receivedClone.ShouldBeSameAs(result[0]);
        result[0].Name.ShouldBe("Original-Copy");
    }

    // ── CloneAs<DBO, DTO>(IEnumerable<DBO>, Action<DBO, DTO>?) tests ─────────

    [Fact]
    public void CloneAs_StronglyTyped_WithoutInit_ClonesAllItems()
    {
        var source = new List<SourceClass>
        {
            new() { Value = 1, Extra = "A" },
            new() { Value = 2, Extra = "B" }
        };
        var result = source.CloneAs<SourceClass, TestClass>();
        
        result.Length.ShouldBe(2);
        result[0].Value.ShouldBe(1);
        result[1].Value.ShouldBe(2);
    }

    [Fact]
    public void CloneAs_StronglyTyped_WithInit_InvokesCallbackForEachItem()
    {
        var source = new List<SourceClass>
        {
            new() { Value = 1, Extra = "A" },
            new() { Value = 2, Extra = "B" }
        };
        var callbackCount = 0;
        var result = source.CloneAs<SourceClass, TestClass>((src, dto) =>
        {
            callbackCount++;
            dto.Name = src.Extra + dto.Value;
        });
        
        result.Length.ShouldBe(2);
        callbackCount.ShouldBe(2);
        result[0].Name.ShouldBe("A1");
        result[1].Name.ShouldBe("B2");
    }

    [Fact]
    public void CloneAs_StronglyTyped_WithNullInit_ClonesWithoutCallback()
    {
        var source = new List<SourceClass>
        {
            new() { Value = 42, Extra = "Test" }
        };
        var result = source.CloneAs<SourceClass, TestClass>(init: null);
        
        result.Length.ShouldBe(1);
        result[0].Value.ShouldBe(42);
        result[0].Name.ShouldBeNull();
    }

    [Fact]
    public void CloneAs_StronglyTyped_EmptyCollection_ReturnsEmptyArray()
    {
        var source = new List<SourceClass>();
        var result = source.CloneAs<SourceClass, TestClass>();
        
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void CloneAs_StronglyTyped_SingleItem_ClonesCorrectly()
    {
        var source = new List<SourceClass>
        {
            new() { Value = 99, Extra = "Single" }
        };
        var result = source.CloneAs<SourceClass, TestClass>((src, dto) =>
        {
            dto.Name = src.Extra;
        });
        
        result.Length.ShouldBe(1);
        result[0].Value.ShouldBe(99);
        result[0].Name.ShouldBe("Single");
    }

    [Fact]
    public void CloneAs_StronglyTyped_WithInit_SourceAndCloneBothAccessible()
    {
        var source = new List<SourceClass>
        {
            new() { Value = 10, Extra = "Source1" },
            new() { Value = 20, Extra = "Source2" }
        };
        var sourcesReceived = new List<SourceClass>();
        var clonesReceived = new List<TestClass>();
        
        var result = source.CloneAs<SourceClass, TestClass>((src, clone) =>
        {
            sourcesReceived.Add(src);
            clonesReceived.Add(clone);
        });
        
        sourcesReceived.Count.ShouldBe(2);
        clonesReceived.Count.ShouldBe(2);
        sourcesReceived[0].ShouldBeSameAs(source[0]);
        sourcesReceived[1].ShouldBeSameAs(source[1]);
        clonesReceived[0].ShouldBeSameAs(result[0]);
        clonesReceived[1].ShouldBeSameAs(result[1]);
    }

    // ── CloneAs<T>(object, Action<T>?) tests ───────────────────────────────────

    [Fact]
    public void CloneAs_SingleObject_CreatesNewInstance()
    {
        var source = new TestClass { Value = 42, Name = "Test" };
        var result = source.CloneAs<TestClass>();
        
        result.ShouldNotBeSameAs(source);
        result.Value.ShouldBe(42);
        result.Name.ShouldBe("Test");
    }

    [Fact]
    public void CloneAs_SingleObject_WithInit_InvokesCallback()
    {
        var source = new TestClass { Value = 42, Name = "Original" };
        TestClass? callbackParameter = null;
        var result = source.CloneAs<TestClass>(clone =>
        {
            callbackParameter = clone;
            clone.Value.ShouldBe(42);
            clone.Name = "Modified";
        });
        
        result.ShouldNotBeSameAs(source);
        result.Value.ShouldBe(42);
        result.Name.ShouldBe("Modified");
        callbackParameter.ShouldBeSameAs(result);
    }

    [Fact]
    public void CloneAs_SingleObject_WithNullInit_ClonesWithoutCallback()
    {
        var source = new TestClass { Value = 99, Name = "Test" };
        var result = source.CloneAs<TestClass>(init: null);
        
        result.ShouldNotBeSameAs(source);
        result.Value.ShouldBe(99);
        result.Name.ShouldBe("Test");
    }

    [Fact]
    public void CloneAs_SingleObject_DifferentTypes_MapsMatchingProperties()
    {
        var source = new SourceClass { Value = 123, Extra = "ignored" };
        var result = source.CloneAs<TestClass>();
        
        result.Value.ShouldBe(123);
        result.Name.ShouldBeNull();
    }

    [Fact]
    public void CloneAs_SingleObject_DifferentTypes_WithInit_InvokesCallback()
    {
        var source = new SourceClass { Value = 50, Extra = "test" };
        var result = source.CloneAs<TestClass>(clone =>
        {
            clone.Name = "InitSet";
        });
        
        result.Value.ShouldBe(50);
        result.Name.ShouldBe("InitSet");
    }

    // ── HasChangedFrom<T> tests ─────────────────────────────────────────────────

    [Fact]
    public void HasChangedFrom_BothNull_ReturnsFalse()
    {
        TestClass? obj1 = null;
        TestClass? obj2 = null;
        
        var result = obj1.HasChangedFrom(obj2);
        
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasChangedFrom_FirstNullSecondNotNull_ReturnsTrue()
    {
        TestClass? obj1 = null;
        var obj2 = new TestClass { Value = 42 };
        
        var result = obj1.HasChangedFrom(obj2);
        
        result.ShouldBeTrue();
    }

    [Fact]
    public void HasChangedFrom_FirstNotNullSecondNull_ReturnsTrue()
    {
        var obj1 = new TestClass { Value = 42 };
        TestClass? obj2 = null;
        
        var result = obj1.HasChangedFrom(obj2);
        
        result.ShouldBeTrue();
    }

    [Fact]
    public void HasChangedFrom_IdenticalValues_ReturnsFalse()
    {
        var obj1 = new TestClass { Value = 42, Name = "Test" };
        var obj2 = new TestClass { Value = 42, Name = "Test" };
        
        var result = obj1.HasChangedFrom(obj2);
        
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasChangedFrom_DifferentValues_ReturnsTrue()
    {
        var obj1 = new TestClass { Value = 42, Name = "Test" };
        var obj2 = new TestClass { Value = 99, Name = "Test" };
        
        var result = obj1.HasChangedFrom(obj2);
        
        result.ShouldBeTrue();
    }

    [Fact]
    public void HasChangedFrom_DifferentStringValues_ReturnsTrue()
    {
        var obj1 = new TestClass { Value = 42, Name = "Changed" };
        var obj2 = new TestClass { Value = 42, Name = "Original" };
        
        var result = obj1.HasChangedFrom(obj2);
        
        result.ShouldBeTrue();
    }

    [Fact]
    public void HasChangedFrom_OnePropertyNullOtherNot_ReturnsTrue()
    {
        var obj1 = new TestClass { Value = 42, Name = "Test" };
        var obj2 = new TestClass { Value = 42, Name = null };
        
        var result = obj1.HasChangedFrom(obj2);
        
        result.ShouldBeTrue();
    }

    [Fact]
    public void HasChangedFrom_BothPropertiesNull_ReturnsFalse()
    {
        var obj1 = new TestClass { Value = 42, Name = null };
        var obj2 = new TestClass { Value = 42, Name = null };
        
        var result = obj1.HasChangedFrom(obj2);
        
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasChangedFrom_SameInstance_ReturnsFalse()
    {
        var obj = new TestClass { Value = 42, Name = "Test" };
        
        var result = obj.HasChangedFrom(obj);
        
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasChangedFrom_ClonedAndUnmodified_ReturnsFalse()
    {
        var original = new TestClass { Value = 42, Name = "Test" };
        var clone = original.Clone();
        
        var result = clone.HasChangedFrom(original);
        
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasChangedFrom_ClonedAndModified_ReturnsTrue()
    {
        var original = new TestClass { Value = 42, Name = "Test" };
        var clone = original.Clone();
        clone.Value = 99;
        
        var result = clone.HasChangedFrom(original);
        
        result.ShouldBeTrue();
    }

    // ── Test helper classes ────────────────────────────────────────────────────

    class TestClass
    {
        public int Value { get; set; }
        public string? Name { get; set; }
    }

    class SourceClass
    {
        public int Value { get; set; }
        public string Extra { get; set; } = string.Empty;
    }
}
