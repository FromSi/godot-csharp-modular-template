using System.IO;
using Game.Game.Common.Enum;
using FileAccess = Godot.FileAccess;

namespace Game.Game.Common.Service.FileHandler;

public abstract class FileHandlerService
{
    private const string EncryptionKey = "2f207ce7395badbdde6d92386b1650a7d18a15d24dca9d2eb40f6c9a4f8e27b6";

    protected readonly IOsService OsService;
    protected readonly IFileSystemService FileSystemService;

    protected FileHandlerService(IOsService osService, IFileSystemService fileSystemService)
    {
        OsService = osService;
        FileSystemService = fileSystemService;
    }
    
    protected abstract void WriteContent(IFileAccess file, string content);
    protected abstract string? ReadContent(IFileAccess file);

    public FileError Store(string content, string path, bool createDir = true)
    {
        var (error, file) = OpenFileForWrite(path, createDir);

        if (error != FileError.Ok)
        {
            return error;
        }

        WriteContent(file!, content);
        file!.Close();
        file.Dispose();

        return FileError.Ok;
    }

    public string? Load(string path)
    {
        var (error, file) = OpenFileForRead(path);

        if (error != FileError.Ok)
        {
            return null;
        }

        var content = ReadContent(file!);
        file!.Close();
        file.Dispose();

        return content;
    }

    private (FileError, IFileAccess?) OpenFileForWrite(string path, bool createDir)
    {
        var error = CheckAndCreateDir(path, createDir);

        if (error != FileError.Ok)
        {
            return (error, null);
        }

        var file = OsService.IsProduction()
            ? FileSystemService.OpenEncryptedWithPass(path, FileAccess.ModeFlags.Write, EncryptionKey)
            : FileSystemService.Open(path, FileAccess.ModeFlags.Write);

        return file == null ? (FileError.FileCantOpen, null) : (FileError.Ok, file);
    }

    private (FileError, IFileAccess?) OpenFileForRead(string path)
    {
        if (!FileSystemService.FileExists(path))
        {
            return (FileError.FileNotFound, null);
        }

        var file = OsService.IsProduction()
            ? FileSystemService.OpenEncryptedWithPass(path, FileAccess.ModeFlags.Read, EncryptionKey)
            : FileSystemService.Open(path, FileAccess.ModeFlags.Read);

        return file == null ? (FileError.FileCantOpen, null) : (FileError.Ok, file);
    }

    private FileError CheckAndCreateDir(string filePath, bool create)
    {
        var dirPath = filePath.Contains("://") 
            ? filePath[..filePath.LastIndexOf('/')] 
            : Path.GetDirectoryName(filePath) ?? "";

        if (dirPath == "" || FileSystemService.DirExists(dirPath))
        {
            return FileError.Ok;
        }

        return !create ? FileError.CantCreate : FileSystemService.MakeDirRecursive(dirPath);
    }
}
