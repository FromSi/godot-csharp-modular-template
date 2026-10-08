using Game.Game.Common.Repository;
using Game.Game.Common.Service;
using Game.Game.Example.Domain;
using Game.Game.Example.Domain.Observer;
using System;

namespace Game.Game.Example.Service;

/// <summary>
/// In-memory logic of the example module: read the note, change it (which notifies
/// observers; the change is stamped with <see cref="IClockService"/> time) and generate a
/// random number. Persistence is NOT here — saving/loading is centralized in
/// <see cref="SaveService"/>, which owns this module's repository.
/// </summary>
public class NoteService : INoteService
{
    private readonly ISingleRepository<NoteState> _repository;
    private readonly IRandomGeneratorService _randomGeneratorService;
    private readonly IClockService _clockService;

    public NoteService(
        ISingleRepository<NoteState> repository,
        IRandomGeneratorService randomGeneratorService,
        IClockService clockService
    )
    {
        _repository = repository;
        _randomGeneratorService = randomGeneratorService;
        _clockService = clockService;
    }

    public string Get()
    {
        return _repository.GetOne().Text;
    }

    public DateTime? GetChangedAt()
    {
        return _repository.GetOne().ChangedAt;
    }

    public int RandomNumber()
    {
        return _randomGeneratorService.RandiRange(0, 999_999);
    }

    public void SetText(string text)
    {
        _repository.GetOne().ChangeText(text, _clockService.Now);
    }

    public void Subscribe(INoteStateObserver observer)
    {
        _repository.GetOne().AddObserver(observer);
    }

    public void Unsubscribe(INoteStateObserver observer)
    {
        _repository.GetOne().RemoveObserver(observer);
    }
}
