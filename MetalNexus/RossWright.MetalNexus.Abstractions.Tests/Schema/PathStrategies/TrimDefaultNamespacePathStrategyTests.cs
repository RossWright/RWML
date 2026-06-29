using System.Reflection;
using RossWright.MetalNexus.Schema.PathStrategies;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies
{
    public class TrimDefaultNamespacePathStrategyTests
    {
        [Fact]
        public void Constructor_DefaultThreshold_SetsToEightyPercent()
        {
            var strategy = new TestableStrategy();
            strategy.GetThreshold().ShouldBe(0.8);
        }

        [Fact]
        public void Constructor_CustomThreshold_SetsThreshold()
        {
            var strategy = new TestableStrategy(threshold: 0.5);
            strategy.GetThreshold().ShouldBe(0.5);
        }

        [Fact]
        public void Constructor_ZeroThreshold_SetsThreshold()
        {
            var strategy = new TestableStrategy(threshold: 0.0);
            strategy.GetThreshold().ShouldBe(0.0);
        }

        [Fact]
        public void Constructor_OneThreshold_SetsThreshold()
        {
            var strategy = new TestableStrategy(threshold: 1.0);
            strategy.GetThreshold().ShouldBe(1.0);
        }

        [Fact]
        public void Constructor_NegativeThreshold_SetsThreshold()
        {
            var strategy = new TestableStrategy(threshold: -0.5);
            strategy.GetThreshold().ShouldBe(-0.5);
        }

        [Fact]
        public void Constructor_ThresholdGreaterThanOne_SetsThreshold()
        {
            var strategy = new TestableStrategy(threshold: 1.5);
            strategy.GetThreshold().ShouldBe(1.5);
        }

        [Fact]
        public void Trim_TypeWithNoNamespace_ReturnsNull()
        {
            var types = new[] { typeof(NoNamespaceType) };
            var strategy = new TestableStrategy(types);
            var result = strategy.Trim(typeof(NoNamespaceType));
            result.ShouldBeNull();
        }

        [Fact]
        public void Trim_TypeInSingleNamespace_FullyTrims()
        {
            var types = new[]
            {
                typeof(TestNamespaceA.Type1),
                typeof(TestNamespaceA.Type2),
                typeof(TestNamespaceA.Type3)
            };
            var strategy = new TestableStrategy(types);
            var result = strategy.Trim(typeof(TestNamespaceA.Type1));
            result.ShouldBeNull();
        }

        [Fact]
        public void Trim_TypeInNestedNamespace_TrimsCommonPart()
        {
            var types = new[]
            {
                typeof(TestNamespaceB.Sub1.Type1),
                typeof(TestNamespaceB.Sub1.Type2),
                typeof(TestNamespaceB.Sub2.Type3)
            };
            var strategy = new TestableStrategy(types);
            var result = strategy.Trim(typeof(TestNamespaceB.Sub1.Type1));
            result.ShouldBe("Sub1");
        }

        [Fact]
        public void Trim_NestedClass_HandlesPlus()
        {
            var types = new[] { typeof(TestNamespaceC.Container.NestedType) };
            var strategy = new TestableStrategy(types);
            var result = strategy.Trim(typeof(TestNamespaceC.Container.NestedType));
            result.ShouldBe("Container");
        }

        [Fact]
        public void Trim_LowThreshold_AcceptsLowerConsensus()
        {
            var types = new[]
            {
                typeof(TestNamespaceD.Type1),
                typeof(TestNamespaceD.Type2),
                typeof(TestNamespaceE.Type3)
            };
            var strategy = new TestableStrategy(types, threshold: 0.5);
            var result = strategy.Trim(typeof(TestNamespaceD.Type1));
            result.ShouldBeNull();
        }

        [Fact]
        public void Trim_HighThreshold_RequiresMoreConsensus()
        {
            var types = new[]
            {
                typeof(TestNamespaceF.Type1),
                typeof(TestNamespaceF.Type2),
                typeof(TestNamespaceG.Type3)
            };
            var strategy = new TestableStrategy(types, threshold: 0.9);
            var result = strategy.Trim(typeof(TestNamespaceF.Type1));
            result.ShouldBe("TestNamespaceF");
        }

        [Fact]
        public void Trim_TypeWithNullNamespace_HandlesGracefully()
        {
            var mockType = new MockTypeWithNullNamespace("NoNs", null);
            var types = new[] { mockType };
            var strategy = new TestableStrategy(types);
            var result = strategy.Trim(mockType);
            result.ShouldBeNull();
        }

        [Fact]
        public void Trim_MixedTypesWithAndWithoutNamespace_FiltersNullNamespaces()
        {
            var types = new[]
            {
                typeof(TestNamespaceH.Type1),
                typeof(TestNamespaceH.Type2),
                new MockTypeWithNullNamespace("NoNs", null)
            };
            var strategy = new TestableStrategy(types);
            var result = strategy.Trim(typeof(TestNamespaceH.Type1));
            // Null namespace types are filtered out, but only 2 types with 100% consensus isn't enough
            // Since the mock type has same assembly, namespace detection considers all test types
            result.ShouldBe("RossWright/MetalNexus/Abstractions/UnitTests/Schema/PathStrategies/TestNamespaceH");
        }

        [Fact]
        public void Trim_AssemblyCaching_ReusesDetectedNamespace()
        {
            var types = new[]
            {
                typeof(TestNamespaceI.Type1),
                typeof(TestNamespaceI.Type2)
            };
            var strategy = new TestableStrategy(types);
            var result1 = strategy.Trim(typeof(TestNamespaceI.Type1));
            var result2 = strategy.Trim(typeof(TestNamespaceI.Type2));
            result1.ShouldBeNull();
            result2.ShouldBeNull();
        }

        [Fact]
        public void Trim_DeepNamespaceHierarchy_TrimsCommonPrefix()
        {
            var types = new[]
            {
                typeof(TestNamespaceJ.L1.L2.L3.Type1),
                typeof(TestNamespaceJ.L1.L2.L3.Type2),
                typeof(TestNamespaceJ.L1.L2.L3.Type3)
            };
            var strategy = new TestableStrategy(types);
            var result = strategy.Trim(typeof(TestNamespaceJ.L1.L2.L3.Type1));
            // All types share the exact same namespace, so fully trimmed
            result.ShouldBeNull();
        }

        [Fact]
        public void Trim_NoTypesInAssembly_HandlesEmptyArray()
        {
            var types = Array.Empty<Type>();
            var strategy = new TestableStrategy(types);
            var mockType = new MockTypeWithNullNamespace("Test.Type", "Test");
            var result = strategy.Trim(mockType);
            result.ShouldBe("Test");
        }

        [Fact]
        public void Trim_ZeroThreshold_NoCommonPrefixDetected()
        {
            var types = new[]
            {
                typeof(TestNamespaceK.Type1),
                typeof(TestNamespaceK.Type2)
            };
            var strategy = new TestableStrategy(types, threshold: 0.0);
            var result = strategy.Trim(typeof(TestNamespaceK.Type1));
            // With 0% threshold, nothing meets consensus, so fully trimmed since all in same namespace
            result.ShouldBeNull();
        }

        [Fact]
        public void Trim_PartialNamespaceMatch_TrimsCommonPrefix()
        {
            var types = new[]
            {
                typeof(TestNamespaceL.Match.Type1),
                typeof(TestNamespaceL.Match.Type2),
                typeof(TestNamespaceL.Different.Type3)
            };
            var strategy = new TestableStrategy(types, threshold: 0.6);
            var result = strategy.Trim(typeof(TestNamespaceL.Match.Type1));
            // All share "...TestNamespaceL" at 100%, then Match at 66.7% which meets 60% threshold
            result.ShouldBeNull();
        }

        [Fact]
        public void GetConsideredTypes_ReturnsTypesFromAssembly()
        {
            var strategy = new TrimDefaultNamespacePathStrategy();
            var assembly = typeof(TrimDefaultNamespacePathStrategy).Assembly;
            var result = strategy.GetType()
                .GetMethod("GetConsideredTypes", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(strategy, new[] { assembly }) as Type[];
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public void Trim_RealAssemblyTypes_WorksWithActualImplementation()
        {
            var strategy = new TrimDefaultNamespacePathStrategy();
            var result = strategy.Trim(typeof(TrimDefaultNamespacePathStrategy));
            result.ShouldNotBeNull();
        }

        [Fact]
        public void Trim_SingleTypeInNamespace_ReturnsNull()
        {
            var types = new[] { typeof(TestNamespaceM.Type1) };
            var strategy = new TestableStrategy(types);
            var result = strategy.Trim(typeof(TestNamespaceM.Type1));
            result.ShouldBeNull();
        }

        [Fact]
        public void Trim_ThresholdExactlyMet_TrimsNamespace()
        {
            var types = new[]
            {
                typeof(TestNamespaceN.Type1),
                typeof(TestNamespaceN.Type2),
                typeof(TestNamespaceN.Type3),
                typeof(TestNamespaceN.Type4),
                typeof(TestNamespaceO.Type5)
            };
            var strategy = new TestableStrategy(types, threshold: 0.8);
            var result = strategy.Trim(typeof(TestNamespaceN.Type1));
            result.ShouldBeNull();
        }

        [Fact]
        public void Trim_ThresholdNotMet_DoesNotTrimNamespace()
        {
            var types = new[]
            {
                typeof(TestNamespaceP.Type1),
                typeof(TestNamespaceP.Type2),
                typeof(TestNamespaceP.Type3),
                typeof(TestNamespaceQ.Type4),
                typeof(TestNamespaceQ.Type5)
            };
            var strategy = new TestableStrategy(types, threshold: 0.61);
            var result = strategy.Trim(typeof(TestNamespaceP.Type1));
            result.ShouldBe("TestNamespaceP");
        }

        private class TestableStrategy : TrimDefaultNamespacePathStrategy
        {
            private readonly Type[]? _typesToReturn;

            public TestableStrategy(Type[]? typesToReturn = null, double threshold = 0.8)
                : base(threshold)
            {
                _typesToReturn = typesToReturn;
            }

            protected override Type[] GetConsideredTypes(Assembly assembly)
            {
                return _typesToReturn ?? base.GetConsideredTypes(assembly);
            }

            public double GetThreshold() => Threshold;
        }

        private class MockTypeWithNullNamespace : Type
        {
            private readonly string _fullName;
            private readonly string? _namespace;

            public MockTypeWithNullNamespace(string fullName, string? ns)
            {
                _fullName = fullName;
                _namespace = ns;
            }

            public override string FullName => _fullName;
            public override string? Namespace => _namespace;
            public override Assembly Assembly => typeof(MockTypeWithNullNamespace).Assembly;
            public override string Name => _fullName.Split('.').Last();

            public override Type? BaseType => throw new NotImplementedException();
            public override string? AssemblyQualifiedName => throw new NotImplementedException();
            public override Guid GUID => throw new NotImplementedException();
            public override Module Module => throw new NotImplementedException();
            public override Type UnderlyingSystemType => throw new NotImplementedException();
            protected override TypeAttributes GetAttributeFlagsImpl() => throw new NotImplementedException();
            protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[] types, ParameterModifier[]? modifiers) => throw new NotImplementedException();
            public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => throw new NotImplementedException();
            public override Type? GetElementType() => throw new NotImplementedException();
            public override EventInfo? GetEvent(string name, BindingFlags bindingAttr) => throw new NotImplementedException();
            public override EventInfo[] GetEvents(BindingFlags bindingAttr) => throw new NotImplementedException();
            public override FieldInfo? GetField(string name, BindingFlags bindingAttr) => throw new NotImplementedException();
            public override FieldInfo[] GetFields(BindingFlags bindingAttr) => throw new NotImplementedException();
            public override Type? GetInterface(string name, bool ignoreCase) => throw new NotImplementedException();
            public override Type[] GetInterfaces() => throw new NotImplementedException();
            public override MemberInfo[] GetMembers(BindingFlags bindingAttr) => throw new NotImplementedException();
            protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[]? types, ParameterModifier[]? modifiers) => throw new NotImplementedException();
            public override MethodInfo[] GetMethods(BindingFlags bindingAttr) => throw new NotImplementedException();
            public override Type? GetNestedType(string name, BindingFlags bindingAttr) => throw new NotImplementedException();
            public override Type[] GetNestedTypes(BindingFlags bindingAttr) => throw new NotImplementedException();
            public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) => throw new NotImplementedException();
            protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder? binder, Type? returnType, Type[]? types, ParameterModifier[]? modifiers) => throw new NotImplementedException();
            protected override bool HasElementTypeImpl() => throw new NotImplementedException();
            public override object? InvokeMember(string name, BindingFlags invokeAttr, Binder? binder, object? target, object?[]? args, ParameterModifier[]? modifiers, System.Globalization.CultureInfo? culture, string[]? namedParameters) => throw new NotImplementedException();
            protected override bool IsArrayImpl() => throw new NotImplementedException();
            protected override bool IsByRefImpl() => throw new NotImplementedException();
            protected override bool IsCOMObjectImpl() => throw new NotImplementedException();
            protected override bool IsPointerImpl() => throw new NotImplementedException();
            protected override bool IsPrimitiveImpl() => throw new NotImplementedException();
            public override object[] GetCustomAttributes(bool inherit) => throw new NotImplementedException();
            public override object[] GetCustomAttributes(Type attributeType, bool inherit) => throw new NotImplementedException();
            public override bool IsDefined(Type attributeType, bool inherit) => throw new NotImplementedException();
        }
    }
}

// Test types in separate namespaces
namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceA
{
    internal class Type1 { }
    internal class Type2 { }
    internal class Type3 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceB.Sub1
{
    internal class Type1 { }
    internal class Type2 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceB.Sub2
{
    internal class Type3 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceC
{
    internal class Container
    {
        internal class NestedType { }
    }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceD
{
    internal class Type1 { }
    internal class Type2 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceE
{
    internal class Type3 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceF
{
    internal class Type1 { }
    internal class Type2 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceG
{
    internal class Type3 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceH
{
    internal class Type1 { }
    internal class Type2 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceI
{
    internal class Type1 { }
    internal class Type2 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceJ.L1.L2.L3
{
    internal class Type1 { }
    internal class Type2 { }
    internal class Type3 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceK
{
    internal class Type1 { }
    internal class Type2 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceL.Match
{
    internal class Type1 { }
    internal class Type2 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceL.Different
{
    internal class Type3 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceM
{
    internal class Type1 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceN
{
    internal class Type1 { }
    internal class Type2 { }
    internal class Type3 { }
    internal class Type4 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceO
{
    internal class Type5 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceP
{
    internal class Type1 { }
    internal class Type2 { }
    internal class Type3 { }
}

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies.TestNamespaceQ
{
    internal class Type4 { }
    internal class Type5 { }
}

internal class NoNamespaceType { }
