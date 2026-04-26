namespace RossWright.MetalCommand.Tests.Infrastructure;

/// <summary>
/// Serializes test classes that change the process working directory or write to the file system,
/// preventing parallel execution from causing interference via Directory.GetCurrentDirectory().
/// </summary>
[CollectionDefinition(Name)]
public class FileSystemCollection
{
    public const string Name = "FileSystem";
}
