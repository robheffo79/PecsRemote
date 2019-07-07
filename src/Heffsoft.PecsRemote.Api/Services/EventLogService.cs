using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Task.Run(() =>
            {
                lock (eventLogRepo)
                {
                    eventLogRepo.Insert<Int32>(new EventLog()
                    {
                        Id = 0,
                        Timestamp = DateTime.Now,
                        Source = source,
                        Message = message,
                        Data = null
                    });
                }
            });
        }

        public void Log(String source, String message, String data)
        {
            Task.Run(() =>
            {
                lock (eventLogRepo)
                {
                    eventLogRepo.Insert<Int32>(new EventLog()
                    {
                        Id = 0,
                        Timestamp = DateTime.Now,
                        Source = source,
                        Message = message,
                        Data = data
                    });
                }
            });
        }

        public void Log(String source, String message, Object data)
        {
            Task.Run(() =>
            {
                String strData = JsonConvert.SerializeObject(data);

                lock (eventLogRepo)
                {
                    eventLogRepo.Insert<Int32>(new EventLog()
                    {
                        Id = 0,
                        Timestamp = DateTime.Now,
                        Source = source,
                        Message = message,
                        Data = strData
                    });
                }
            });
        }
    }
}
