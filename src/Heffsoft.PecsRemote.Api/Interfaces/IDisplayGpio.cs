using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IDisplayGpio
    {
        Boolean LED { get; set; }
        Boolean DC { get; set; }
        Boolean RESET { get; set; }
        Byte Channel { get; set; }
        Byte[] ChannelMapping { get; }

        //Task FadeLED(Double to, TimeSpan time, FadeMethod fade);
    }

    public enum FadeMethod
    {
        Linear,
        EaseInOut
    }
}
