using Game.Game.Common.Domain.Observer;
using Game.Game.Example.Domain.Observer;
using System;

namespace Game.Game.Example.Domain;

/// <summary>
/// Serializable state of the example module. It is observable: change the text
/// through <see cref="ChangeText"/> and listeners (see <see cref="INoteStateObserver"/>)
/// are notified. The public setters exist only for JSON (de)serialization — mutate via
/// <see cref="ChangeText"/> so observers fire. <see cref="ChangedAt"/> is null until the
/// first change; the time comes from the service's clock, not from the state itself.
/// </summary>
public class NoteState : ObservableState<INoteStateObserver>
{
    public string Text { get; set; } = "";
    public DateTime? ChangedAt { get; set; }

    public void ChangeText(string text, DateTime changedAt)
    {
        Text = text;
        ChangedAt = changedAt;
        Notify(observer => observer.OnTextChanged(text, changedAt));
    }
}
