namespace Heffsoft.PecsRemote.Api.Models
{
    public class SystemSettings
    {
        public HostSettings Host { get; set; }
        public NetworkSettings Network { get; set; }
        public CastSettings Cast { get; set; }
    }
}
