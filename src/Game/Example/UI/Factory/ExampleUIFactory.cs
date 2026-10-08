using Game.Game.Common.Service;
using Game.Game.Example.Service;

namespace Game.Game.Example.UI.Factory;

/// <summary>
/// Builds <see cref="ExampleUI"/> with its dependencies injected. Factories keep
/// the composition root (<c>GameLevel</c>) from knowing how a screen is assembled.
/// </summary>
public class ExampleUIFactory
{
    private readonly INoteService _noteService;
    private readonly SaveService _saveService;

    public ExampleUIFactory(INoteService noteService, SaveService saveService)
    {
        _noteService = noteService;
        _saveService = saveService;
    }

    public ExampleUI Create()
    {
        return new ExampleUI(_noteService, _saveService);
    }
}
