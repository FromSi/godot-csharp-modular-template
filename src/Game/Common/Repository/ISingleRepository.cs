namespace Game.Game.Common.Repository;

public interface ISingleRepository<T> where T : class
{
    void Update(T state);
    T GetOne();
    void Delete();
}
