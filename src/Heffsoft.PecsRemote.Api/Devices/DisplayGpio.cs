using Heffsoft.PecsRemote.Api.Interfaces;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Devices
{
    public class DisplayGpio : IDisplayGpio
    {
        private Boolean led = false;
        private Boolean dc = false;
        private Boolean reset = false;
        private Byte channel = 0;

        private Int32 ledPin = -1;
        private Int32 dcPin = -1;
        private Int32 resetPin = -1;
        private Int32[] channelPins = null;

        private GpioController gpioController = new GpioController();
        private PwmChannel ledPwmChannel = null;

        public DisplayGpio(Int32 ledPin, Int32 dcPin, Int32 resetPin, params Int32[] channelPins)
        {
            if (gpioController.IsPinOpen(ledPin))
                throw new InvalidOperationException($"{nameof(ledPin)} is in use");

            if (gpioController.IsPinOpen(dcPin))
                throw new InvalidOperationException($"{nameof(dcPin)} is in use");

            if (gpioController.IsPinOpen(resetPin))
                throw new InvalidOperationException($"{nameof(resetPin)} is in use");

            if (channelPins.Any(i => gpioController.IsPinOpen(i)))
                throw new InvalidOperationException($"{nameof(channelPins)} has an in use pin");

            this.ledPin = ledPin;
            this.dcPin = dcPin;
            this.resetPin = resetPin;
            this.channelPins = channelPins;

            gpioController.OpenPin(ledPin);
            gpioController.SetPinMode(ledPin, PinMode.Output);
            gpioController.Write(ledPin, PinValue.Low);

            gpioController.OpenPin(dcPin);
            gpioController.SetPinMode(dcPin, PinMode.Output);
            gpioController.Write(dcPin, PinValue.Low);

            gpioController.OpenPin(resetPin);
            gpioController.SetPinMode(resetPin, PinMode.Output);
            gpioController.Write(resetPin, PinValue.Low);

            for (Int32 i = 0; i < channelPins.Length; i++)
            {
                gpioController.OpenPin(channelPins[i]);
                gpioController.SetPinMode(channelPins[i], PinMode.Output);
                gpioController.Write(channelPins[i], PinValue.Low);
            }

            for(Int32 i = 0; i < 256; i++)
            {
                ChannelMapping[i] = (Byte)i;
            }
        }

        public Boolean LED
        {
            get => led;
            set
            {
                if(value != led)
                {
                    led = value;
                    gpioController.Write(ledPin, value ? PinValue.High : PinValue.Low);
                }
            }
        }
        
        public Boolean DC
        {
            get => dc;
            set
            {
                if (value != dc)
                {
                    dc = value;
                    gpioController.Write(dcPin, value ? PinValue.High : PinValue.Low);
                }
            }
        }

        public Boolean RESET
        {
            get => reset;
            set
            {
                if (value != reset)
                {
                    reset = value;
                    gpioController.Write(resetPin, value ? PinValue.High : PinValue.Low);
                }
            }
        }

        public Byte Channel
        {
            get => channel;
            set
            {
                if (value != channel)
                {
                    channel = value;

                    Byte c = ChannelMapping[value];
                    PinValuePair[] pinValues = new PinValuePair[channelPins.Length];
                    for(Int32 i = 0; i < channelPins.Length; i++)
                    {
                        pinValues[i] = new PinValuePair(channelPins[i], ((c >> i) & 0x01) == 1 ? PinValue.High : PinValue.Low);
                    }

                    gpioController.Write(new ReadOnlySpan<PinValuePair>(pinValues));
                }
            }
        }

        public Byte[] ChannelMapping { get; } = new Byte[256];

        //public Task FadeLED(Double to, TimeSpan time, FadeMethod fade)
        //{
        //    return Task.Run(() =>
        //    {
        //        //AutoResetEvent autoResetEvent = new AutoResetEvent(true);
        //        Double from = LED;

        //        Double d = 1.00D / time.Ticks;
        //        Stopwatch timer = Stopwatch.StartNew();
        //        while(timer.Elapsed < time)
        //        {
        //            Double v = 0.00D;
        //            switch (fade)
        //            {
        //                case FadeMethod.Linear:
        //                    v = LinearInterpolate(from, to, d * timer.ElapsedTicks);
        //                    break;

        //                case FadeMethod.EaseInOut:
        //                    v = EaseInterpolate(from, to, d * timer.ElapsedTicks);
        //                    break;
        //            }

        //            LED = Math.Clamp(v, 0.00D, 1.00D);
        //            Delay.Ms(10);
        //        }

        //        LED = to;
        //    });
        //}

        //private static Double LinearInterpolate(Double from, Double to, Double t)
        //{
        //    return from + (to - from) * t;
        //}

        //private static Double EaseInterpolate(Double from, Double to, Double t)
        //{
        //    Double sqt = Math.Pow(t, 2.00D);
        //    Double u = sqt / (2.00D * (sqt - t) + 1.00D);
        //    return from + (to - from) * u;
        //}
    }
}
