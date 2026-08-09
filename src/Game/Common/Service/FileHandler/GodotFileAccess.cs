using Godot;

namespace Game.Game.Common.Service.FileHandler;

public class GodotFileAccess : IFileAccess
{
    private readonly FileAccess _file;

    public GodotFileAccess(FileAccess file)
    {
        _file = file;
    }

    public void StoreString(string value)
    {
        _file.StoreString(value);
    }

    public void StoreBuffer(byte[] buffer)
    {
        _file.StoreBuffer(buffer);
    }

    public string GetAsText()
    {
        return _file.GetAsText();
    }

    public byte[] GetBuffer(long length)
    {
        return _file.GetBuffer(length);
    }

    public long GetLength()
    {
        return (long)_file.GetLength();
    }

    public void Close()
    {
        _file.Close();
    }

    public void Dispose()
    {
        _file.Dispose();
    }
}
