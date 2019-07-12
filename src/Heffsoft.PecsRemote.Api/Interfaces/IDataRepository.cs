using System;
using System.Collections.Generic;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IDataRepository<T>
    {
        IEnumerable<T> GetAll();
        T Get<U>(U id);

        IEnumerable<T> Find(String where, Object args);

        U Insert<U>(T obj);
        void Update(T obj);
        void Delete(T obj);
        void Delete<U>(U id);
    }
}
