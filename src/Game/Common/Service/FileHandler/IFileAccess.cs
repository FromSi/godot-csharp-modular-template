using System;

namespace Game.Game.Common.Service.FileHandler;

public interface IFileAccess : IDisposable
{
    void StoreString(string value);
    void StoreBuffer(byte[] buffer);
    string GetAsText();
    byte[] GetBuffer(long length);
    long GetLength();
    void Close();
}
