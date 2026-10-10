using Game.Game.Common.Repository;
using Game.Game.Common.Service;
using Game.Game.Common.Service.FileHandler;
using Game.Game.Level.Adapter;
using Game.Game.Settings.Domain;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace Game.Tests.Level.Adapter;

[TestFixture]
public class SettingsAdapterTests
{
    [Test]
    public void SettingsFileStore_WritesOnlyTheSettingsToTheirFile()
    {
        var repository = new SingleRepository<SettingsState>();
        var settings = new SettingsState { WindowWidth = 1920, WindowHeight = 1080 };
        repository.Update(settings);
        var file = new Mock<IJsonStateFileHandlerService>();
        var slot = new StateSlot<SettingsState>(repository, () => new SettingsState());

        new SettingsFileStore(new SaveService("settings.json", file.Object, [slot])).Save();

        file.Verify(f => f.Store(
            It.Is<List<object?>>(data => data.Count == 1 && data[0] == settings),
            "settings.json",
            It.IsAny<bool>()
        ), Times.Once);
    }
}
