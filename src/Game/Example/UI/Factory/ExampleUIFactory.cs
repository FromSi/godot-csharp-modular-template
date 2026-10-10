using Game.Game.Example.Service;

namespace Game.Game.Example.UI.Factory;

/// <summary>
/// Builds <see cref="ExampleUI"/> with its dependencies injected. Factories keep
/// the composition root (<c>GameLevel</c>) from knowing how a screen is assembled.
/// </summary>
public class ExampleUIFactory
{
    private readonly INoteService _noteService;

    public ExampleUIFactory(INoteService noteService)
    {
        _noteService = noteService;
    }

    public ExampleUI Create()
    {
        return new ExampleUI(_noteService);
    }
}
