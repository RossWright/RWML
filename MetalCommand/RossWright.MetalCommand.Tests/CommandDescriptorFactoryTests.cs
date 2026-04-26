using RossWright.MetalCommand.Tests.Models;

namespace RossWright.MetalCommand.Tests;

public class CommandDescriptorFactoryTests
{
    // -------------------------------------------------------------------------
    // No-arg command
    // -------------------------------------------------------------------------

    [Fact]
    public void NoArgCommand_ProducesEmptyArgsArray()
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(NoArgCommand));

        descriptor.Args.ShouldBeEmpty();
    }

    // -------------------------------------------------------------------------
    // Invocations
    // -------------------------------------------------------------------------

    [Fact]
    public void DefaultInvocation_IsLowercasedName()
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(NoArgCommand));

        descriptor.Invocations.ShouldBe(["noarg"]);
    }

    [Fact]
    public void ExplicitInvocations_ArePreserved()
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(ExplicitInvocationsCommand));

        // First invocation is always the name lowercased when no explicit list is given;
        // here explicit invocations "foo" and "f" are provided.
        descriptor.Invocations.ShouldBe(["foo", "f"]);
    }

    // -------------------------------------------------------------------------
    // HelpBrief / HelpDetail propagation
    // -------------------------------------------------------------------------

    [Fact]
    public void HelpBrief_PropagatedFromAttribute()
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(NoArgCommand));

        descriptor.HelpBrief.ShouldBe("No args brief");
    }

    [Fact]
    public void HelpDetail_PropagatedFromAttribute()
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(NoArgCommand));

        descriptor.HelpDetail.ShouldBe("No args detail");
    }

    // -------------------------------------------------------------------------
    // Arg ordering
    // -------------------------------------------------------------------------

    [Fact]
    public void Args_OrderedByDeclarationOrder_WhenNoExplicitOrder()
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(DeclarationOrderCommand));

        var names = descriptor.Args!.Select(a => a.Name).ToArray();
        names.ShouldBe(["Alpha", "Beta", "Gamma"]);
    }

    [Fact]
    public void Args_OrderedByExplicitOrder_WhenOrderSet()
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(ExplicitOrderCommand));

        var names = descriptor.Args!.Select(a => a.Name).ToArray();
        names.ShouldBe(["First", "Second", "Third"]);
    }

    // -------------------------------------------------------------------------
    // ArgAttribute property propagation
    // -------------------------------------------------------------------------

    [Fact]
    public void ArgName_DefaultsToPropertyName_WhenNullInAttribute()
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(PropagationCommand));

        var arg = descriptor.Args!.Single(a => a.Name == "MyProp");
        arg.Name.ShouldBe("MyProp");
    }

    [Fact]
    public void ArgName_UsesAttributeName_WhenSet()
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(PropagationCommand));

        descriptor.Args!.ShouldContain(a => a.Name == "custom");
    }

    [Fact]
    public void IsRequired_PropagatedCorrectly()
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(PropagationCommand));

        var required = descriptor.Args!.Single(a => a.Name == "RequiredProp");
        required.IsRequired.ShouldBeTrue();

        var notRequired = descriptor.Args!.Single(a => a.Name == "MyProp");
        notRequired.IsRequired.ShouldBeFalse();
    }

    [Fact]
    public void DefaultValue_PropagatedCorrectly()
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(PropagationCommand));

        var arg = descriptor.Args!.Single(a => a.Name == "DefaultProp");
        arg.DefaultValue.ShouldBe("default-val");
    }

    [Fact]
    public void ValidValues_PropagatedCorrectly()
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(PropagationCommand));

        var arg = descriptor.Args!.Single(a => a.Name == "ValidValuesProp");
        arg.ValidValues.ShouldBe(["a", "b"]);
    }

    [Fact]
    public void ContextKey_PropagatedCorrectly()
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(PropagationCommand));

        var arg = descriptor.Args!.Single(a => a.Name == "ContextProp");
        arg.UseContextKeyForDefault.ShouldBe("ctx-key");
    }

    // -------------------------------------------------------------------------
    // No [Command] attribute
    // -------------------------------------------------------------------------

    [Fact]
    public void NoCommandAttribute_Throws()
    {
        Should.Throw<InvalidOperationException>(() =>
            CommandDescriptorFactory.FromAttributes(typeof(NoAttributeCommand)));
    }

    // -------------------------------------------------------------------------
    // Category propagation
    // -------------------------------------------------------------------------

    [Fact]
    public void Category_PropagatedToDescriptor()
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(CategorisedCommand));

        descriptor.Category.ShouldBe("Tools");
    }

    // -------------------------------------------------------------------------
    // [EnvironmentArg] descriptors appended after [Arg] descriptors
    // -------------------------------------------------------------------------

    [Fact]
    public void EnvironmentArgDescriptors_AppendedAfterRegularArgDescriptors()
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(MixedArgCommand));

        descriptor.Args.ShouldNotBeNull();
        descriptor.Args!.Length.ShouldBe(2);

        // Regular [Arg] comes first
        descriptor.Args[0].PropertyName.ShouldBe("Name");

        // [EnvironmentArg] descriptor comes second, with AllowNamed=true and lowercase name
        descriptor.Args[1].PropertyName.ShouldBe("Environment");
        descriptor.Args[1].AllowNamed.ShouldBeTrue();
        descriptor.Args[1].Name.ShouldBe("environment");
    }

    // -------------------------------------------------------------------------
    // Descriptor caching
    // -------------------------------------------------------------------------

    [Fact]
    public void Cache_ReturnsSameInstance_ForSameType()
    {
        var first = CommandDescriptorFactory.FromAttributes(typeof(NoArgCommand));
        var second = CommandDescriptorFactory.FromAttributes(typeof(NoArgCommand));

        ReferenceEquals(first, second).ShouldBeTrue();
    }
}
