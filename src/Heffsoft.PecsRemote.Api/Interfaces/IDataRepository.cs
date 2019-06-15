using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IDataRepository<T>
    {
        IEnumerable<T> GetAll();
        T Get(Int32 id);

        IEnumerable<T> Find(String where, Object args);

        Int32 Insert(T obj);
        void Update(T obj);
        void Delete(T obj);
        void Delete(Int32 id);
    }
}
