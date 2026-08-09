using System.Collections.Generic;
using Game.Game.Common.Enum;
using Game.Game.Common.Service.JsonConverter;

namespace Game.Game.Common.Service.FileHandler;

public class JsonStateFileHandlerService : FileHandlerService, IJsonStateFileHandlerService
{
    private readonly IJsonConverterStateService _jsonConverterStateService;

    public JsonStateFileHandlerService(
        IOsService osService,
        IFileSystemService fileSystemService,
        IJsonConverterStateService jsonConverterStateService
    ) : base(osService, fileSystemService)
    {
        _jsonConverterStateService = jsonConverterStateService;
    }

    protected override void WriteContent(IFileAccess file, string content)
    {
        file.StoreString(content);
    }

    protected override string? ReadContent(IFileAccess file)
    {
        var text = file.GetAsText();
        return text.Length == 0 ? null : text;
    }

    public FileError Store(List<object?> data, string path, bool createDir = true)
    {
        return base.Store(_jsonConverterStateService.Serialize(data), path, createDir);
    }

    public new List<object?> Load(string path)
    {
        var json = base.Load(path);
        return json == null ? [] : _jsonConverterStateService.Deserialize(json);
    }
}
