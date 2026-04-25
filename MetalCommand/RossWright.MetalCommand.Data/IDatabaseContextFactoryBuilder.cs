namespace RossWright.MetalCommand.Data;

public interface IDatabaseContextFactoryBuilder
{
    IConfiguration Configuration { get; }
    void Add(string environment, Action<DbContextOptionsBuilder> opts, 
        bool isDefault = false, bool isProtected = false);
}

public static class IDatabaseContextFactoryBuilderExtensions
{
    public static void AddDefault(this IDatabaseContextFactoryBuilder builder, 
        string environment, Action<DbContextOptionsBuilder> opts) =>
        builder.Add(environment, opts, isDefault: true, isProtected: false);
    
    public static void AddProtected(this IDatabaseContextFactoryBuilder builder, 
        string environment, Action<DbContextOptionsBuilder> opts) =>
        builder.Add(environment, opts, isDefault: false, isProtected: true);
    
    public static void AddDefaultProtected(this IDatabaseContextFactoryBuilder builder, 
        string environment, Action<DbContextOptionsBuilder> opts) =>
        builder.Add(environment, opts, isDefault: true, isProtected: true);
}
