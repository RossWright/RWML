namespace RossWright.MetalCore.Tests.CloneAsExtension;

public class ThenCloneAsTests
{
    // -------------------------------------------------------------------------
    // T1 — ThenCloneAs<DBO, DTO>(Task<DBO?>) single-object overload
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ThenCloneAs_SingleObject_HappyPath()
    {
        var source = new BasicTypeTwoProp { Value = 42, OtherValue = 7 };
        var result = await Task.FromResult<BasicTypeTwoProp?>(source)
            .ThenCloneAs<BasicTypeTwoProp, BasicTypeOneProp>();

        result.ShouldNotBeNull();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public async Task ThenCloneAs_SingleObject_WithInit_HappyPath()
    {
        var source = new BasicTypeTwoProp { Value = 10, OtherValue = 20 };
        BasicTypeTwoProp? capturedSrc = null;
        BasicTypeOneProp? capturedClone = null;

        var result = await Task.FromResult<BasicTypeTwoProp?>(source)
            .ThenCloneAs<BasicTypeTwoProp, BasicTypeOneProp>((src, clone) =>
            {
                capturedSrc = src;
                capturedClone = clone;
            });

        result.ShouldNotBeNull();
        result.Value.ShouldBe(10);
        capturedSrc.ShouldBeSameAs(source);
        capturedClone.ShouldBeSameAs(result);
    }

    [Fact]
    public async Task ThenCloneAs_SingleObject_NullSourceTaskResult_ReturnsDefault()
    {
        var result = await Task.FromResult<BasicTypeTwoProp?>(null)
            .ThenCloneAs<BasicTypeTwoProp, BasicTypeOneProp>();

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ThenCloneAs_SingleObject_FaultedTask_Propagates()
    {
        static async Task<BasicTypeTwoProp?> FaultedTask()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }

        await Should.ThrowAsync<InvalidOperationException>(() =>
            FaultedTask().ThenCloneAs<BasicTypeTwoProp, BasicTypeOneProp>());
    }

    // -------------------------------------------------------------------------
    // T2 — ThenCloneAs<DBO, DTO>(Task<List<DBO>>) list overload
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ThenCloneAs_List_HappyPath()
    {
        var sources = new List<BasicTypeTwoProp>
        {
            new() { Value = 1, OtherValue = 10 },
            new() { Value = 2, OtherValue = 20 },
            new() { Value = 3, OtherValue = 30 },
        };

        var result = await Task.FromResult(sources)
            .ThenCloneAs<BasicTypeTwoProp, BasicTypeOneProp>();

        result.Count.ShouldBe(3);
        result[0].Value.ShouldBe(1);
        result[1].Value.ShouldBe(2);
        result[2].Value.ShouldBe(3);
    }

    [Fact]
    public async Task ThenCloneAs_List_WithInit_HappyPath()
    {
        var sources = new List<BasicTypeTwoProp>
        {
            new() { Value = 5, OtherValue = 50 },
        };

        var result = await Task.FromResult(sources)
            .ThenCloneAs<BasicTypeTwoProp, BasicTypeOneProp>((src, clone) =>
                clone.Value = src.OtherValue);

        result.ShouldHaveSingleItem();
        result[0].Value.ShouldBe(50);
    }

    [Fact]
    public async Task ThenCloneAs_List_EmptyList_ReturnsEmptyList()
    {
        var result = await Task.FromResult(new List<BasicTypeTwoProp>())
            .ThenCloneAs<BasicTypeTwoProp, BasicTypeOneProp>();

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task ThenCloneAs_List_FaultedTask_Propagates()
    {
        static async Task<List<BasicTypeTwoProp>> FaultedTask()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }

        await Should.ThrowAsync<InvalidOperationException>(() =>
            FaultedTask().ThenCloneAs<BasicTypeTwoProp, BasicTypeOneProp>());
    }

    // -------------------------------------------------------------------------
    // T3 — ThenCloneAs<DBO, DTO>(Task<DBO[]>) array overload
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ThenCloneAs_Array_HappyPath()
    {
        var sources = new BasicTypeTwoProp[]
        {
            new() { Value = 7, OtherValue = 70 },
            new() { Value = 8, OtherValue = 80 },
        };

        var result = await Task.FromResult(sources)
            .ThenCloneAs<BasicTypeTwoProp, BasicTypeOneProp>();

        result.Length.ShouldBe(2);
        result[0].Value.ShouldBe(7);
        result[1].Value.ShouldBe(8);
    }

    [Fact]
    public async Task ThenCloneAs_Array_WithInit_HappyPath()
    {
        var sources = new BasicTypeTwoProp[]
        {
            new() { Value = 3, OtherValue = 33 },
        };

        var result = await Task.FromResult(sources)
            .ThenCloneAs<BasicTypeTwoProp, BasicTypeOneProp>((src, clone) =>
                clone.Value = src.OtherValue);

        result.ShouldHaveSingleItem();
        result[0].Value.ShouldBe(33);
    }

    [Fact]
    public async Task ThenCloneAs_Array_EmptyArray_ReturnsEmptyArray()
    {
        var result = await Task.FromResult(Array.Empty<BasicTypeTwoProp>())
            .ThenCloneAs<BasicTypeTwoProp, BasicTypeOneProp>();

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task ThenCloneAs_Array_FaultedTask_Propagates()
    {
        static async Task<BasicTypeTwoProp[]> FaultedTask()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }

        await Should.ThrowAsync<InvalidOperationException>(() =>
            FaultedTask().ThenCloneAs<BasicTypeTwoProp, BasicTypeOneProp>());
    }

    // -------------------------------------------------------------------------
    // Ordering — continuation fires after the awaited task resolves
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ThenCloneAs_SingleObject_ContinuationOrderAfterAwait()
    {
        var log = new List<string>();

        async Task<BasicTypeOneProp?> Run()
        {
            log.Add("before");
            var result = await ProduceWithDelay(log)
                .ThenCloneAs<BasicTypeTwoProp, BasicTypeOneProp>();
            log.Add("after");
            return result;
        }

        static async Task<BasicTypeTwoProp?> ProduceWithDelay(List<string> log)
        {
            await Task.Yield();
            log.Add("task resolved");
            return new BasicTypeTwoProp { Value = 1 };
        }

        var clone = await Run();

        log.ShouldBe(["before", "task resolved", "after"]);
        clone.ShouldNotBeNull();
        clone.Value.ShouldBe(1);
    }
}
