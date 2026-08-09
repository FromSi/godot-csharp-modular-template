using Game.Game.Common.Domain.Observer;
using Game.Game.Example.Domain.Observer;

namespace Game.Game.Example.Domain;

/// <summary>
/// Serializable state of the example module. It is observable: change the text
/// through <see cref="ChangeText"/> and listeners (see <see cref="INoteStateObserver"/>)
/// are notified. The public <see cref="Text"/> setter exists only for JSON
/// (de)serialization — mutate via <see cref="ChangeText"/> so observers fire.
/// </summary>
public class NoteState : ObservableState<INoteStateObserver>
{
    public string Text { get; set; } = "";

    public void ChangeText(string text)
    {
        Text = text;
        Notify(observer => observer.OnTextChanged(text));
    }
}
