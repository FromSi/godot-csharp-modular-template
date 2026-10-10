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

    // False when the save could not be written; the failure is logged and shown to the player.
    bool SaveGame();

    // The last save could not be written (the menu and the game screen warn about it).
    bool IsLastSaveFailed { get; }

    // Saves, leaves the game screen (it's dropped and rebuilt on the next entry) and opens the menu;
    // false (and stays in the game) when the save could not be written.
    bool ReturnToMenu();

    // Leaves for the menu without saving — after the player agreed to lose the unsaved progress.
    void LeaveToMenu();
}
