using Heffsoft.PecsRemote.Api.Data.Models;
using Heffsoft.PecsRemote.Api.Interfaces;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class EventLogService : IEventLogService
    {
        private readonly IDataContext dataContext;
        private readonly IDataRepository<EventLog> eventLogRepo;

        public EventLogService(IDataContext dataContext)
        {
            this.dataContext = dataContext;
            this.eventLogRepo = dataContext.GetRepository<EventLog>();
        }

        public void Log(String source, String message)
        {
            DateTime timestamp = DateTime.Now;

            Task.Run(() =>
            {
                lock (eventLogRepo)
                {
                    eventLogRepo.Insert<Int32>(new EventLog()
                    {
                        Id = 0,
                        Timestamp = timestamp,
                        Source = source,
                        Message = message,
                        Data = null
                    });
                }
            });
        }

        public void Log(String source, String message, String data)
        {
            DateTime timestamp = DateTime.Now;

            Task.Run(() =>
            {
                lock (eventLogRepo)
                {
                    eventLogRepo.Insert<Int32>(new EventLog()
                    {
                        Id = 0,
                        Timestamp = timestamp,
                        Source = source,
                        Message = message,
                        Data = data
                    });
                }
            });
        }

        public void Log(String source, String message, Object data)
        {
            DateTime timestamp = DateTime.Now;

            Task.Run(() =>
            {
                String strData = JsonConvert.SerializeObject(data);

                lock (eventLogRepo)
                {
                    eventLogRepo.Insert<Int32>(new EventLog()
                    {
                        Id = 0,
                        Timestamp = timestamp,
                        Source = source,
                        Message = message,
                        Data = strData
                    });
                }
            });
        }
    }
}
