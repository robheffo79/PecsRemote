namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IDataContext
    {
        void BeginTransaction();
        void Commit();
        void Rollback();

        IDataRepository<T> GetRepository<T>();
    }
}
