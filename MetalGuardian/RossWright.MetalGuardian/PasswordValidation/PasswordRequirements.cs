namespace RossWright.MetalGuardian;

public class PasswordRequirements
{
    public int MinimumLength { get; set; } = 8;
    public int MaximumLength { get; set; } = int.MaxValue;
    public bool RequireUpperCase { get; set; } = true;
    public bool RequireLowerCase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSymbol { get; set; } = true;
    public char[] AllowedSymbols { get; set; } = ['\'','`','~','!','@','#','$','%','^','&','*','(',')','_','+','-','=','[',']','\\','{','}','|',';',':','\'','"',',','.','/','<','>','?'];
}
