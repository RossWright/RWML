using System.Globalization;

namespace RossWright;

public static partial class Tools
{
    /// <summary>
    /// Returns an <c>#RRGGBB</c> color lightened by <paramref name="byPercent"/> via
    /// HSL conversion. Lightness is clamped to 1.0.
    /// </summary>
    /// <param name="hexColor">A 7-character hex color string in <c>#RRGGBB</c> format.</param>
    /// <param name="byPercent">The amount to increase the HSL lightness value. Defaults to 0.1 (10%).</param>
    /// <returns>A lightened <c>#RRGGBB</c> hex color string.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="hexColor"/> is not a valid <c>#RRGGBB</c> string.</exception>
    public static string GetLighterColor(string hexColor, double byPercent = 0.1) =>
        AdjustHsl(hexColor, lDelta: byPercent);

    /// <summary>
    /// Returns an <c>#RRGGBB</c> color darkened by <paramref name="byPercent"/> via
    /// HSL conversion. Lightness is clamped to 0.0.
    /// </summary>
    /// <param name="hexColor">A 7-character hex color string in <c>#RRGGBB</c> format.</param>
    /// <param name="byPercent">The amount to decrease the HSL lightness value. Defaults to 0.1 (10%).</param>
    /// <returns>A darkened <c>#RRGGBB</c> hex color string.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="hexColor"/> is not a valid <c>#RRGGBB</c> string.</exception>
    public static string GetDarkerColor(string hexColor, double byPercent = 0.1) =>
        AdjustHsl(hexColor, lDelta: -byPercent);

    /// <summary>
    /// Returns an <c>#RRGGBB</c> color with its HSL saturation reduced by <paramref name="byPercent"/>.
    /// Saturation is clamped to 0.0.
    /// </summary>
    /// <param name="hexColor">A 7-character hex color string in <c>#RRGGBB</c> format.</param>
    /// <param name="byPercent">The amount to decrease the HSL saturation value. Defaults to 0.1 (10%).</param>
    /// <returns>A desaturated <c>#RRGGBB</c> hex color string.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="hexColor"/> is not a valid <c>#RRGGBB</c> string.</exception>
    public static string GetDesaturatedColor(string hexColor, double byPercent = 0.1) =>
        AdjustHsl(hexColor, sDelta: -byPercent);

    /// <summary>
    /// Returns an <c>#RRGGBB</c> color with its HSL saturation increased by <paramref name="byPercent"/>.
    /// Saturation is clamped to 1.0.
    /// </summary>
    /// <param name="hexColor">A 7-character hex color string in <c>#RRGGBB</c> format.</param>
    /// <param name="byPercent">The amount to increase the HSL saturation value. Defaults to 0.1 (10%).</param>
    /// <returns>A saturated <c>#RRGGBB</c> hex color string.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="hexColor"/> is not a valid <c>#RRGGBB</c> string.</exception>
    public static string GetSaturatedColor(string hexColor, double byPercent = 0.1) =>
        AdjustHsl(hexColor, sDelta: byPercent);

    private static string AdjustHsl(string hexColor, double sDelta = 0.0, double lDelta = 0.0)
    {
        if (string.IsNullOrWhiteSpace(hexColor) || hexColor.Length != 7 || hexColor[0] != '#')
            throw new ArgumentException("Invalid hex color format. Expected #RRGGBB.");

        int r = int.Parse(hexColor.Substring(1, 2), NumberStyles.HexNumber);
        int g = int.Parse(hexColor.Substring(3, 2), NumberStyles.HexNumber);
        int b = int.Parse(hexColor.Substring(5, 2), NumberStyles.HexNumber);

        RgbToHsl(r, g, b, out var h, out var s, out var l);

        s = Math.Min(1.0, Math.Max(0.0, s + sDelta));
        l = Math.Min(1.0, Math.Max(0.0, l + lDelta));

        HslToRgb(h, s, l, out r, out g, out b);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private static void RgbToHsl(int r, int g, int b, out double h, out double s, out double l)
    {
        double rf = r / 255.0;
        double gf = g / 255.0;
        double bf = b / 255.0;

        double max = Math.Max(rf, Math.Max(gf, bf));
        double min = Math.Min(rf, Math.Min(gf, bf));
        double delta = max - min;

        l = (max + min) / 2.0;

        if (delta == 0)
        {
            h = 0;
            s = 0;
        }
        else
        {
            s = (l < 0.5) ? (delta / (max + min)) : (delta / (2.0 - max - min));

            if (rf >= max)
            {
                h = (gf - bf) / delta + (gf < bf ? 6 : 0);
            }
            else if (gf >= max)
            {
                h = (bf - rf) / delta + 2;
            }
            else
            {
                h = (rf - gf) / delta + 4;
            }
            h /= 6.0;
        }
    }

    private static void HslToRgb(double h, double s, double l, out int r, out int g, out int b)
    {
        double rf, gf, bf;

        if (s == 0)
        {
            rf = gf = bf = l;
        }
        else
        {
            double q = (l < 0.5) ? (l * (1 + s)) : (l + s - l * s);
            double p = 2 * l - q;

            rf = HueToRgb(p, q, h + 1.0 / 3.0);
            gf = HueToRgb(p, q, h);
            bf = HueToRgb(p, q, h - 1.0 / 3.0);
        }

        r = (int)(rf * 255);
        g = (int)(gf * 255);
        b = (int)(bf * 255);
    }

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2.0) return q;
        if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
        return p;
    }
}