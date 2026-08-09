using Game.Game.Common.Repository;
using Game.Game.Common.Service;
using Game.Game.Example.Domain;
using Game.Game.Example.Domain.Observer;

namespace Game.Game.Example.Service;

/// <summary>
/// In-memory logic of the example module: read the note, change it (which notifies
/// observers) and generate a random number. Persistence is NOT here — saving/loading
/// is centralized in <see cref="SaveService"/>, which owns this module's repository.
/// </summary>
public class NoteService : INoteService
{
    private readonly ISingleRepository<NoteState> _repository;
    private readonly IRandomGeneratorService _randomGeneratorService;

    public NoteService(
        ISingleRepository<NoteState> repository,
        IRandomGeneratorService randomGeneratorService
    )
    {
        _repository = repository;
        _randomGeneratorService = randomGeneratorService;
    }

    public string Get()
    {
        return _repository.GetOne().Text;
    }

    public int RandomNumber()
    {
        return _randomGeneratorService.RandiRange(0, 999_999);
    }

    public void SetText(string text)
    {
        _repository.GetOne().ChangeText(text);
    }

    public void Subscribe(INoteStateObserver observer)
    {
        _repository.GetOne().AddObserver(observer);
    }
}
