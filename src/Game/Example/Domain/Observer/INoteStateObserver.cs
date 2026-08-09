namespace Game.Game.Example.Domain.Observer;

/// <summary>
/// Listener for <see cref="Domain.NoteState"/> changes. Implemented by the UI so it
/// can react to the state instead of polling it.
/// </summary>
public interface INoteStateObserver
{
    void OnTextChanged(string text);
}
