namespace RossWright.MetalCore.Tests.CloneAsExtension;

public class CopyToTests
{
    [Fact] public void BasicTypeOneField()
    {
        var source = new BasicTypeOneField { Value = 1 };
        var target = new BasicTypeOneField { Value = 2 };
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        target.Value.ShouldBe(1);
    }
    [Fact] public void BasicTypeOneProp()
    {
        var source = new BasicTypeOneProp { Value = 1 };
        var target = new BasicTypeOneProp { Value = 2 };
        source.CopyTo(target);
        source.ShouldNotBe(target);
        source.Value.ShouldBe(1);
        target.Value.ShouldBe(1);
    }

    [Fact] public void BasicTypeOneField_To_BasicTypeOneProp()
    {
        var source = new BasicTypeOneField { Value = 1 };
        var target = new BasicTypeOneProp { Value = 2 };
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        target.Value.ShouldBe(1);
    }
    [Fact] public void BasicTypeOneProp_To_BasicTypeOneField()
    {
        var source = new BasicTypeOneProp { Value = 1 };
        var target = new BasicTypeOneField { Value = 2 };
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        target.Value.ShouldBe(1);
    }

    [Fact] public void BasicTypeOneField_To_BasicTypeTwoProp()
    {
        var source = new BasicTypeOneField { Value = 1 };
        var target = new BasicTypeTwoProp { Value = 2, OtherValue = 3 };
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        target.Value.ShouldBe(1);
        target.OtherValue.ShouldBe(3);
    }
    [Fact] public void BasicTypeTwoProp_To_BasicTypeOneField()
    {
        var source = new BasicTypeTwoProp { Value = 1, OtherValue = 2 };
        var target = new BasicTypeOneField { Value = 3 };
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        source.OtherValue.ShouldBe(2);
        target.Value.ShouldBe(1);
    }

    [Fact] public void SourceUnknownProp()
    {
        var source = new BasicTypeTwoPropOneUnknown { Value = 1, Unknown = 2 };
        var target = new BasicTypeTwoProp { Value = 3, OtherValue = 4 };
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        source.Unknown.ShouldBe(2);
        target.Value.ShouldBe(1);
        target.OtherValue.ShouldBe(4);
    }
    [Fact] public void DestUnknownProp()
    {
        var source = new BasicTypeTwoProp { Value = 1, OtherValue = 2 };
        var target = new BasicTypeTwoPropOneUnknown { Value = 3, Unknown = 4 };
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        source.OtherValue.ShouldBe(2);
        target.Value.ShouldBe(1);
        target.Unknown.ShouldBe(4);
    }

    [Fact] public void SourceIgnoreProp()
    {
        var source = new BasicTypeTwoPropOneIgnore { Value = 1, OtherValue = 2 };
        var target = new BasicTypeTwoProp { Value = 3, OtherValue = 4 };
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        source.OtherValue.ShouldBe(2);
        target.Value.ShouldBe(1);
        target.OtherValue.ShouldBe(4);
    }
    [Fact] public void DestIgnoreProp()
    {
        var source = new BasicTypeTwoProp { Value = 1, OtherValue = 2 };
        var target = new BasicTypeTwoPropOneIgnore { Value = 3, OtherValue = 4 };
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        source.OtherValue.ShouldBe(2);
        target.Value.ShouldBe(1);
        target.OtherValue.ShouldBe(4);
    }

    [Fact] public void SourceAliasProp()
    {
        var source = new BasicTypeOneAliasProp { OtherValue = 1 };
        var target = new BasicTypeOneProp { Value = 2 };
        source.CopyTo(target);
        source.OtherValue.ShouldBe(1);
        target.Value.ShouldBe(1);
    }
    [Fact] public void DestAliasProp()
    {
        var source = new BasicTypeOneProp { Value = 1 };
        var target = new BasicTypeOneAliasProp { OtherValue = 2 };
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        target.OtherValue.ShouldBe(1);
    }

    [Fact] public void DestAliasPropWithNameCollision()
    {
        var source = new BasicTypeOneProp { Value = 1 };
        var target = new BasicTypeOneAliasPropWithNameCollision { OtherValue = 2 };
        Should.Throw<MetalCoreException>(() => source.CopyTo(target));
    }

    [Fact] public void BasicTypeOnePropWithMethod()
    {
        var source = new BasicTypeOnePropWithMethod { Value = 1 };
        var target = new BasicTypeOnePropWithMethod { Value = 2 };
        source.CopyTo(target);
        source.ShouldNotBe(target);
        source.Value.ShouldBe(1);
        target.Value.ShouldBe(1);
    }

    [Fact] public void ComplexType_ShallowCopy()
    {
        var source = new ComplexTypeWithFieldField { Obj = new BasicTypeOneField { Value = 1 } };
        var target = new ComplexTypeWithFieldField { Obj = new BasicTypeOneField { Value = 2 } };
        source.CopyTo(target);
        target.Obj.ShouldBeSameAs(source.Obj);
    }
    [Fact] public void SourceComplexTypeIsDerived()
    {
        var source = new ComplexTypeWithDerivedType { Obj = new DerivedType { Value = 1, OtherValue = 3 } };
        var target = new ComplexTypeWithFieldField { Obj = new BasicTypeOneField { Value = 2 } };
        source.CopyTo(target);
        target.Obj.ShouldBeSameAs(source.Obj);
    }
    [Fact] public void DestComplexTypeIsDerived()
    {
        var source = new ComplexTypeWithFieldField { Obj = new BasicTypeOneField { Value = 1 } };
        var target = new ComplexTypeWithDerivedType { Obj = new DerivedType { Value = 2, OtherValue = 3 } };
        source.CopyTo(target);
        target.Obj.ShouldNotBeSameAs(source.Obj);
        source.Obj.Value.ShouldBe(1);
        target.Obj.Value.ShouldBe(2);
    }

    [Fact] public void ComplexType_SourceNull()
    {
        var source = new ComplexTypeWithFieldField { Obj = null };
        var target = new ComplexTypeWithFieldField { Obj = new BasicTypeOneField { Value = 2 } };
        source.CopyTo(target);
        source.Obj.ShouldBeNull();
        target.Obj.ShouldBeNull();
    }
    [Fact] public void ComplexType_DestNull()
    {
        var source = new ComplexTypeWithFieldField { Obj = new BasicTypeOneField { Value = 1 } };
        var target = new ComplexTypeWithFieldField { Obj = null };
        source.CopyTo(target);
        target.Obj.ShouldBeSameAs(source.Obj);
    }

    [Fact] public void BasicType_SourceOneReadOnlyField()
    {
        var source = new BasicTypeOneReadOnlyField(1);
        var target = new BasicTypeOneProp { Value = 2 };
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        target.Value.ShouldBe(1);
    }
    [Fact] public void BasicType_DestOneReadOnlyField()
    {
        var source = new BasicTypeOneProp { Value = 1 };
        var target = new BasicTypeOneReadOnlyField(2);
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        target.Value.ShouldBe(2);
    }

    [Fact] public void BasicType_SourceOneReadOnlyProp()
    {
        var source = new BasicTypeOneReadOnlyProp(1);
        var target = new BasicTypeOneProp { Value = 2 };
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        target.Value.ShouldBe(1);
    }
    [Fact] public void BasicType_DestOneReadOnlyProp()
    {
        var source = new BasicTypeOneProp { Value = 1 };
        var target = new BasicTypeOneReadOnlyProp(2);
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        target.Value.ShouldBe(2);
    }

    [Fact] public void BasicType_SourceOneWriteOnlyProp()
    {
        var source = new BasicTypeOneWriteOnlyProp { Value = 1 };
        var target = new BasicTypeOneProp { Value = 2 };
        source.CopyTo(target);
        source.GetValue().ShouldBe(1);
        target.Value.ShouldBe(2);
    }
    [Fact] public void BasicType_DestOneWriteOnlyProp()
    {
        var source = new BasicTypeOneProp { Value = 1 };
        var target = new BasicTypeOneWriteOnlyProp();
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        target.GetValue().ShouldBe(1);
    }

    [Fact] public void FieldTypeMismatch()
    {
        var source = new BasicTypeOneStringField { Value = "1" };
        var target = new BasicTypeOneField { Value = 2 };
        source.CopyTo(target);
        source.Value.ShouldBe("1");
        target.Value.ShouldBe(2); // string cannot convert to int
        target.CopyTo(source);
        source.Value.ShouldBe("2"); // int can convert to string
        target.Value.ShouldBe(2);
    }

    [Fact] public void PropTypeMismatch()
    {
        var source = new BasicTypeOneStringProp { Value = "1" };
        var target = new BasicTypeTwoProp { Value = 2 };
        source.CopyTo(target);
        source.Value.ShouldBe("1");
        target.Value.ShouldBe(2); // string cannot convert to int
        target.CopyTo(source);
        source.Value.ShouldBe("2"); // int can convert to string
        target.Value.ShouldBe(2);
    }

    [Fact] public void BasicTypeWithPrivateField()
    {
        var source = new BasicTypeWithPrivateField(1, 2);
        var target = new BasicTypeWithPrivateField(3, 4);
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        source.GetOtherValue().ShouldBe(2);
        target.Value.ShouldBe(1);
        target.GetOtherValue().ShouldBe(4);
    }

    [Fact] public void BasicTypeWithPrivateProp()
    {
        var source = new BasicTypeOneProp { Value = 1 };
        var target = new BasicTypeWithPrivateProp(2, 3, 4);
        source.CopyTo(target);
        source.Value.ShouldBe(1);
        target.Value.ShouldBe(1);
        target.GetOtherValue().ShouldBe(3);
        target.GetAnotherValue().ShouldBe(4);
    }

    [Fact] public void SourceNull()
    {
        BasicTypeOneField? source = null;
        var target = new BasicTypeOneField { Value = 2 };
        Should.Throw<NullReferenceException>(() => source!.CopyTo(target));
    }
    [Fact] public void DestNull()
    {
        var source = new BasicTypeOneField { Value = 2 };
        BasicTypeOneField? b = null;
        Should.Throw<NullReferenceException>(() => source.CopyTo(b!));
    }

    [Fact] public void ComplexType_MixedTypes_FieldField_FieldProp()
    {
        var source = new ComplexTypeWithFieldField { Obj = new BasicTypeOneField { Value = 1 } };
        var target = new ComplexTypeWithFieldProp { Obj = new BasicTypeOneField { Value = 2 } };
        source.CopyTo(target);
        target.Obj.ShouldBeSameAs(source.Obj);
    }
    [Fact] public void ComplexType_MixedTypes_FieldField_PropField()
    {
        var source = new ComplexTypeWithFieldField { Obj = new BasicTypeOneField { Value = 1 } };
        var target = new ComplexTypeWithPropField { Obj = new BasicTypeOneProp { Value = 2 } };
        source.CopyTo(target);
        target.Obj.ShouldNotBeSameAs(source.Obj);
        source.Obj.Value.ShouldBe(1);
        target.Obj.Value.ShouldBe(2);
    }

    [Fact] public void Copy_Nullable_Prop_With_Value_To_NonNullable()
    {
        var source = new ObjWithNullableEnum { DayOfWeek = DayOfWeek.Monday };
        var target = new ObjWithoutNullableEnum();
        source.CopyTo(target);
        target.DayOfWeek.ShouldBe(DayOfWeek.Monday);

    }
    [Fact] public void Copy_Nullable_Prop_Without_Value_To_NonNullable()
    {
        var source = new ObjWithNullableEnum();
        var target = new ObjWithoutNullableEnum { DayOfWeek = DayOfWeek.Tuesday };
        source.CopyTo(target);
        target.DayOfWeek.ShouldBe(DayOfWeek.Tuesday);
    }
    [Fact] public void Copy_NonNullable_Prop_To_Nullable()
    {
        var source = new ObjWithoutNullableEnum { DayOfWeek = DayOfWeek.Wednesday };
        var target = new ObjWithNullableEnum();
        source.CopyTo(target);
        target.DayOfWeek.ShouldBe(DayOfWeek.Wednesday);
    }
}
