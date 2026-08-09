namespace Game.Game.Level;

/// <summary>
/// Contract of the composition root. Level screens (see <see cref="Observer.ILevelObserver"/>)
/// talk back to it to switch the active level. Add module factories/services here
/// as your game grows, then expose them the same way the template does.
/// </summary>
public interface IGameLevel
{
    Enum.Level CurrentLevel { get; }
    Enum.Level PreviousLevel { get; }

    void OpenLevel(Enum.Level level);
    void OpenPreviousLevel();
}
