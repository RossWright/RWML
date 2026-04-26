using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Tests.Infrastructure;
using RossWright.MetalCommand.Tests.Models;

namespace RossWright.MetalCommand.Tests;

/// <summary>
/// Direct unit tests for <see cref="ArgBinder.TryBind"/>. All tests call the internal method
/// directly — enabled by the InternalsVisibleTo attribute on RossWright.MetalCommand.
/// </summary>
public class ArgBinderTests
{
    // Shared empty context and no warning colour — overridden per test when needed.
    private static readonly Dictionary<string, string> EmptyContext = [];
    private static readonly ConsoleColor? NoColor = null;

    private static readonly IServiceProvider EmptyServiceProvider =
        new ServiceCollection().BuildServiceProvider();

    private static bool Bind<TCommand>(
        TCommand command,
        string[] rawArgs,
        IConsole console,
        out IReadOnlyDictionary<string, object?> bound,
        IDictionary<string, string>? context = null)
        where TCommand : ICommand
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(TCommand));
        return ArgBinder.TryBind(command, descriptor, rawArgs, context ?? EmptyContext, console, NoColor, EmptyServiceProvider, out bound);
    }

    // -------------------------------------------------------------------------
    // Positional binding
    // -------------------------------------------------------------------------

    [Fact]
    public void Positional_SingleArg_BoundCorrectly()
    {
        var command = new SingleStringCommand();
        var console = new TestConsole();

        var result = Bind(command, ["alice"], console, out _);

        result.ShouldBeTrue();
        command.Name.ShouldBe("alice");
    }

    [Fact]
    public void Positional_TwoArgs_BoundInDeclarationOrder()
    {
        var command = new TwoPositionalCommand();
        var console = new TestConsole();

        var result = Bind(command, ["one", "two"], console, out _);

        result.ShouldBeTrue();
        command.First.ShouldBe("one");
        command.Second.ShouldBe("two");
    }

    // -------------------------------------------------------------------------
    // Named binding
    // -------------------------------------------------------------------------

    [Fact]
    public void Named_BindsByPropertyName()
    {
        var command = new AllowNamedCommand();
        var console = new TestConsole();

        var result = Bind(command, ["--Target", "db"], console, out _);

        result.ShouldBeTrue();
        command.Target.ShouldBe("db");
    }

    [Fact]
    public void Named_HyphenNormalised()
    {
        var command = new HyphenNamedCommand();
        var console = new TestConsole();

        // "--my-target" normalises to "mytarget" which matches property "MyTarget" (normalised)
        var result = Bind(command, ["--my-target", "db"], console, out _);

        result.ShouldBeTrue();
        command.MyTarget.ShouldBe("db");
    }

    [Fact]
    public void Named_TakesPrecedenceOverPositional()
    {
        var command = new AllowNamedCommand();
        var console = new TestConsole();

        // Named token should fill Target; no positional tokens remain
        var result = Bind(command, ["--Target", "named-value"], console, out _);

        result.ShouldBeTrue();
        command.Target.ShouldBe("named-value");
    }

    // -------------------------------------------------------------------------
    // Boolean flag
    // -------------------------------------------------------------------------

    [Fact]
    public void BoolFlag_BareFlag_SetsTrue()
    {
        var command = new BoolFlagCommand();
        var console = new TestConsole();

        var result = Bind(command, ["--Verbose"], console, out _);

        result.ShouldBeTrue();
        command.Verbose.ShouldBeTrue();
    }

    [Fact]
    public void BoolFlag_ExplicitFalse()
    {
        var command = new BoolFlagCommand();
        var console = new TestConsole();

        var result = Bind(command, ["--Verbose", "false"], console, out _);

        result.ShouldBeTrue();
        command.Verbose.ShouldBeFalse();
    }

    // -------------------------------------------------------------------------
    // AllowNamed = false
    // -------------------------------------------------------------------------

    [Fact]
    public void AllowNamed_False_DoesNotConsumeNamedToken()
    {
        var command = new NoNamedAllowedCommand();
        var console = new TestConsole();

        // "--foo" is a named token but AllowNamed = false on Foo — "bar" has no arg to bind to
        var result = Bind(command, ["--foo", "bar"], console, out _);

        // Binding succeeds (no required args), but Foo is not set from the named token
        result.ShouldBeTrue();
        command.Foo.ShouldBeNull();
    }

    // -------------------------------------------------------------------------
    // Context fallback
    // -------------------------------------------------------------------------

    [Fact]
    public void ContextFallback_UsedWhenArgOmitted()
    {
        var command = new ContextKeyCommand();
        var console = new TestConsole();
        var context = new Dictionary<string, string> { ["env"] = "dev" };

        var result = Bind(command, [], console, out _, context: context);

        result.ShouldBeTrue();
        command.Value.ShouldBe("dev");
        console.Lines.ShouldContain(l => l.Contains("dev"));
    }

    [Fact]
    public void ContextFallback_NotUsedWhenArgSupplied()
    {
        var command = new ContextKeyCommand();
        var console = new TestConsole();
        var context = new Dictionary<string, string> { ["env"] = "dev" };

        var result = Bind(command, ["prod"], console, out _, context: context);

        result.ShouldBeTrue();
        command.Value.ShouldBe("prod");
    }

    // -------------------------------------------------------------------------
    // Default value fallback
    // -------------------------------------------------------------------------

    [Fact]
    public void DefaultValue_UsedWhenArgOmitted()
    {
        var command = new DefaultValueCommand();
        var console = new TestConsole();

        var result = Bind(command, [], console, out _);

        result.ShouldBeTrue();
        command.Value.ShouldBe("hello");
    }

    // -------------------------------------------------------------------------
    // IsRequired validation
    // -------------------------------------------------------------------------

    [Fact]
    public void IsRequired_MissingArg_ReturnsFalseAndWritesError()
    {
        var command = new RequiredArgCommand();
        var console = new TestConsole();

        var result = Bind(command, [], console, out _);

        result.ShouldBeFalse();
        console.ErrorLines.ShouldNotBeEmpty();
    }

    [Fact]
    public void IsRequired_WithContextFallback_Succeeds()
    {
        var command = new RequiredWithContextCommand();
        var console = new TestConsole();
        var context = new Dictionary<string, string> { ["env"] = "staging" };

        var result = Bind(command, [], console, out _, context: context);

        result.ShouldBeTrue();
        command.Value.ShouldBe("staging");
    }

    // -------------------------------------------------------------------------
    // ValidValues
    // -------------------------------------------------------------------------

    [Fact]
    public void ValidValues_AcceptsValidValue_CaseInsensitive()
    {
        var command = new ValidValuesCommand();
        var console = new TestConsole();

        var result = Bind(command, ["DEV"], console, out _);

        result.ShouldBeTrue();
        command.Env.ShouldBe("DEV");
    }

    [Fact]
    public void ValidValues_RejectsInvalidValue_ReturnsFalse()
    {
        var command = new ValidValuesCommand();
        var console = new TestConsole();

        var result = Bind(command, ["staging"], console, out _);

        result.ShouldBeFalse();
        console.ErrorLines.ShouldNotBeEmpty();
    }

    // -------------------------------------------------------------------------
    // Type conversion
    // -------------------------------------------------------------------------

    [Fact]
    public void TypeConversion_Int()
    {
        var command = new TypedArgsCommand();
        var console = new TestConsole();
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(TypedArgsCommand));
        // Bind only the IntVal arg — supply positional tokens for all preceding typed args
        // Easiest: use a dedicated single-arg command instead; re-use SingleStringCommand approach
        // but we have TypedArgsCommand; supply tokens for all 7 args.
        var guid = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        var result = ArgBinder.TryBind(command, descriptor,
            ["text", "42", "3.14", "true", guid.ToString(), date.ToString("o"), "Active"],
            EmptyContext, console, NoColor, EmptyServiceProvider, out _);

        result.ShouldBeTrue();
        command.IntVal.ShouldBe(42);
    }

    [Fact]
    public void TypeConversion_Double()
    {
        var command = new TypedArgsCommand();
        var console = new TestConsole();
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(TypedArgsCommand));
        var guid = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        ArgBinder.TryBind(command, descriptor,
            ["text", "1", "3.14", "false", guid.ToString(), date.ToString("o"), "Active"],
            EmptyContext, console, NoColor, EmptyServiceProvider, out _);

        command.DoubleVal.ShouldBe(3.14);
    }

    [Fact]
    public void TypeConversion_Bool_True()
    {
        var command = new TypedArgsCommand();
        var console = new TestConsole();
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(TypedArgsCommand));
        var guid = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        ArgBinder.TryBind(command, descriptor,
            ["text", "1", "1.0", "true", guid.ToString(), date.ToString("o"), "Active"],
            EmptyContext, console, NoColor, EmptyServiceProvider, out _);

        command.BoolVal.ShouldBeTrue();
    }

    [Fact]
    public void TypeConversion_Bool_False()
    {
        var command = new TypedArgsCommand();
        var console = new TestConsole();
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(TypedArgsCommand));
        var guid = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        ArgBinder.TryBind(command, descriptor,
            ["text", "1", "1.0", "false", guid.ToString(), date.ToString("o"), "Active"],
            EmptyContext, console, NoColor, EmptyServiceProvider, out _);

        command.BoolVal.ShouldBeFalse();
    }

    [Fact]
    public void TypeConversion_Guid()
    {
        var command = new TypedArgsCommand();
        var console = new TestConsole();
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(TypedArgsCommand));
        var guid = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        ArgBinder.TryBind(command, descriptor,
            ["text", "1", "1.0", "false", guid.ToString(), date.ToString("o"), "Active"],
            EmptyContext, console, NoColor, EmptyServiceProvider, out _);

        command.GuidVal.ShouldBe(guid);
    }

    [Fact]
    public void TypeConversion_DateTime()
    {
        var command = new TypedArgsCommand();
        var console = new TestConsole();
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(TypedArgsCommand));
        var guid = Guid.NewGuid();
        var date = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);

        ArgBinder.TryBind(command, descriptor,
            ["text", "1", "1.0", "false", guid.ToString(), "2024-06-15", "Active"],
            EmptyContext, console, NoColor, EmptyServiceProvider, out _);

        command.DateTimeVal.Date.ShouldBe(date.Date);
    }

    [Fact]
    public void TypeConversion_Enum()
    {
        var command = new TypedArgsCommand();
        var console = new TestConsole();
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(TypedArgsCommand));
        var guid = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        ArgBinder.TryBind(command, descriptor,
            ["text", "1", "1.0", "false", guid.ToString(), date.ToString("o"), "Active"],
            EmptyContext, console, NoColor, EmptyServiceProvider, out _);

        command.StatusVal.ShouldBe(Status.Active);
    }

    [Fact]
    public void TypeConversion_Enum_CaseInsensitive()
    {
        var command = new TypedArgsCommand();
        var console = new TestConsole();
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(TypedArgsCommand));
        var guid = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        ArgBinder.TryBind(command, descriptor,
            ["text", "1", "1.0", "false", guid.ToString(), date.ToString("o"), "active"],
            EmptyContext, console, NoColor, EmptyServiceProvider, out _);

        command.StatusVal.ShouldBe(Status.Active);
    }

    [Fact]
    public void TypeConversion_InvalidInt_ReturnsFalse()
    {
        var command = new TypedArgsCommand();
        var console = new TestConsole();
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(TypedArgsCommand));

        // IntVal token is invalid
        var result = ArgBinder.TryBind(command, descriptor,
            ["text", "notanumber"],
            EmptyContext, console, NoColor, EmptyServiceProvider, out _);

        result.ShouldBeFalse();
        console.ErrorLines.ShouldNotBeEmpty();
    }

    // -------------------------------------------------------------------------
    // Explicit Order override
    // -------------------------------------------------------------------------

    [Fact]
    public void ExplicitOrder_OverridesDeclarationOrder()
    {
        var command = new ExplicitOrderBinderCommand();
        var console = new TestConsole();

        // First positional token goes to Order=0 (First), second to Order=1 (Second)
        var result = Bind(command, ["token-a", "token-b"], console, out _);

        result.ShouldBeTrue();
        command.First.ShouldBe("token-a");
        command.Second.ShouldBe("token-b");
    }

    // -------------------------------------------------------------------------
    // Excess tokens
    // -------------------------------------------------------------------------

    [Fact]
    public void ExcessTokens_DoNotCauseError()
    {
        var command = new SingleStringCommand();
        var console = new TestConsole();

        var result = Bind(command, ["alice", "extra1", "extra2"], console, out _);

        result.ShouldBeTrue();
        console.ErrorLines.ShouldBeEmpty();
    }

    // -------------------------------------------------------------------------
    // BoundArgs output dictionary
    // -------------------------------------------------------------------------

    [Fact]
    public void BoundArgs_ContainsAllProperties()
    {
        var command = new TwoPositionalCommand();
        var console = new TestConsole();

        Bind(command, ["x", "y"], console, out var bound);

        bound.Keys.ShouldContain("First");
        bound.Keys.ShouldContain("Second");
    }

    // -------------------------------------------------------------------------
    // Default value warning message
    // -------------------------------------------------------------------------

    [Fact]
    public void DefaultValue_PrintsWarning()
    {
        var command = new DefaultValueCommand();
        var console = new TestConsole();

        Bind(command, [], console, out _);

        // A warning line mentioning the default value should be written to the console
        console.Lines.ShouldContain(l => l.Contains("hello"));
    }

    // -------------------------------------------------------------------------
    // Named arg matched by display Name (ArgAttribute.Name)
    // -------------------------------------------------------------------------

    [Fact]
    public void Named_ByDisplayName_BindsToProperty()
    {
        var command = new DisplayNameArgCommand();
        var console = new TestConsole();

        // Property is "Dest" but [Arg(Name = "target")] — token uses display name
        var result = Bind(command, ["--target", "mydb"], console, out _);

        result.ShouldBeTrue();
        command.Dest.ShouldBe("mydb");
    }

    // -------------------------------------------------------------------------
    // BoundArgs dictionary — unbound optional entries
    // -------------------------------------------------------------------------

    [Fact]
    public void BoundArgs_UnboundOptional_ContainsNullEntry()
    {
        var command = new SingleStringCommand();
        var console = new TestConsole();

        // Supply no tokens — optional Name arg gets no value
        Bind(command, [], console, out var bound);

        bound.Keys.ShouldContain("Name");
        bound["Name"].ShouldBeNull();
    }

    // -------------------------------------------------------------------------
    // Type conversion — invalid enum value
    // -------------------------------------------------------------------------

    [Fact]
    public void TypeConversion_Enum_InvalidValue_ReturnsFalseAndWritesError()
    {
        var command = new TypedArgsCommand();
        var console = new TestConsole();
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(TypedArgsCommand));
        var guid = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        var result = ArgBinder.TryBind(command, descriptor,
            ["text", "1", "1.0", "false", guid.ToString(), date.ToString("o"), "Unknown"],
            EmptyContext, console, NoColor, EmptyServiceProvider, out _);

        result.ShouldBeFalse();
        console.ErrorLines.ShouldNotBeEmpty();
    }

    // -------------------------------------------------------------------------
    // [EnvironmentArg] binding — Phase 3
    // -------------------------------------------------------------------------

    private static bool BindWithEnv<TCommand>(
        TCommand command,
        string[] rawArgs,
        IConsole console,
        out IReadOnlyDictionary<string, object?> bound,
        IDictionary<string, string>? context = null)
        where TCommand : ICommand
    {
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(TCommand));
        var services = new ServiceCollection();
        services.AddSingleton<IEnvironmentSource>(new StubEnvironmentSource());
        var sp = services.BuildServiceProvider();
        return ArgBinder.TryBind(command, descriptor, rawArgs, context ?? EmptyContext,
            console, NoColor, sp, out bound);
    }

    [Fact]
    public void EnvironmentArg_ExplicitValue_BindsCorrectly()
    {
        var command = new EnvOnlyCommand();
        var console = new TestConsole();

        var result = BindWithEnv(command, ["test"], console, out _);

        result.ShouldBeTrue();
        command.Environment.ShouldBe("test");
    }

    [Fact]
    public void EnvironmentArg_Omitted_UsesDefault()
    {
        var command = new EnvOnlyCommand();
        var console = new TestConsole();

        var result = BindWithEnv(command, [], console, out _);

        result.ShouldBeTrue();
        command.Environment.ShouldBe("local");
        console.Lines.ShouldContain(l => l.Contains("local"));
    }

    [Fact]
    public void EnvironmentArg_CaseInsensitive_BindsCanonicalCasing()
    {
        var command = new EnvOnlyCommand();
        var console = new TestConsole();

        // "TEST" should resolve to canonical "test"
        var result = BindWithEnv(command, ["TEST"], console, out _);

        result.ShouldBeTrue();
        command.Environment.ShouldBe("test");
    }

    [Fact]
    public void EnvironmentArg_InvalidValue_ReturnsFalseAndWritesError()
    {
        var command = new EnvOnlyCommand();
        var console = new TestConsole();

        var result = BindWithEnv(command, ["staging"], console, out _);

        result.ShouldBeFalse();
        console.ErrorLines.ShouldNotBeEmpty();
    }

    [Fact]
    public void EnvironmentArg_WithPrecedingRegularArg_RegularArgNotConsumedByPhase3()
    {
        // This is the exact regression: "name" is the regular arg, "test" is the env arg.
        // Before the fix, Phase 2 consumed "test" for the env descriptor entry in descriptor.Args,
        // leaving Phase 3 with nothing and falling back to the default "local".
        var command = new EnvArgCommand();
        var console = new TestConsole();

        var result = BindWithEnv(command, ["myname", "test"], console, out _);

        result.ShouldBeTrue();
        command.Name.ShouldBe("myname");
        command.Environment.ShouldBe("test");
    }

    [Fact]
    public void EnvironmentArg_NamedToken_BindsCorrectly()
    {
        var command = new EnvArgCommand();
        var console = new TestConsole();

        var result = BindWithEnv(command, ["myname", "--environment", "test"], console, out _);

        result.ShouldBeTrue();
        command.Name.ShouldBe("myname");
        command.Environment.ShouldBe("test");
    }

    [Fact]
    public void EnvironmentArg_NoSourceRegistered_ReturnsFalseAndWritesError()
    {
        var command = new EnvOnlyCommand();
        var console = new TestConsole();
        var descriptor = CommandDescriptorFactory.FromAttributes(typeof(EnvOnlyCommand));

        // Use an empty service provider — no IEnvironmentSource registered
        var result = ArgBinder.TryBind(command, descriptor, ["test"],
            EmptyContext, console, NoColor, EmptyServiceProvider, out _);

        result.ShouldBeFalse();
        console.ErrorLines.ShouldNotBeEmpty();
    }

    // -------------------------------------------------------------------------
    // Safety guard for invalid descriptor args
    // -------------------------------------------------------------------------

    [Fact]
    public void InvalidDescriptorArg_NullPropertyName_SkippedSafely()
    {
        var command = new SingleStringCommand();
        var console = new TestConsole();
        
        // Create a descriptor with an arg that has null PropertyName
        var invalidArg = new ArgumentDescriptor(
            name: "invalid",
            isRequired: false,
            defaultValue: null,
            contextKey: null,
            validValues: null,
            helpDetail: null,
            allowNamed: false,
            propertyName: null!,
            propertyType: typeof(string))
        {
            PropertyName = null
        };

        var descriptor = new CommandDescriptor
        {
            Name = "test",
            Invocations = ["test"],
            Args = [invalidArg],
            HelpBrief = "Test command"
        };

        var result = ArgBinder.TryBind(command, descriptor, [],
            EmptyContext, console, NoColor, EmptyServiceProvider, out var bound);

        result.ShouldBeTrue();
        bound.ShouldNotBeNull();
    }

    [Fact]
    public void InvalidDescriptorArg_NullPropertyType_SkippedSafely()
    {
        var command = new SingleStringCommand();
        var console = new TestConsole();
        
        // Create a descriptor with an arg that has null PropertyType
        var invalidArg = new ArgumentDescriptor(
            name: "invalid",
            isRequired: false,
            defaultValue: null,
            contextKey: null,
            validValues: null,
            helpDetail: null,
            allowNamed: false,
            propertyName: "Invalid",
            propertyType: null!)
        {
            PropertyType = null
        };

        var descriptor = new CommandDescriptor
        {
            Name = "test",
            Invocations = ["test"],
            Args = [invalidArg],
            HelpBrief = "Test command"
        };

        var result = ArgBinder.TryBind(command, descriptor, [],
            EmptyContext, console, NoColor, EmptyServiceProvider, out var bound);

        result.ShouldBeTrue();
        bound.ShouldNotBeNull();
    }
}
