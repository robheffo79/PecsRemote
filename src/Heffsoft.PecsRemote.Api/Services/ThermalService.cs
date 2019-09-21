using Heffsoft.PecsRemote.Api.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class ThermalService : IThermalService
    {
        private readonly String sensorFile;
        private readonly Int32 fanPin;
        private readonly Double startPower;
        private readonly Double minPower;
        private readonly Int32 frequency;
        private readonly Int32 updateTime;

        private Timer timer = null;

        private readonly PwmChannel fanPwm;

        public Double Temperature
        {
            get
            {
                if(Double.TryParse(File.ReadAllText(sensorFile), out Double temp))
                {
                    return temp / 1000.0D;
                }

                return 0.0D;
            }
        }

        private Double desiredPower;
        public Double FanPower
        {
            get => desiredPower;
            set
            {
                Double newPower = desiredPower;

                if(value < minPower)
                {
                    newPower = minPower;
                }
                else if(value > 1.0D)
                {
                    newPower = 1.0D;
                }
                else
                {
                    newPower = value;
                }

                if(newPower != desiredPower)
                {
                    desiredPower = newPower;
                    fanPwm.DutyCyclePercentage = desiredPower;
                }
            }
        }

        public Dictionary<Double, Double> PowerCurve { get; } = new Dictionary<Double, Double>();

        public ThermalService(IConfiguration configuration)
        {
            IConfigurationSection control = configuration.GetSection("thermal:control");
            IConfigurationSection power = configuration.GetSection("thermal:power");

            sensorFile = control.GetValue<String>("sensor");
            fanPin = control.GetValue<Int32>("pin");
            frequency = control.GetValue<Int32>("frequency");
            updateTime = control.GetValue<Int32>("updatetime");

            minPower = power.GetValue<Double>("min");
            startPower = power.GetValue<Double>("start");

            InitPowerCurves(control);

            fanPwm = new SoftwarePwmChannel(fanPin, frequency, startPower);
        }

        private void InitPowerCurves(IConfigurationSection control)
        {
            Int32 i = 0;
            IConfigurationSection curve = control.GetSection($"curves:{i++}");
            while (curve.Exists())
            {
                Double tmp = curve.GetValue<Double>("temp");
                Double pow = curve.GetValue<Double>("power");

                PowerCurve.Add(tmp, pow);

                curve = control.GetSection($"curves:{i++}");
            }
        }

        public void StartControl()
        {
            fanPwm.Start();
            timer = new Timer(new TimerCallback(Poll), null, updateTime, updateTime);
        }

        private void Poll(object state)
        {
            Double temp = Temperature;
            KeyValuePair<Double, Double> nodeA = GetPreviousNode(temp);
            KeyValuePair<Double, Double> nodeB = GetNextNode(temp);

            FanPower = Interpolate(nodeA.Value, nodeB.Value, (1.0D / (nodeB.Key - nodeA.Key) * (temp - nodeA.Key)));
            Debug.WriteLine($"Fan Power Requested: {FanPower}");
        }

        private KeyValuePair<Double, Double> GetNextNode(Double temp)
        {
            return PowerCurve.Where(i => i.Key >= temp)
                             .OrderBy(i => i.Key)
                             .FirstOrDefault();
        }

        private KeyValuePair<Double, Double> GetPreviousNode(Double temp)
        {
            return PowerCurve.Where(i => i.Key <= temp)
                             .OrderByDescending(i => i.Key)
                             .FirstOrDefault();
        }

        private Double Interpolate(Double a, Double b, Double x)
        {
            return a * (1.0D - x) + b * x;
        }
    }
}
