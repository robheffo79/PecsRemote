using Heffsoft.PecsRemote.Api.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class HostService : IHostService
    {
        private const String HOSTNAME_FILE = "/etc/hostname";
        private const String DEV_FOLDER = "/dev";
        private const String FRAMEBUFFER_NODES = "fb";
        private const Int32 TFT_WIDTH = 320;
        private const Int32 TFT_HEIGHT = 240;
        private const PixelFormat TFT_PIXELFORMAT = PixelFormat.Format16bppRgb565;

        public String Hostname => File.ReadAllText(HOSTNAME_FILE);

        public Int32 ConnectedDisplays => Directory.EnumerateFiles(DEV_FOLDER, FRAMEBUFFER_NODES + "*").Count();

        public void ConfigureIPSettings()
        {
            throw new NotImplementedException();
        }

        public void ConfigureIPSettings(String ipv4, String subnet, String gateway, String primaryDns, String secondaryDns)
        {
            throw new NotImplementedException();
        }

        public void ConnectToWiFi(String ssid, String key)
        {
            throw new NotImplementedException();
        }

        public void ConnectToWiFi(String ssid, String username, String password)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<String> ScanForWiFi()
        {
            throw new NotImplementedException();
        }

        public void SetHostname(String hostname)
        {
            throw new NotImplementedException();
        }

        public void SetImage(Int32 displayId, Bitmap image)
        {
            Byte[] pixelData = GetBitmapBytes(TFT_WIDTH, TFT_HEIGHT, TFT_PIXELFORMAT, image);
            String fbNode = Path.Combine(DEV_FOLDER, $"{FRAMEBUFFER_NODES}{displayId}");

            File.WriteAllBytes(fbNode, pixelData);
        }

        private Byte[] GetBitmapBytes(Int32 width, Int32 height, PixelFormat pixelFormat, Bitmap image)
        {
            Bitmap dest = new Bitmap(width, height, pixelFormat);
            dest.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using(Graphics gfx = Graphics.FromImage(dest))
            {
                gfx.CompositingQuality = CompositingQuality.HighQuality;
                gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gfx.SmoothingMode = SmoothingMode.HighQuality;

                gfx.FillRectangle(Brushes.Black, 0, 0, 320, 240);

                Single scaleX = (Single)image.Width / width;
                Single scaleY = (Single)image.Height / height;
                Single scale = (scaleX < scaleY) ? scaleX : scaleY;

                Int32 drawWidth = (Int32)(image.Width / scale);
                Int32 drawHeight = (Int32)(image.Height / scale);

                Int32 ofsX = (Int32)((drawWidth - width) * 0.5f);
                Int32 ofsY = (Int32)((drawHeight - height) * 0.5f);

                gfx.DrawImage(image, ofsX, ofsY, drawWidth, drawHeight);
            }

            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bitmapData = dest.LockBits(rect, ImageLockMode.ReadOnly, pixelFormat);
            Byte[] pixelData = new Byte[width * height * Image.GetPixelFormatSize(pixelFormat)];
            Marshal.Copy(bitmapData.Scan0, pixelData, 0, pixelData.Length);
            dest.UnlockBits(bitmapData);

            return pixelData;
        }
    }
}
