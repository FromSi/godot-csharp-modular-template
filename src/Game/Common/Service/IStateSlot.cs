using System;

namespace Game.Game.Common.Service;

/// <summary>
/// One piece of game state that takes part in save / load / new game. Lets
/// <see cref="SaveService"/> and <see cref="NewGameService"/> handle every state the same way
/// without knowing concrete module types.
/// </summary>
public interface IStateSlot
{
    Type StateType { get; }
    object Current();
    void Restore(object state);
    void Reset();
}
