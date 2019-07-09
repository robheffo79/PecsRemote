using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public NotificationType Type { get; set; }
        public String Title { get; set; }
        public String Image { get; set; }
        public String Content { get; set; }
        public Boolean Read { get; set; }
    }

    public enum NotificationType : int
    {
        System = 0,
        Updates = 1,
        General = 2
    }
}