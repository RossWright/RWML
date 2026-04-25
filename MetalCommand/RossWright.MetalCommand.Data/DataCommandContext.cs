namespace RossWright.MetalCommand.Data;

public class DataCommandContext<DBCTX> where DBCTX : DbContext
{
    public IConsole Console { get; set; } = null!;
    public string Environment { get; set; } = null!;
    public DBCTX DbContext { get; set; } = null!;
}

