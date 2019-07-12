using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heffsoft.PecsRemote.Api.Models
{
    [Table("Playlists")]
    public class Playlist
    {
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public Guid Image { get; set; }
        public Boolean Enabled { get; set; }
        public PlaybackMode PlaybackMode { get; set; }
        public DateTime Created { get; set; }
        public Int32 CreatedByUserId { get; set; }
        public DateTime LastUpdated { get; set; }
        public Int32 LastUpdatedByUserId { get; set; }
    }

    public enum PlaybackMode : int
    {
        Sequential,
        Random
    }
}