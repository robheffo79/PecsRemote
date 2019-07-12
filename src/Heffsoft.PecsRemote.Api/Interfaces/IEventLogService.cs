using System;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IEventLogService
    {
        void Log(String source, String message);
        void Log(String source, String message, String data);
        void Log(String source, String message, Object data);
    }
}
