using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Models
{
    public class FailedAttempt
    {
        public IPAddress ClientIP { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
