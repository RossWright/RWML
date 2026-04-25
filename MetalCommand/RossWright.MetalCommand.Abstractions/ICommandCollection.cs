namespace RossWright.MetalCommand;

public interface ICommandCollection : IAssemblyScanningOptionsBuilder
{
    ICommandCollection Add<TCOMMAND>(params object[] parameters) where TCOMMAND : class, ILegacyCommand;
    ICommandCollection Add<TCOMMAND>(CommandDescriptor descriptor, params object[] parameters) where TCOMMAND : class, ILegacyCommand;
}
