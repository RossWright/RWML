namespace RossWright.MetalCommand.Data;

public class DatabaseEnvironment
{
    public string Environment { get; set; } = null!;
    public bool IsProtected { get; set; }
    public Action<DbContextOptionsBuilder> SetOptions { get; set; } = null!;
}
