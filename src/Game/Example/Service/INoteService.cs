using Game.Game.Example.Domain.Observer;
using System;

namespace Game.Game.Example.Service;

public interface INoteService
{
    string Get();
    DateTime? GetChangedAt();
    int RandomNumber();
    void SetText(string text);
    void Subscribe(INoteStateObserver observer);
    void Unsubscribe(INoteStateObserver observer);
}
