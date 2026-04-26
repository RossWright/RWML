namespace RossWright.MetalChain.Tests;

/// <summary>
/// Serializes test classes that assert against the static BasicCommand.Handler.LastValue,
/// preventing parallel execution from causing race conditions on that shared field.
/// </summary>
[CollectionDefinition(Name)]
public class BasicCommandHandlerCollection
{
    public const string Name = "BasicCommandHandler";
}
