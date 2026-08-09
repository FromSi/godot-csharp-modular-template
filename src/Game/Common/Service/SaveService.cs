using System.Collections.Generic;
using Game.Game.Common.Domain;
using Game.Game.Common.Repository;
using Game.Game.Common.Service.FileHandler;
using Game.Game.Example.Domain;

namespace Game.Game.Common.Service;

/// <summary>
/// Central persistence: gathers every repository that takes part in a save and writes
/// them to a single file (and restores them back). Module services do NOT do file I/O
/// themselves — they own in-memory state, this service owns saving.
///
/// The list order in <see cref="Save"/> must match the positional casts in
/// <see cref="Load"/>. When you add a persistable module, add its repository here
/// (single state → <c>GetOne()</c>, collection → <c>GetAll()</c>) and register its
/// type in the JsonConverter (in <c>GameLevel</c>). This is the one place in
/// <c>Common</c> that references concrete module states — the trade-off for a single,
/// explicit save file.
/// </summary>
public class SaveService
{
    private readonly string _savePath;
    private readonly IJsonStateFileHandlerService _fileHandlerService;

    private readonly ISingleRepository<IdState> _idRepository;
    private readonly ISingleRepository<NoteState> _noteRepository;

    public SaveService(
        string savePath,
        IJsonStateFileHandlerService fileHandlerService,
        ISingleRepository<IdState> idRepository,
        ISingleRepository<NoteState> noteRepository
    )
    {
        _savePath = savePath;
        _fileHandlerService = fileHandlerService;
        _idRepository = idRepository;
        _noteRepository = noteRepository;
    }

    public void Save()
    {
        var data = new List<object?>
        {
            _idRepository.GetOne(),
            _noteRepository.GetOne(),
        };

        _fileHandlerService.Store(data, _savePath);
    }

    public bool Load()
    {
        var data = _fileHandlerService.Load(_savePath);

        if (data is not { Count: 2 })
        {
            return false;
        }

        _idRepository.Delete();
        _idRepository.Update((IdState)data[0]!);

        _noteRepository.Delete();
        _noteRepository.Update((NoteState)data[1]!);

        return true;
    }
}
