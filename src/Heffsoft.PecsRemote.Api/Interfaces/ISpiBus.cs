using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface ISpiBus : IDisposable
    {
        void Aquire();
        Boolean Aquire(TimeSpan timeout);

        void Write(Byte data);
        void Write(IEnumerable<Byte> data);

        void Write(UInt16 data);
        void Write(IEnumerable<UInt16> data);

        void Write(UInt32 data);
        void Write(IEnumerable<UInt32> data);

        Byte Read();
        void Read(Byte[] buffer);
        void Read(Byte[] buffer, Int32 offset, Int32 length);

        void Release();
    }
}
