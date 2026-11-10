using Game.Game.Settings.Domain;
using Godot;

namespace Game.Game.Settings.UI;

/// <summary>
/// Switches the game language. Texts are written in English in code; Godot looks them up in
/// <c>locale/&lt;code&gt;.po</c> (registered in <c>project.godot</c>) and re-translates the
/// static texts of every control on the switch. An untranslated text stays English.
/// </summary>
public static class GameLanguage
{
    public static void Apply(Language language)
    {
        TranslationServer.SetLocale(language.Code);
    }
}
