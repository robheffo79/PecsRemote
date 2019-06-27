using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Models
{
    public class NetworkSettings
    {
        public Boolean UseDhcp { get; set; }
        public String IPAddress { get; set; }
        public String SubnetMask { get; set; }
        public String GatewayAddress { get; set; }
        public IEnumerable<String> DNSServers { get; set; }
        public String SSID { get; set; }
        public String Key { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
    }
}
