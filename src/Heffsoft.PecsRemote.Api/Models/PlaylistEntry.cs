using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heffsoft.PecsRemote.Api.Models
{
    [Table("PlaylistEntries")]
    public class PlaylistEntry
    {
        public Int32 PlaylistId { get; set; }
        public Int32 MediaId { get; set; }
        public Boolean Enabled { get; set; }
        public Int32 Order { get; set; }
        public DateTime Added { get; set; }
        public Int32 AddedByUserId { get; set; }
    }
}
