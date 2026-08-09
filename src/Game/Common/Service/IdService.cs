using Game.Game.Common.Domain;
using Game.Game.Common.Repository;

namespace Game.Game.Common.Service;

public class IdService : IIdService
{
    private readonly ISingleRepository<IdState> _idRepository;

    public IdService(ISingleRepository<IdState> idRepository)
    {
        _idRepository = idRepository;
    }

    public int Next()
    {
        var state = _idRepository.GetOne();

        state.Counter++;

        return state.Counter;
    }
}
