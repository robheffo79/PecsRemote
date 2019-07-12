using System;
using System.Net;

namespace Heffsoft.PecsRemote.Api.Models
{
    public class FailedAttempt
    {
        public IPAddress ClientIP { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
