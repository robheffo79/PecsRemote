using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Models
{
    [Table("EventLog")]
    public class EventLog
    {
        public Int32 Id { get; set; }
        public DateTime Timestamp { get; set; }
        public String Source { get; set; }
        public String Message { get; set; }
        public String Data { get; set; }
    }
}
