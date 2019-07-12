using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heffsoft.PecsRemote.Api.Data.Models
{
    [Table("Settings")]
    public class Setting
    {
        public Int32 Id { get; set; }
        public String Key { get; set; }
        public String Value { get; set; }
    }
}
