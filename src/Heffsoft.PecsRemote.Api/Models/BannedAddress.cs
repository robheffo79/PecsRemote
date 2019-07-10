﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

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
