namespace RossWright.MetalCommand;

public static class ConsoleApplicationExtensions
{
    public static ConsoleApplication UseApp(this ConsoleApplication app, Action<ConsoleApplication> useApp)
    {
        useApp(app);
        return app;
    }

    public static ConsoleApplication AddContext(this ConsoleApplication app, string contextName, string value)
    {
        app.Context.Add(contextName, value);
        return app;
    }

    public static ConsoleApplication Build(this IConsoleApplicationBuilder builder) => 
        ((ConsoleApplicationBuilder)builder).Build();
}
