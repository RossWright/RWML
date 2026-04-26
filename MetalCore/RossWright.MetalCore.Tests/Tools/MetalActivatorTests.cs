using System.Globalization;
using System.Reflection;
namespace RossWright.MetalCore.Tests;

#pragma warning disable CS9113

public class MetalActivatorTests
{
    [Fact] public void HappyPath()
    {
        MetalActivator.CreateInstance<TestClass>(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
                null, [1, "2"], null).ShouldNotBeNull();
        MetalActivator.CreateInstance<TestClass>(1, "2").ShouldNotBeNull();
        MetalActivator.CreateInstance<TestClass>([1, "2"], null).ShouldNotBeNull();
        MetalActivator.CreateInstance(typeof(TestClass),
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
                    null, [1, "2"], null).ShouldNotBeNull();
        MetalActivator.CreateInstance(typeof(TestClass), 1, "2").ShouldNotBeNull();
        MetalActivator.CreateInstance(typeof(TestClass), [1, "2"], null).ShouldNotBeNull();
    }

    public class TestClass(int x, string f) { }

    [Fact] public void IgnoringOptionalParameters()
    {
        MetalActivator.CreateInstance<TestClassWithOptionalParameters>(["1", 1]).ShouldNotBeNull();
        MetalActivator.CreateInstance<TestClassWithOptionalParameters>(["1"]).ShouldNotBeNull();
    }

    public class TestClassWithOptionalParameters(string req, int opt = 0) { }


    [Fact] public void IgnoringOptionalParametersWithMultipleConstructors()
    {
        var obj = MetalActivator.CreateInstance<TestClassWithMultipleConstructors>(["1", 1]);
        obj.ShouldNotBeNull();
        obj.CtorCalled.ShouldBe(3);

        obj = MetalActivator.CreateInstance<TestClassWithMultipleConstructors>(["1"]);
        obj.ShouldNotBeNull();
        obj.CtorCalled.ShouldBe(2);

        obj = MetalActivator.CreateInstance<TestClassWithMultipleConstructors>();
        obj.ShouldNotBeNull();
        obj.CtorCalled.ShouldBe(1);
    }

    public class TestClassWithMultipleConstructors
    {
        public TestClassWithMultipleConstructors() => CtorCalled = 1;
        public TestClassWithMultipleConstructors(string req) => CtorCalled = 2;
        public TestClassWithMultipleConstructors(string req, int opt = 0) => CtorCalled = 3;

        public int CtorCalled = -1;
    }

    [Fact] public void FailWithMismatchedParameters()
    {
        MetalActivator.CreateInstance<TestClassWithMultipleConstructors>([1, "1"]).ShouldBeNull();
        MetalActivator.CreateInstance<TestClassWithMultipleConstructors>(["1", 1, "bad"]).ShouldBeNull();
    }

    [Fact]
    public void CreateInstanceFrom_ValidAssemblyAndType_ReturnsHandle()
    {
        var assemblyPath = typeof(MetalCoreException).Assembly.Location;
        var typeName = typeof(MetalCoreException).FullName!;

        var handle = MetalActivator.CreateInstanceFrom(assemblyPath, typeName);

        handle.ShouldNotBeNull();
        handle.Unwrap().ShouldBeOfType<MetalCoreException>();
    }

    [Fact]
    public void CreateInstanceFrom_WithActivationAttributes_ReturnsHandle()
    {
        var assemblyPath = typeof(MetalCoreException).Assembly.Location;
        var typeName = typeof(MetalCoreException).FullName!;

        var handle = MetalActivator.CreateInstanceFrom(assemblyPath, typeName, null);

        handle.ShouldNotBeNull();

        handle.Unwrap().ShouldBeOfType<MetalCoreException>();
    }

    [Fact]
    public void CreateInstanceFrom_MissingAssemblyPath_Throws()
    {
        Should.Throw<FileNotFoundException>(() =>
            MetalActivator.CreateInstanceFrom("nonexistent.dll", "Any.Type"));
    }

    [Fact]
    public void CreateInstanceFrom_TypeNotFoundInAssembly_Throws()
    {
        var assemblyPath = typeof(MetalActivator).Assembly.Location;

        Should.Throw<TypeLoadException>(() =>
            MetalActivator.CreateInstanceFrom(assemblyPath, "Does.Not.Exist.Type"));
    }

    [Fact]
    public void CreateInstanceGeneric_WithBindingFlags_NoArgs_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
            null,
            null,
            null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithBindingFlags_WithArgs_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClass>(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
            null,
            new object?[] { 42, "test" },
            null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithBindingFlags_NullArgs_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
            null,
            null,
            null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithBindingFlags_EmptyArgs_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
            null,
            Array.Empty<object>(),
            null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithBindingFlags_DefaultBindingFlags_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(
            null,
            null,
            null,
            null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithBindingFlags_NonPublicConstructor_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithPrivateConstructor>(
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
            null,
            null,
            null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithBindingFlags_PublicOnly_ThrowsForPrivate()
    {
        Should.Throw<MissingMethodException>(() =>
            MetalActivator.CreateInstance<TestClassWithPrivateConstructor>(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
                null,
                null,
                null));
    }

    [Fact]
    public void CreateInstanceGeneric_ParamsArgs_NoArgs_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>();

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_ParamsArgs_NullArgs_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_ParamsArgs_EmptyArgs_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(Array.Empty<object>());

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_ParamsArgs_WithSingleArg_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithSingleArg>("test");

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_ParamsArgs_WithMultipleArgs_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClass>(42, "test");

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_ParamsArgs_MismatchedArgs_ReturnsNull()
    {
        var result = MetalActivator.CreateInstance<TestClass>("wrong", "types");

        result.ShouldBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_ParamsArgs_WithOptionalParams_UsesDefaults()
    {
        var result = MetalActivator.CreateInstance<TestClassWithOptionalParameters>("required");

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithActivationAttributes_NoArgs_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(null, null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithActivationAttributes_WithArgs_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClass>(new object?[] { 42, "test" }, null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithActivationAttributes_NullArgs_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(null, null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithActivationAttributes_EmptyArgs_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(Array.Empty<object>(), null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithActivationAttributes_MismatchedArgs_ReturnsNull()
    {
        var result = MetalActivator.CreateInstance<TestClass>(new object?[] { "wrong", "types" }, null);

        result.ShouldBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithActivationAttributes_WithOptionalParams_UsesDefaults()
    {
        var result = MetalActivator.CreateInstance<TestClassWithOptionalParameters>(new object?[] { "required" }, null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithBindingFlags_WithCultureInfo_CreatesInstance()
    {
        var culture = CultureInfo.InvariantCulture;
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
            null,
            null,
            culture);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_ParamsArgs_MultipleConstructors_SelectsBest()
    {
        var result = MetalActivator.CreateInstance<TestClassWithMultipleConstructors>("test", 42);

        result.ShouldNotBeNull();
        result.CtorCalled.ShouldBe(3);
    }

    [Fact]
    public void CreateInstanceGeneric_WithBindingFlags_WithArgs_SelectsMatchingConstructor()
    {
        var result = MetalActivator.CreateInstance<TestClass>(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
            null,
            new object?[] { 42, "test" },
            null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_ParamsArgs_ExcessArgs_ReturnsNull()
    {
        var result = MetalActivator.CreateInstance<TestClassWithSingleArg>("test", "extra", "args");

        result.ShouldBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithActivationAttributes_ExcessArgs_ReturnsNull()
    {
        var result = MetalActivator.CreateInstance<TestClassWithSingleArg>(
            new object?[] { "test", "extra", "args" }, 
            null);

        result.ShouldBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithBindingFlags_ComplexArgs_CreatesInstance()
    {
        var complexArg = new TestClassWithDefaultConstructor();
        var result = MetalActivator.CreateInstance<TestClassWithComplexArg>(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
            null,
            new object?[] { complexArg },
            null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_ParamsArgs_ComplexArgs_CreatesInstance()
    {
        var complexArg = new TestClassWithDefaultConstructor();
        var result = MetalActivator.CreateInstance<TestClassWithComplexArg>(complexArg);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithActivationAttributes_ComplexArgs_CreatesInstance()
    {
        var complexArg = new TestClassWithDefaultConstructor();
        var result = MetalActivator.CreateInstance<TestClassWithComplexArg>(
            new object?[] { complexArg }, 
            null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithBindingFlags_InheritedArgs_CreatesInstance()
    {
        var derivedArg = new DerivedTestClass();
        var result = MetalActivator.CreateInstance<TestClassWithBaseArg>(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
            null,
            new object?[] { derivedArg },
            null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_ParamsArgs_InheritedArgs_CreatesInstance()
    {
        var derivedArg = new DerivedTestClass();
        var result = MetalActivator.CreateInstance<TestClassWithBaseArg>(derivedArg);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithActivationAttributes_InheritedArgs_CreatesInstance()
    {
        var derivedArg = new DerivedTestClass();
        var result = MetalActivator.CreateInstance<TestClassWithBaseArg>(
            new object?[] { derivedArg }, 
            null);

        result.ShouldNotBeNull();
    }

    public class TestClassWithDefaultConstructor { }

    public class TestClassWithSingleArg(string value) { }

    public class TestClassWithPrivateConstructor
    {
        private TestClassWithPrivateConstructor() { }
    }

    public class TestClassWithNullableArg(string? value) { }

    public class TestClassWithComplexArg(TestClassWithDefaultConstructor value) { }

    public class BaseTestClass { }

    public class DerivedTestClass : BaseTestClass { }

    public class TestClassWithBaseArg(BaseTestClass value) { }

    [Fact]
    public void CreateInstanceGeneric_WithActivationAttributes_EmptyAttributesArray_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(
            Array.Empty<object>(),
            Array.Empty<object>());

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithActivationAttributes_NonEmptyAttributesArray_ThrowsPlatformNotSupported()
    {
        var activationAttrs = new object[] { "attr1", "attr2" };
        
        Should.Throw<PlatformNotSupportedException>(() =>
            MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(
                null,
                activationAttrs));
    }

    [Fact]
    public void CreateInstanceGeneric_WithBindingFlags_WithBinder_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
            null,
            null,
            null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithBindingFlags_WithCurrentCulture_CreatesInstance()
    {
        var culture = CultureInfo.CurrentCulture;
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
            null,
            null,
            culture);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithBindingFlags_AllParametersNull_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(
            null,
            null,
            null,
            null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_ParamsArgs_SingleElementArray_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithSingleArg>(new object[] { "value" });

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithActivationAttributes_BothArgsNull_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(null, null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithActivationAttributes_EmptyArgsNullAttributes_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithDefaultConstructor>(
            Array.Empty<object>(),
            null);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_ParamsArgs_ArrayWithMultipleTypes_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithMixedArgs>(
            42,
            "test",
            3.14);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void CreateInstanceGeneric_WithActivationAttributes_ArrayWithMultipleTypes_CreatesInstance()
    {
        var result = MetalActivator.CreateInstance<TestClassWithMixedArgs>(
            new object[] { 42, "test", 3.14 },
            null);

        result.ShouldNotBeNull();
    }

    public class TestClassWithMixedArgs(int i, string s, double d) { }
}