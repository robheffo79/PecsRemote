using System;
using System.Drawing.Imaging;

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
