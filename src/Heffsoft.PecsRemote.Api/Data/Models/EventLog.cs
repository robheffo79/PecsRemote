using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heffsoft.PecsRemote.Api.Data.Models
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
