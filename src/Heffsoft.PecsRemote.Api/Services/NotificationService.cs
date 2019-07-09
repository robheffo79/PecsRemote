using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class NotificationService : INotificationService
    {
        private const Int32 MAX_NOTIFICATIONS = 200;

        private readonly IDataContext dataContext;
        private readonly IDataRepository<Notification> notificationRepo;

        public NotificationService(IDataContext dataContext)
        {
            this.dataContext = dataContext;
            this.notificationRepo = this.dataContext.GetRepository<Notification>();
        }

        public void AddNotification(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            if (notification.Id != Guid.Empty && notificationRepo.Get(notification.Id) != null)
                throw new Exception($"Notification '{notification.Id}' already exists.");

            if (notification.Id == Guid.Empty)
                notification.Id = Guid.NewGuid();

            notificationRepo.Insert<Guid>(notification);
        }

        public void DeleteNotification(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            DeleteNotification(notification.Id);
        }

        public void DeleteNotification(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(id));

            notificationRepo.Delete(id);
        }

        public IEnumerable<Notification> GetNotifications()
        {
            IEnumerable<Notification> notifications = notificationRepo.GetAll().OrderByDescending(n => n.Timestamp).ToArray();

            if(notifications.Count() > MAX_NOTIFICATIONS)
            {
                notifications.Skip(MAX_NOTIFICATIONS).ForEach(n => notificationRepo.Delete(n));
                notifications = notifications.Take(MAX_NOTIFICATIONS);
            }

            return notifications;
        }

        public void MarkAllAsRead()
        {
            GetNotifications().ForEach(i =>
            {
                i.Read = true;
                notificationRepo.Update(i);
            });
        }

        public void MarkAsRead(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            MarkAsRead(notification.Id);
        }

        public void MarkAsRead(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(id));

            Notification notification = notificationRepo.Get(id);
            if(notification != null)
            {
                notification.Read = true;
                notificationRepo.Update(notification);
            }
        }
    }
}
