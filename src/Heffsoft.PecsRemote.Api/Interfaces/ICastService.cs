using Heffsoft.PecsRemote.Api.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface ICastService
    {
        String CurrentReceiver { get; }

        Task<IEnumerable<CastTarget>> GetCastReceivers();
        Task<Boolean> ConnectToCastReceiver(String id);
        Task DisconnectCastReceiver();
        Task CastMedia(Uri url);
        Task StopMedia();
    }
}
