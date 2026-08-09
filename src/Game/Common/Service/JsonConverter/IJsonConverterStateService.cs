using System;
using System.Collections.Generic;

namespace Game.Game.Common.Service.JsonConverter;

public interface IJsonConverterStateService
{
    void Register(Type type);
    string Serialize(List<object?> collections);
    List<object?> Deserialize(string json);
}
