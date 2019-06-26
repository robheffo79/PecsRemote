using Heffsoft.PecsRemote.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface ICastService
    {
        IEnumerable<CastTarget> GetCastReceivers();
        Boolean ConnectToCastReceiver(String id);
        void DisconnectCastReceiver();
        void CastMedia(Uri url);
        void StopMedia();
    }
}
