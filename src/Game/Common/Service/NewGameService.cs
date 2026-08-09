using Game.Game.Common.Domain;
using Game.Game.Common.Repository;
using Game.Game.Example.Domain;

namespace Game.Game.Common.Service;

/// <summary>
/// The "new game" counterpart of <see cref="SaveService"/>: resets every repository to
/// a fresh initial state and seeds a new game's starting content. Like <c>SaveService</c>,
/// this is a place in <c>Common</c> that references concrete module states on purpose —
/// it must know what a fresh game looks like.
///
/// Single state → <c>Delete()</c> + <c>Update(new ...)</c>; collection → <c>DeleteAll()</c>
/// (+ <c>UpdateAll(...)</c> to seed). When you add a persistable module, reset it here too.
/// </summary>
public class NewGameService
{
    private readonly ISingleRepository<IdState> _idRepository;
    private readonly ISingleRepository<NoteState> _noteRepository;

    public NewGameService(
        ISingleRepository<IdState> idRepository,
        ISingleRepository<NoteState> noteRepository
    )
    {
        _idRepository = idRepository;
        _noteRepository = noteRepository;
    }

    public void Create()
    {
        _idRepository.Delete();
        _idRepository.Update(new IdState());

        _noteRepository.Delete();
        _noteRepository.Update(new NoteState());
    }
}
