using Game.Game.Common.Enum;
using Godot;

namespace Game.Game.Common.Service.FileHandler;

public class GodotFileSystemService : IFileSystemService
{
    public bool FileExists(string path)
    {
        return FileAccess.FileExists(path);
    }

    public bool DirExists(string path)
    {
        return DirAccess.DirExistsAbsolute(path);
    }

    public FileError MakeDirRecursive(string path)
    {
        return MapError(DirAccess.MakeDirRecursiveAbsolute(path));
    }

    public FileError Remove(string path)
    {
        return MapError(DirAccess.RemoveAbsolute(path));
    }

    public FileError Rename(string from, string to)
    {
        return MapError(DirAccess.RenameAbsolute(from, to));
    }

    public IFileAccess? Open(string path, FileAccess.ModeFlags mode)
    {
        var file = FileAccess.Open(path, mode);
        return file == null ? null : new GodotFileAccess(file);
    }

    public IFileAccess? OpenEncryptedWithPass(string path, FileAccess.ModeFlags mode, string password)
    {
        var file = FileAccess.OpenEncryptedWithPass(path, mode, password);
        return file == null ? null : new GodotFileAccess(file);
    }

    private static FileError MapError(Error error)
    {
        return error switch
        {
            Error.Ok => FileError.Ok,
            Error.FileAlreadyInUse => FileError.AccessDenied,
            Error.FileCantOpen => FileError.FileCantOpen,
            Error.FileNotFound => FileError.FileNotFound,
            Error.FileNoPermission => FileError.AccessDenied,
            Error.CantCreate => FileError.CantCreate,
            Error.CantOpen => FileError.FileCantOpen,
            _ => FileError.Failed
        };
    }
}
