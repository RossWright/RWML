namespace RossWright.MetalCore.Tests.CloneAsExtension;

public class CloneAsTests
{
    [Fact] public void Clone_BasicTypeOneField()
    {
        var a = new BasicTypeOneField { Value = 1 };
        var b = a.Clone();
        a.Value.ShouldBe(1);
        b.Value.ShouldBe(1);
    }
    [Fact] public void Clone_Init()
    {
        var a = new BasicTypeOneProp { Value = 1 };
        BasicTypeOneProp? initObj = null;
        var b = a.Clone(_ =>
        {
            initObj = _;
            _.ShouldNotBeSameAs(a);
            _.Value.ShouldBe(1);
        });
        a.ShouldNotBe(b);
        a.Value.ShouldBe(1);
        b.Value.ShouldBe(1);
        initObj.ShouldBeSameAs(b);
    }

    [Fact] public void BasicTypeOneField()
    {
        var a = new BasicTypeOneField { Value = 1 };
        var b = a.CloneAs<BasicTypeOneField>();
        a.Value.ShouldBe(1);
        b.Value.ShouldBe(1);
    }

    [Fact] public void Init()
    {
        var a = new BasicTypeOneProp { Value = 1 };
        BasicTypeOneField? initObj = null;
        var b = a.CloneAs<BasicTypeOneField>(_ =>
        {
            initObj = _;
            _.ShouldNotBeSameAs(a);
            _.Value.ShouldBe(1);
        });
        a.Value.ShouldBe(1);
        b.Value.ShouldBe(1);
        initObj.ShouldBeSameAs(b);
    }

    [Fact] public void NullOriginal()
    {
        BasicTypeOneField? a = null;
        Should.Throw<NullReferenceException>(() => a!.CloneAs<BasicTypeOneField>());
    }
    [Fact] public void NullOriginal_Init()
    {
        BasicTypeOneField? a = null;
        Should.Throw<NullReferenceException>(() => 
            a!.CloneAs<BasicTypeOneField>(_ =>
                Assert.Fail("Init should not be called if original is null")));
    }

    [Fact] public void MixedTypes_Contained()
    {
        var a = new DerivedType { Value = 1, OtherValue = 2 };
        var b = a.CloneAs<BasicTypeOneField>();
        b.ShouldBeOfType<BasicTypeOneField>();
        b.ShouldNotBeOfType<DerivedType>();
        a.Value.ShouldBe(1);
        a.OtherValue.ShouldBe(2);
        b.Value.ShouldBe(1);
    }
    [Fact] public void MixedTypes_Uncontained()
    {
        var a = new BasicTypeOneField { Value = 1 };
        var b = a.CloneAs<DerivedType>();
        b.ShouldBeOfType<DerivedType>();
        a.Value.ShouldBe(1);
        b.Value.ShouldBe(1);
        b.OtherValue.ShouldBe(0);
    }

    [Fact] public void CloneAs_Collection_OneType()
    {
        var a = new BasicTypeOneField[]
        {
            new BasicTypeOneField { Value = 1 },
            new BasicTypeOneField { Value = 2 },
            new BasicTypeOneField { Value = 3 },
        };
        var b = a.CloneAs<BasicTypeOneField>();
        b.ShouldNotBeSameAs(a);
        b.Length.ShouldBe(3);
        Assert.Collection(b,
            _ => 
            {
                _.ShouldNotBeSameAs(a[0]);
                _.Value.ShouldBe(1);
            },
            _ =>
            {
                _.ShouldNotBeSameAs(a[1]);
                _.Value.ShouldBe(2);
            },
            _ =>
            {
                _.ShouldNotBeSameAs(a[2]);
                _.Value.ShouldBe(3);
            });
    }

    [Fact] public void CloneAs_Collection_Init_OneType()
    {
        var a = new BasicTypeOneField[]
        {
            new BasicTypeOneField { Value = 1 },
            new BasicTypeOneField { Value = 2 },
            new BasicTypeOneField { Value = 3 },
        };
        var encountered = new List<BasicTypeOneField>();
        var b = a.CloneAs<BasicTypeOneField>(encountered.Add);
        encountered.Count.ShouldBe(3);
        Assert.Collection(encountered,
            _ =>
            {
                _.ShouldBeSameAs(b[0]);
                _.Value.ShouldBe(1);
            },
            _ =>
            {
                _.ShouldBeSameAs(b[1]);
                _.Value.ShouldBe(2);
            },
            _ =>
            {
                _.ShouldBeSameAs(b[2]);
                _.Value.ShouldBe(3);
            });
    }

    [Fact] public void CloneAs_Collection_Init_InOut_OneType()
    {
        var a = new BasicTypeOneField[]
        {
            new BasicTypeOneField { Value = 1 },
            new BasicTypeOneField { Value = 2 },
            new BasicTypeOneField { Value = 3 },
        };
        var encountered = new List<(object, BasicTypeOneField)>();
        var b = a.CloneAs<BasicTypeOneField>((aObj, bObj) => encountered.Add((aObj, bObj)));
        encountered.Count.ShouldBe(3);
        encountered[0].Item1.ShouldBeSameAs(a[0]);
        encountered[0].Item2.ShouldBeSameAs(b[0]);
        encountered[0].Item2.Value.ShouldBe(1);
        encountered[1].Item1.ShouldBeSameAs(a[1]);
        encountered[1].Item2.ShouldBeSameAs(b[1]);
        encountered[1].Item2.Value.ShouldBe(2);
        encountered[2].Item1.ShouldBeSameAs(a[2]);
        encountered[2].Item2.ShouldBeSameAs(b[2]);
        encountered[2].Item2.Value.ShouldBe(3);
    }

    // ── P3-B: CloneAs<DBO,DTO> strongly-typed collection overload ──────────────
    [Fact]
    public void CloneAs_StronglyTyped_CollectionWithoutCallback_ClonesAllItems()
    {
        var source = new List<BasicTypeOneProp> { new() { Value = 1 }, new() { Value = 2 } };
        var result = source.CloneAs<BasicTypeOneProp, BasicTypeTwoProp>().ToList();
        result.Count.ShouldBe(2);
        result[0].Value.ShouldBe(1);
        result[1].Value.ShouldBe(2);
    }

    [Fact]
    public void CloneAs_StronglyTyped_CollectionWithCallback_CallbackAppliedToEach()
    {
        var source = new List<BasicTypeOneProp> { new() { Value = 1 }, new() { Value = 2 } };
        var result = source.CloneAs<BasicTypeOneProp, BasicTypeTwoProp>((_, dst) => dst.OtherValue = 99).ToList();
        result.ShouldAllBe(item => item.OtherValue == 99);
    }

    [Fact]
    public void CloneAs_StronglyTyped_EmptyCollection_ReturnsEmpty()
    {
        var result = new List<BasicTypeOneProp>().CloneAs<BasicTypeOneProp, BasicTypeTwoProp>();
        result.ShouldBeEmpty();
    }
}
