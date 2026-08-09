using System.Collections.Generic;
using Game.Game.Common.Enum;

namespace Game.Game.Common.Service.FileHandler;

public interface IJsonStateFileHandlerService
{
    FileError Store(List<object?> data, string path, bool createDir = true);
    List<object?> Load(string path);
}
