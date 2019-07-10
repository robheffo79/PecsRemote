using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heffsoft.PecsRemote.Api.Models
{
    [Table("BannedAddresses")]
    public class BannedAddress
    {
        public Guid Id { get; set; }
        public DateTime LastBanned { get; set; }
        public DateTime? UnbanAt { get; set; }
        public Int64 BanCount { get; set; }
        public String IPAddress { get; set; }
    }
}
