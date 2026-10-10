using Game.Game.Common.Enum;
using Game.Game.Common.Service;
using Game.Game.Common.Service.FileHandler;
using Game.Game.Common.Service.JsonConverter;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using FileAccess = Godot.FileAccess;

namespace Game.Tests.Common.Service.FileHandler;

// Writes go through path.tmp, are read back, and only then replace the save, so a failed write
// never damages the previous one.
[TestFixture]
public class JsonStateFileHandlerServiceTests
{
    private const string Path = "user://saves/game.save";
    private const string TempPath = Path + ".tmp";
    private const string Json = "[{\"_cls\":\"Note\"}]";

    private Mock<IOsService> _os = null!;
    private Mock<IFileSystemService> _fileSystem = null!;
    private Mock<IJsonConverterStateService> _converter = null!;
    private Mock<IFileAccess> _file = null!;
    private JsonStateFileHandlerService _service = null!;
    private string _written = "";

    [SetUp]
    public void SetUp()
    {
        _os = new Mock<IOsService>();
        _fileSystem = new Mock<IFileSystemService>();
        _converter = new Mock<IJsonConverterStateService>();
        _file = new Mock<IFileAccess>();
        _written = "";

        _converter.Setup(c => c.Serialize(It.IsAny<List<object?>>())).Returns(Json);
        _file.Setup(f => f.StoreString(It.IsAny<string>())).Callback<string>(text => _written = text);
        _file.Setup(f => f.GetAsText()).Returns(() => _written);
        _fileSystem.Setup(s => s.DirExists("user://saves")).Returns(true);
        _fileSystem.Setup(s => s.FileExists(TempPath)).Returns(true);
        _fileSystem.Setup(s => s.Open(TempPath, It.IsAny<FileAccess.ModeFlags>())).Returns(_file.Object);
        _fileSystem.Setup(s => s.Rename(TempPath, Path)).Returns(FileError.Ok);

        _service = new JsonStateFileHandlerService(_os.Object, _fileSystem.Object, _converter.Object);
    }

    [Test]
    public void Store_WritesTheTempFileThenReplacesTheSave()
    {
        Assert.That(_service.Store([], Path), Is.EqualTo(FileError.Ok));

        Assert.That(_written, Is.EqualTo(Json));
        _fileSystem.Verify(s => s.Open(Path, It.IsAny<FileAccess.ModeFlags>()), Times.Never);
        _fileSystem.Verify(s => s.Rename(TempPath, Path), Times.Once);
    }

    [Test]
    public void Store_KeepsTheOldSaveWhenTheReadBackDiffers()
    {
        _file.Setup(f => f.GetAsText()).Returns("[{\"_cls\"");   // the disk filled up mid-write

        Assert.That(_service.Store([], Path), Is.EqualTo(FileError.Failed));

        _fileSystem.Verify(s => s.Remove(TempPath), Times.Once);
        _fileSystem.Verify(s => s.Rename(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Store_KeepsTheOldSaveWhenTheTempFileDoesNotOpen()
    {
        _fileSystem.Setup(s => s.Open(TempPath, It.IsAny<FileAccess.ModeFlags>())).Returns((IFileAccess?)null);

        Assert.That(_service.Store([], Path), Is.EqualTo(FileError.FileCantOpen));

        _fileSystem.Verify(s => s.Rename(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Store_ReportsAFailedReplace()
    {
        _fileSystem.Setup(s => s.Rename(TempPath, Path)).Returns(FileError.AccessDenied);

        Assert.That(_service.Store([], Path), Is.EqualTo(FileError.AccessDenied));
    }

    [Test]
    public void Store_EncryptsInProduction()
    {
        _os.Setup(o => o.IsProduction()).Returns(true);
        _fileSystem
            .Setup(s => s.OpenEncryptedWithPass(TempPath, It.IsAny<FileAccess.ModeFlags>(), It.IsAny<string>()))
            .Returns(_file.Object);

        Assert.That(_service.Store([], Path), Is.EqualTo(FileError.Ok));

        _fileSystem.Verify(s => s.Open(It.IsAny<string>(), It.IsAny<FileAccess.ModeFlags>()), Times.Never);
    }

    [Test]
    public void Exists_AsksTheFileSystem()
    {
        _fileSystem.Setup(s => s.FileExists(Path)).Returns(true);

        Assert.That(_service.Exists(Path), Is.True);
        Assert.That(_service.Exists("user://saves/other.save"), Is.False);
    }
}
