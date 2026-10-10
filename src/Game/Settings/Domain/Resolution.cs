using System;
using System.Linq;

namespace Game.Game.Settings.Domain;

/// <summary>
/// A window size. The game is laid out for 1280 × 720 and scaled to fit (other aspects get bars).
/// </summary>
public record Resolution(int Width, int Height)
{
    // Aspect names as players know them; a size gets the nearest one (1366 × 768 is "16:9",
    // 3440 × 1440 is "21:9").
    private static readonly (string Name, double Ratio)[] Aspects =
    [
        ("5:4", 5.0 / 4), ("4:3", 4.0 / 3), ("16:10", 16.0 / 10), ("16:9", 16.0 / 9), ("21:9", 21.0 / 9),
        ("32:9", 32.0 / 9),
    ];

    public string Aspect()
    {
        var ratio = (double)Width / Height;

        return Aspects.MinBy(aspect => Math.Abs(aspect.Ratio - ratio)).Name;
    }

    public override string ToString()
    {
        return $"{Width} × {Height} ({Aspect()})";
    }
}
