using System.Globalization;
using System.Linq;
using Godot;

namespace Game.Game.Common.UI;

/// <summary>
/// Translates player-facing texts written in English (msgids of <c>locale/*.po</c>). A plain
/// <c>Text = "..."</c> on a control needs none of this — Godot translates it itself; use these
/// for texts with values and outside a control. String values are translated too (item names);
/// others (numbers, dates) have no msgid and stay as they are.
/// </summary>
public static class Localize
{
    public static string Text(string text)
    {
        return TranslationServer.Translate(text);
    }

    public static string Format(string template, params object[] values)
    {
        var translated = values.Select(value => value is string text ? Value(text) : value).ToArray();

        return string.Format(CultureInfo.InvariantCulture, Text(template), translated);
    }

    // A list "A, B" is translated item by item (unless the whole text has a translation itself).
    private static string Value(string text)
    {
        var whole = Text(text);

        if (whole != text || !text.Contains(", "))
        {
            return whole;
        }

        return string.Join(", ", text.Split(", ").Select(Text));
    }
}
