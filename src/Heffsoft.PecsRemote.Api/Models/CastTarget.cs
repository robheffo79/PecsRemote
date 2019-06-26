using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Models
{
    public class CastTarget
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public IPAddress Address { get; set; }
    }
}
