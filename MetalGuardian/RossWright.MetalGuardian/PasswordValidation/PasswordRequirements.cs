namespace RossWright.MetalGuardian;

/// <summary>
/// Configures the rules used by <see cref="IPasswordValidator"/> to evaluate passwords.
/// All properties have sensible defaults; override only what is needed.
/// </summary>
public class PasswordRequirements
{
    /// <summary>Minimum number of characters required. Defaults to <c>8</c>.</summary>
    public int MinimumLength { get; set; } = 8;

    /// <summary>Maximum number of characters allowed. Defaults to <see cref="int.MaxValue"/> (no limit).</summary>
    public int MaximumLength { get; set; } = int.MaxValue;

    /// <summary>When <c>true</c>, at least one uppercase letter is required. Defaults to <c>true</c>.</summary>
    public bool RequireUpperCase { get; set; } = true;

    /// <summary>When <c>true</c>, at least one lowercase letter is required. Defaults to <c>true</c>.</summary>
    public bool RequireLowerCase { get; set; } = true;

    /// <summary>When <c>true</c>, at least one numeric digit is required. Defaults to <c>true</c>.</summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, at least one symbol from <see cref="AllowedSymbols"/> is required. Defaults to <c>true</c>.
    /// </summary>
    public bool RequireSymbol { get; set; } = true;

    /// <summary>
    /// The set of characters that are accepted as symbols. A symbol character not in this list will
    /// cause validation to fail even when <see cref="RequireSymbol"/> is <c>false</c>.
    /// Defaults to common punctuation and special characters.
    /// </summary>
    public char[] AllowedSymbols { get; set; } = ['\'','`','~','!','@','#','$','%','^','&','*','(',')','_','+','-','=','[',']','\\','{','}','|',';',':','\'','"',',','.','/','<','>','?'];
}
