﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Models
{
    [Table("Media")]
    public class Media
    {
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public Guid Image { get; set; }
        public String Url { get; set; }
        public Boolean Enabled { get; set; }
        public DateTime Created { get; set; }
        public Int32 CreatedByUserId { get; set; }
        public DateTime LastUpdated { get; set; }
        public Int32 LastUpdatedByUserId { get; set; }
    }
}
