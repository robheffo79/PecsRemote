using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IThermalService
    {
        Double Temperature { get; }
        Double FanPower { get; }
        Dictionary<Double, Double> PowerCurve { get; }

        void StartControl();
    }
}
