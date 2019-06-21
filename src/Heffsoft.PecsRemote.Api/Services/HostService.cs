using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class HostService : IHostService
    {
        private const String REBOOT_CMD = "reboot";
        private const String WIFI_SCAN_CMD = "iwlist wlan0 scan";
        private const String HOSTNAME_FILE = "/etc/hostname";
        private const String HOSTS_FILE = "/etc/hosts";
        private const String DEV_FOLDER = "/dev";
        private const String SYSGRAPHICS_FOLDER = "/sys/class/graphics";
        private const String VIRTUAL_SIZE_NODE = "virtual_size";
        private const String BPP_NODE = "bits_per_pixel";
        private const String FRAMEBUFFER_NODES = "fb";

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
            Regex ssid = new Regex("ESSID:\"(?<ssid>[\\s\\S]{1,32})\"");

            String scanOutput = RunBash(WIFI_SCAN_CMD);

            foreach (String line in scanOutput.Split(new Char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()))
            {
                Match match = ssid.Match(line);
                if(match.Success)
                {
                    yield return match.Groups["ssid"].Value;
                }
            }
        }

        public void SetHostname(String hostname)
        {
            String oldHostname = File.ReadAllText(HOSTNAME_FILE).Trim();
            String newHostname = hostname.Trim().ToLower();

            // Update Hostname
            File.WriteAllText(HOSTNAME_FILE, newHostname);

            // Update Hosts
            String hostsFile = File.ReadAllText(HOSTS_FILE);
            hostsFile = hostsFile.Replace(oldHostname, newHostname);
            File.WriteAllText(HOSTS_FILE, hostsFile);
        }

        public void SetImage(Int32 displayId, Bitmap image)
        {
            FramebufferInfo info = GetFramebufferInfo(displayId);

            Byte[] pixelData = GetBitmapBytes(info.Width, info.Height, info.PixelFormat, image);

            File.WriteAllBytes(info.DevNode, pixelData);
        }

        private FramebufferInfo GetFramebufferInfo(Int32 displayId)
        {
            String sysFolder = Path.Combine(SYSGRAPHICS_FOLDER, $"{FRAMEBUFFER_NODES}{displayId}");
            String sizeFile = Path.Combine(sysFolder, VIRTUAL_SIZE_NODE);
            String bppFile = Path.Combine(sysFolder, BPP_NODE);
            String size = File.ReadAllText(sizeFile);
            String bpp = File.ReadAllText(bppFile);

            FramebufferInfo info = new FramebufferInfo()
            {
                DisplayId = displayId,
                DevNode = Path.Combine(DEV_FOLDER, $"{FRAMEBUFFER_NODES}{displayId}"),
                Width = Int32.Parse(size.Split(',').First()),
                Height = Int32.Parse(size.Split(',').Last()),
                PixelFormat = TranslatePixelFormat(Int32.Parse(bpp))
            };

            return info;
        }

        private static PixelFormat TranslatePixelFormat(Int32 bpp)
        {
            switch(bpp)
            {
                case 15:
                    return PixelFormat.Format16bppRgb555;

                case 16:
                    return PixelFormat.Format16bppRgb565;

                case 24:
                    return PixelFormat.Format24bppRgb;

                case 32:
                    return PixelFormat.Format32bppRgb;

                default:
                    return PixelFormat.Undefined;
            }
        }

        private static Byte[] GetBitmapBytes(Int32 width, Int32 height, PixelFormat pixelFormat, Bitmap image)
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

        private static String RunBash(String cmd)
        {
            String escaped = cmd.Replace("\"", "\\\"");
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escaped}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            String output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();
            process.Dispose();

            return output;
        }

        public void Reboot()
        {
            RunBash(REBOOT_CMD);
        }
    }
}
