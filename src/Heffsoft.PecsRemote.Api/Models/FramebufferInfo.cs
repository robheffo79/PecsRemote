using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Models
{
    public class FramebufferInfo
    {
        public Int32 DisplayId { get; set; }
        public String DevNode { get; set; }
        public Int32 Width { get; set; }
        public Int32 Height { get; set; }
        public PixelFormat PixelFormat { get; set; }
    }
}
