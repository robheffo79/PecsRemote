using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IEventLogService
    {
        void Log(String source, String message);
        void Log(String source, String message, String data);
        void Log(String source, String message, Object data);
    }
}
