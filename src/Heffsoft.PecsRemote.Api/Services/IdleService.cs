using Heffsoft.PecsRemote.Api.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class IdleService : IIdleService
    {
        private readonly IConfigurationSection idleSection;
        private readonly Timer timer;
        private TimeSpan idleTime = TimeSpan.Zero;

        public IdleService(IConfiguration configuration)
        {
            timer = new Timer();
            timer.Elapsed += Timer_Elapsed;

            idleSection = configuration.GetSection("display:backlight:idle");
            idleTime = TimeSpan.FromMilliseconds(idleSection.Get<Double>());

            ResetTimeout();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            OnIdleTimeout();
        }

        public TimeSpan IdleTime
        {
            get => idleTime;
            set
            {
                idleTime = value;
                idleSection.Value = value.ToString();

                ResetTimeout();
            }
        }

        public event EventHandler IdleTimeout;

        public void ResetTimeout()
        {
            timer.Stop();

            if (idleTime > TimeSpan.Zero)
            {
                timer.Interval = idleTime.TotalMilliseconds;
                timer.Start();
            }
        }

        public virtual void OnIdleTimeout()
        {
            IdleTimeout?.Invoke(this, EventArgs.Empty);
        }
    }
}
