using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IIdleService
    {
        event EventHandler IdleTimeout;
        event EventHandler TimeoutReset;

        TimeSpan IdleTime { get; set; }

        void ResetTimeout();
    }
}
