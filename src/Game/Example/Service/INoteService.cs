using Game.Game.Example.Domain.Observer;

namespace Game.Game.Example.Service;

public interface INoteService
{
    string Get();
    int RandomNumber();
    void SetText(string text);
    void Subscribe(INoteStateObserver observer);
}
