using Game.Game.Level.Observer;
using Godot;

namespace Game.Game.Level;

public partial class QuitLevel : Control, ILevelObserver
{
    private readonly IGameLevel _gameLevel;

    public QuitLevel(IGameLevel gameLevel)
    {
        _gameLevel = gameLevel;
        ProcessMode = ProcessModeEnum.Always;

        Hide();
    }

    public void OnLevelOppened(Enum.Level newLevel, Enum.Level oldLevel)
    {
        if (newLevel == Enum.Level.Quit || oldLevel == Enum.Level.Quit)
        {
            GetTree().Quit();
        }
    }
}
