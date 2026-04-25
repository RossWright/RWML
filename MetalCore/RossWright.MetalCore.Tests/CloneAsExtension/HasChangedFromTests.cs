namespace RossWright.MetalCore.Tests.CloneAsExtension;

public class HasChangedFromTests
{
    [Fact] public void BasicType()
    {
        var a = new BasicTypeOneProp { Value = 1 };
        var b = new BasicTypeOneProp { Value = 2 };
        b.HasChangedFrom(a).ShouldBeTrue();
        a.HasChangedFrom(b).ShouldBeTrue();
    }

    [Fact] public void ComplexType_FieldProp()
    {
        var a = new ComplexTypeWithFieldProp { Obj = new BasicTypeOneField { Value = 1 } };
        var b = new ComplexTypeWithFieldProp { Obj = new BasicTypeOneField { Value = 1 } };
        b.HasChangedFrom(a).ShouldBeTrue();
        a.HasChangedFrom(b).ShouldBeTrue();
        a.Obj.ShouldNotBeSameAs(b.Obj);
    }
    [Fact] public void ComplexType_FieldField()
    {
        var a = new ComplexTypeWithFieldField { Obj = new BasicTypeOneField { Value = 1 } };
        var b = new ComplexTypeWithFieldField { Obj = new BasicTypeOneField { Value = 1 } };
        b.HasChangedFrom(a).ShouldBeTrue();
        a.HasChangedFrom(b).ShouldBeTrue();
        a.Obj.ShouldNotBeSameAs(b.Obj);
    }
    [Fact] public void ComplexType_PropProp()
    {
        var a = new ComplexTypeWithPropProp { Obj = new BasicTypeOneProp { Value = 1 } };
        var b = new ComplexTypeWithPropProp { Obj = new BasicTypeOneProp { Value = 1 } };
        b.HasChangedFrom(a).ShouldBeTrue();
        a.HasChangedFrom(b).ShouldBeTrue();
        a.Obj.ShouldNotBeSameAs(b.Obj);
    }
    [Fact] public void ComplexType_PropField()
    {
        var a = new ComplexTypeWithFieldProp { Obj = new BasicTypeOneField { Value = 1 } };
        var b = new ComplexTypeWithFieldProp { Obj = new BasicTypeOneField { Value = 1 } };
        b.HasChangedFrom(a).ShouldBeTrue();
        a.HasChangedFrom(b).ShouldBeTrue();
        a.Obj.ShouldNotBeSameAs(b.Obj);
    }

    [Fact] public void ComplexType_ShallowCopy()
    {
        var obj = new BasicTypeOneField { Value = 1 };
        var a = new ComplexTypeWithFieldField { Obj = obj };
        var b = new ComplexTypeWithFieldField { Obj = obj };
        b.HasChangedFrom(a).ShouldBeFalse();
        a.HasChangedFrom(b).ShouldBeFalse();
        a.Obj.ShouldBeSameAs(b.Obj);
    }

    [Fact] public void ComplexType_OneNullValue()
    {
        var a = new ComplexTypeWithFieldField { Obj = null };
        var b = new ComplexTypeWithFieldField { Obj = new BasicTypeOneField { Value = 1 } };
        b.HasChangedFrom(a).ShouldBeTrue();
        a.HasChangedFrom(b).ShouldBeTrue();
    }

    [Fact] public void ComplexType_BothNullValue()
    {
        var a = new ComplexTypeWithFieldField { Obj = null };
        var b = new ComplexTypeWithFieldField { Obj = null };
        b.HasChangedFrom(a).ShouldBeFalse();
        a.HasChangedFrom(b).ShouldBeFalse();
    }

    [Fact] public void BothNull()
    {
        BasicTypeOneProp? a = null;
        BasicTypeOneProp? b = null;
        b.HasChangedFrom(a).ShouldBeFalse();
        a.HasChangedFrom(b).ShouldBeFalse();
    }

    [Fact] public void Null()
    {
        var a = new BasicTypeOneProp { Value = 1 };
        BasicTypeOneProp? b = null;
        b.HasChangedFrom(a).ShouldBeTrue();
        a.HasChangedFrom(b).ShouldBeTrue();
    }

    [Fact] public void MismatchedType()
    {
        var a = new BasicTypeOneField { Value = 1 };
        var b = new DerivedType { Value = 1, OtherValue = 3 };
        b.HasChangedFrom(a).ShouldBeFalse();
        a.HasChangedFrom(b).ShouldBeFalse();
    }

    [Fact] public void Ignore()
    {
        var a = new BasicTypeTwoPropOneIgnore { Value = 1, OtherValue = 2 };
        var b = new BasicTypeTwoPropOneIgnore { Value = 1, OtherValue = 3 };
        b.HasChangedFrom(a).ShouldBeFalse();
    }
}