using Game.Game.Common.Enum;
using Godot;

namespace Game.Game.Common.Service.FileHandler;

public interface IFileSystemService
{
    bool FileExists(string path);
    bool DirExists(string path);
    FileError MakeDirRecursive(string path);
    FileError Remove(string path);
    IFileAccess? Open(string path, FileAccess.ModeFlags mode);
    IFileAccess? OpenEncryptedWithPass(string path, FileAccess.ModeFlags mode, string password);
}
