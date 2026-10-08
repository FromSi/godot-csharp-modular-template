using Game.Game.Common.Service;

namespace Game.Game.MainMenu.UI.Factory;

/// <summary>
/// Builds <see cref="MainMenuUI"/> with its dependencies injected.
/// </summary>
public class MainMenuUIFactory
{
    private readonly SaveService _saveService;

    public MainMenuUIFactory(SaveService saveService)
    {
        _saveService = saveService;
    }

    public MainMenuUI Create()
    {
        return new MainMenuUI(_saveService);
    }
}
