namespace Game.Game.MainMenu.UI.Observer;

/// <summary>
/// UI → Level intents raised by <see cref="MainMenuUI"/>; the hosting level starts the game, opens
/// the settings or quits.
/// </summary>
public interface IMainMenuUIObserver
{
    void OnContinuePressed();
    void OnNewGamePressed();
    void OnSettingsPressed();
    void OnQuitPressed();
}
