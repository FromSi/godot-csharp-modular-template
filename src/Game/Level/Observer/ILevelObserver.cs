namespace Game.Game.Level.Observer;

public interface ILevelObserver
{
    void OnLevelOppened(Enum.Level newLevel, Enum.Level oldLevel);
}
