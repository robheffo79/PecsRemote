using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Models
{
    [Table("Settings")]
    public class Setting
    {
        public Int32 Id { get; set; }
        public String Key { get; set; }
        public String Value { get; set; }
    }
}
