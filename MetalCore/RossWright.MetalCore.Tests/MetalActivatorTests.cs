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
}