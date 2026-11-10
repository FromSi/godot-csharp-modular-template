using Game.Game.Settings.Service;
using System.Globalization;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Game.Tests.Locale;

// Checks the translations: every language the settings offer but English (the language of the code)
// has locale/<code>.po, named in its header and registered in project.godot; every text there is
// translated once and keeps its {0}-style placeholders; every text the game data shows as is is there
// — so a missing or broken line fails here instead of showing English (or crashing a Format) in game.
[TestFixture]
public class TranslationTests
{
    private const string SourceLanguage = "en";

    private static readonly Regex Entry = new("^msgid \"(.*)\"\\r?\\nmsgstr \"(.*)\"$", RegexOptions.Multiline);
    private static readonly Regex MsgId = new("^msgid ", RegexOptions.Multiline);
    private static readonly Regex Escape = new(@"\\(.)");
    private static readonly Regex Placeholder = new(@"\{(\d+)(:[^}]*)?\}");
    private static readonly Regex Translations = new(
        "^locale/translations=PackedStringArray\\((.*)\\)$",
        RegexOptions.Multiline
    );
    private static readonly Regex TranslationPath = new("\"res://locale/([^\"]+)\\.po\"");
    private static readonly Regex GameTitle = new("^config/name=\"(.*)\"$", RegexOptions.Multiline);

    private static IEnumerable<string> TranslatedLanguages()
    {
        return SettingsService.Languages.Select(language => language.Code).Where(code => code != SourceLanguage);
    }

    [Test]
    public void EveryOfferedLanguageButEnglishHasARegisteredTranslation()
    {
        var files = Directory.GetFiles(Path.Combine(Root(), "locale"), "*.po").Select(Path.GetFileNameWithoutExtension);
        var registered = TranslationPath.Matches(Translations.Match(Project()).Groups[1].Value)
            .Select(match => match.Groups[1].Value);

        Assert.That(SettingsService.Languages[0].Code, Is.EqualTo(SourceLanguage));
        Assert.That(files, Is.EquivalentTo(TranslatedLanguages()));
        Assert.That(registered, Is.EquivalentTo(TranslatedLanguages()));

        foreach (var code in TranslatedLanguages())
        {
            Assert.That(Read(code), Does.Contain($"\"Language: {code}\\n\""), code);
        }
    }

    [TestCaseSource(nameof(TranslatedLanguages))]
    public void EveryTextIsTranslatedOnce(string code)
    {
        var po = Read(code);
        var entries = Entries(po);

        Assert.That(MsgId.Matches(po), Has.Count.EqualTo(entries.Count + 1), "one msgid and one msgstr line each");
        Assert.That(entries, Is.Not.Empty);
        Assert.That(entries.Where(entry => entry.Text == "").Select(entry => entry.Id), Is.Empty);
        Assert.That(entries.Select(entry => entry.Id), Is.Unique);
    }

    // A text added to one translation and forgotten in another would show English there.
    [Test]
    public void EveryTranslationHasTheSameTexts()
    {
        var first = TranslatedLanguages().First();
        var texts = Entries(Read(first)).Select(entry => entry.Id);

        foreach (var code in TranslatedLanguages().Skip(1))
        {
            Assert.That(Entries(Read(code)).Select(entry => entry.Id), Is.EquivalentTo(texts), $"{code} vs {first}");
        }
    }

    [TestCaseSource(nameof(TranslatedLanguages))]
    public void EveryTranslationKeepsThePlaceholders(string code)
    {
        foreach (var (id, text) in Entries(Read(code)))
        {
            var expected = Placeholder.Matches(id).Select(match => match.Value).Order().ToList();
            var actual = Placeholder.Matches(text).Select(match => match.Value).Order().ToList();

            Assert.That(actual, Is.EqualTo(expected), id);

            if (expected.Count > 0)
            {
                var values = new object[Placeholder.Matches(id).Max(match => int.Parse(match.Groups[1].Value)) + 1];

                Assert.DoesNotThrow(() => string.Format(CultureInfo.InvariantCulture, text, values), id);
            }
        }
    }

    // Texts the game shows as they are (the game's title, also the window title; an enum's name, a
    // seeded item's name) — add new ones here. A language's name is shown in that language, so it must
    // have no translation.
    [TestCaseSource(nameof(TranslatedLanguages))]
    public void EveryDataTextIsTranslated(string code)
    {
        var ids = Entries(Read(code)).Select(entry => entry.Id).ToHashSet();
        var texts = SettingsService.DisplayModes.Select(mode => mode.ToString())
            .Append(GameTitle.Match(Project()).Groups[1].Value);

        Assert.That(texts.Distinct().Where(text => !ids.Contains(text)), Is.Empty);
        Assert.That(SettingsService.Languages.Select(language => language.Name).Where(ids.Contains), Is.Empty);
    }

    // The project folder (with project.godot): the tests run from its tests/…/bin.
    private static string Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);

        while (!File.Exists(Path.Combine(directory.FullName, "project.godot")))
        {
            directory = directory.Parent!;
        }

        return directory.FullName;
    }

    private static string Read(string code)
    {
        return File.ReadAllText(Path.Combine(Root(), "locale", $"{code}.po"));
    }

    private static string Project()
    {
        return File.ReadAllText(Path.Combine(Root(), "project.godot"));
    }

    // Every msgid / msgstr pair but the header (its msgid is empty), unescaped (\" \n \\).
    private static List<(string Id, string Text)> Entries(string po)
    {
        return Entry.Matches(po)
            .Select(match => (Unescape(match.Groups[1].Value), Unescape(match.Groups[2].Value)))
            .Where(entry => entry.Item1 != "")
            .ToList();
    }

    private static string Unescape(string text)
    {
        return Escape.Replace(text, match => match.Groups[1].Value switch
        {
            "n" => "\n",
            "t" => "\t",
            var character => character,
        });
    }
}
