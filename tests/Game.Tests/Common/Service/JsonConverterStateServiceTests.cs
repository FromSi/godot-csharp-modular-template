using Game.Game.Common.Domain;
using Game.Game.Common.Service.JsonConverter;
using Game.Game.Example.Domain;
using Game.Game.Settings.Domain;
using Game.Game.Settings.Enum;
using NUnit.Framework;

namespace Game.Tests.Common.Service;

// Round-trips every saved state of the game through real JSON, so a type that doesn't
// (de)serialize (dictionary keys, nullables, lists with defaults, private setters) fails here,
// not in a player's save file. Keep the type list in sync with the slots in GameLevel.
[TestFixture]
public class JsonConverterStateServiceTests
{
    private JsonConverterStateService _converter = null!;

    [SetUp]
    public void SetUp()
    {
        _converter = new JsonConverterStateService();

        foreach (var type in new[] { typeof(IdState), typeof(NoteState), typeof(SettingsState) })
        {
            _converter.Register(type);
        }
    }

    [Test]
    public void EveryGameState_SurvivesARoundTrip()
    {
        var note = new NoteState();
        note.ChangeText("4242", new DateTime(2026, 1, 1, 12, 0, 9));

        var json = _converter.Serialize([new IdState { Counter = 9 }, note]);
        var states = _converter.Deserialize(json);

        Assert.That(states, Has.Count.EqualTo(2));
        Assert.That(((IdState)states[0]!).Counter, Is.EqualTo(9));
        Assert.That(((NoteState)states[1]!).Text, Is.EqualTo("4242"));
        Assert.That(((NoteState)states[1]!).ChangedAt, Is.EqualTo(new DateTime(2026, 1, 1, 12, 0, 9)));
    }

    [Test]
    public void Settings_SurviveARoundTrip()
    {
        var settings = new SettingsState
        {
            DisplayMode = DisplayMode.Fullscreen,
            WindowWidth = 1920,
            WindowHeight = 1080,
        };

        var loaded = (SettingsState)_converter.Deserialize(_converter.Serialize([settings]))[0]!;

        Assert.That(loaded.DisplayMode, Is.EqualTo(DisplayMode.Fullscreen));
        Assert.That(loaded.WindowWidth, Is.EqualTo(1920));
        Assert.That(loaded.WindowHeight, Is.EqualTo(1080));
    }

    // A truncated or malformed file reads as "no states", so a load is refused instead of crashing.
    [TestCase("")]
    [TestCase("[{\"_cls\": \"NoteState\", \"_obj\": {\"text\": \"12")]
    [TestCase("{}")]
    [TestCase("[1, 2]")]
    [TestCase("[{\"_obj\": null}]")]
    [TestCase("[{\"_cls\": \"NoteState\", \"_arr\": 5}]")]
    public void BrokenJson_ReadsAsNoStates(string json)
    {
        Assert.That(_converter.Deserialize(json), Is.Empty);
    }

    [Test]
    public void Serialize_SkipsUnregisteredTypes()
    {
        var converter = new JsonConverterStateService();
        converter.Register(typeof(IdState));

        var states = converter.Deserialize(converter.Serialize([new IdState(), new NoteState()]));

        // The slot count no longer matches, so SaveService.Load rejects such a file as a whole.
        Assert.That(states, Has.Count.EqualTo(1));
    }
}
