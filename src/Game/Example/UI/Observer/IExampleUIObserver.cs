namespace Game.Game.Example.UI.Observer;

/// <summary>
/// UI → Level notifications: the screen raises intents, and the level (see
/// <see cref="Level.ExampleLevel"/>) decides what to do with them. Keeps the UI free of
/// navigation logic — it just reports what the user asked for.
/// </summary>
public interface IExampleUIObserver
{
    void OnMenuRequested();
}
