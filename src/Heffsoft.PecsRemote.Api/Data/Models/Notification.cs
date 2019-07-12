using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heffsoft.PecsRemote.Api.Data.Models
{
    [Table("Notifications")]
    public class Notification
    {
        public Guid Id { get; set; } = Guid.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public NotificationType Type { get; set; }
        public String Title { get; set; }
        public String Image { get; set; }
        public String Content { get; set; }
        public Boolean Read { get; set; } = false;
    }

    public enum NotificationType : int
    {
        System = 0,
        Security = 1,
        Updates = 2,
        General = 3
    }
}