using System.Text;

namespace Game.Game.Common.Service.FileHandler;

public class BinFileHandlerService : FileHandlerService
{
    public BinFileHandlerService(IOsService osService, IFileSystemService fileSystemService) 
        : base(osService, fileSystemService) { }

    protected override void WriteContent(IFileAccess file, string content)
    {
        file.StoreBuffer(Encoding.UTF8.GetBytes(content));
    }

    protected override string? ReadContent(IFileAccess file)
    {
        var bytes = file.GetBuffer(file.GetLength());

        return bytes.Length == 0 ? null : Encoding.UTF8.GetString(bytes);
    }
}
