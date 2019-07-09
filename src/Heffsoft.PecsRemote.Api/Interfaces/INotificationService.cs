using Heffsoft.PecsRemote.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface INotificationService
    {
        void AddNotification(Notification notification);
        void DeleteNotification(Notification notification);
        void DeleteNotification(Guid id);
        void MarkAsRead(Notification notification);
        void MarkAsRead(Guid id);
        void MarkAllAsRead();

        IEnumerable<Notification> GetNotifications();
    }
}
