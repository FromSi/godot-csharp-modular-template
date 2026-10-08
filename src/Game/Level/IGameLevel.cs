namespace Game.Game.Level;

/// <summary>
/// Contract of the composition root. Level screens (see <see cref="Observer.ILevelObserver"/>)
/// talk back to it to switch the active level and to enter / leave the game.
/// </summary>
public interface IGameLevel
{
    Enum.Level CurrentLevel { get; }
    Enum.Level PreviousLevel { get; }

    void OpenLevel(Enum.Level level);
    void OpenPreviousLevel();

    // Loads the save and enters the game; false (and stays put) when there is no valid save.
    bool ContinueGame();
    void StartNewGame();
    void SaveGame();

    // Saves, leaves the game screen (it's dropped and rebuilt on the next entry) and opens the menu.
    void ReturnToMenu();
}
