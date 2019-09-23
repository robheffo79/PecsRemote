using Heffsoft.PecsRemote.Api.Devices;
using Heffsoft.PecsRemote.Api.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class DisplayService : IDisplayService
    {
        private readonly Int32 ledPin;
        private readonly Int32 dcPin;
        private readonly Int32 resetPin;
        private readonly Int32 resetTime;
        private readonly Int32 selectPin0;
        private readonly Int32 selectPin1;
        private readonly Int32 selectPin2;
        private readonly Int32 spiBus;
        private readonly Int32 spiCs;
        private readonly Int32 spiFrequency;

        private readonly ISpiBus spi;
        private readonly IDisplayGpio gpio;
        private readonly IIdleService idleService;

        private Bitmap logoBitmap = null;
        private Bitmap blankBitmap = null;

        public Double MaxBrightness { get; set; } = 1.00D;

        public DisplayService(IConfiguration configuration, IIdleService idle)
        {
            IConfigurationSection control = configuration.GetSection("display:control");

            ledPin = control.GetValue<Int32>("led:pin");
            dcPin = control.GetValue<Int32>("dc:pin");
            resetPin = control.GetValue<Int32>("reset:pin");
            resetTime = control.GetValue<Int32>("reset:time");

            selectPin0 = control.GetValue<Int32>("select:pin:0");
            selectPin1 = control.GetValue<Int32>("select:pin:1");
            selectPin2 = control.GetValue<Int32>("select:pin:2");

            spiBus = control.GetValue<Int32>("spi:bus");
            spiCs = control.GetValue<Int32>("spi:cs");
            spiFrequency = control.GetValue<Int32>("spi:frequency");

            spi = new PiSpiBus(spiBus, spiCs, spiFrequency);
            gpio = new DisplayGpio(ledPin, dcPin, resetPin, selectPin0, selectPin1, selectPin2);
            idleService = idle;

            Task.WhenAll(InitialiseDisplays(configuration), LoadInitBitmaps())
                .ContinueWith(delegate { DrawLogoBitmaps(); });
        }

        private void StartIdleService()
        {
            idleService.IdleTimeout += IdleService_IdleTimeout;
            idleService.TimeoutReset += IdleService_TimeoutReset;
            idleService.ResetTimeout();
        }

        private void IdleService_TimeoutReset(object sender, EventArgs e)
        {
            gpio.LED = true;
        }

        private void IdleService_IdleTimeout(Object sender, EventArgs e)
        {
            gpio.LED = false;
        }

        private Task LoadInitBitmaps()
        {
            return Task.Run(() =>
            {
                logoBitmap = LoadBitmap("Heffsoft.PecsRemote.Api.Resources.Logo.png");
                blankBitmap = LoadBitmap("Heffsoft.PecsRemote.Api.Resources.Blank.png");
            });
        }

        private Task InitialiseDisplays(IConfiguration configuration)
        {
            return Task.Run(() =>
            {
                ResetDisplays();

                for (Int32 i = 0; i < 8; i++)
                {
                    if (configuration.GetValue<Boolean>($"display:{i}:enabled", false) == true)
                    {
                        Byte channel = configuration.GetValue<Byte>($"display:{i}:channel", (Byte)i);
                        gpio.ChannelMapping[i] = channel;

                        Displays[i] = new ILI9341(spi, gpio, (Byte)i, Rotation.Landscape, true);
                        Displays[i].Init();
                    }
                    else
                    {
                        Displays[i] = null;
                    }
                }
            });
        }

        private Task DrawLogoBitmaps()
        {
            return Task.Run(() =>
            {
                Boolean logo = true;
                for (Int32 i = 0; i < 8; i++)
                {
                    if (Displays[i] != null)
                    {
                        using (Graphics gfx = Displays[i].Graphics)
                        {
                            gfx.DrawImageUnscaled(logo ? logoBitmap : blankBitmap, 0, 0);
                        }

                        logo = false;
                    }
                }

                if (logoBitmap != null)
                {
                    logoBitmap.Dispose();
                    logoBitmap = null;
                }

                if (blankBitmap != null)
                {
                    blankBitmap.Dispose();
                    blankBitmap = null;
                }

                StartIdleService();
            });
        }

        private void ResetDisplays()
        {
            gpio.LED = false;

            gpio.RESET = false;
            Delay.Us(20);
            gpio.RESET = true;
            Delay.Ms(resetTime);

            Debug.WriteLine("Display Modules Reset");
        }

        private Bitmap LoadBitmap(String resource)
        {
            return new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(resource));
        }

        public IDisplay[] Displays { get; private set; } = new IDisplay[8];

        public void Dispose()
        {
            if (Displays != null)
            {
                foreach (IDisplay display in Displays)
                {
                    display.Dispose();
                }
            }
        }

        public void IdentifyDisplays(TimeSpan time)
        {
        }

        public void SetBacklightTimeout(TimeSpan time)
        {
        }
    }
}
