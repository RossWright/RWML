namespace RossWright.MetalCommand;

public interface ICommandExecutor
{
    Task Execute(string invocation, params string[] args);
    Task Execute<TCommand>(params string[] args) where TCommand : ILegacyCommand;
    IDictionary<string, string> Context { get; }
}
