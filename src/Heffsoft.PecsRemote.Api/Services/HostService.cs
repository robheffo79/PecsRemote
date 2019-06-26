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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.PeerToPeer;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class HostService : IHostService
    {
        private const String REBOOT_CMD = "reboot";
        private const String WIFI_SCAN_CMD = "iwlist wlan0 scan";
        private const String INTERFACES_FILE = "/etc/network/interfaces";
        private const String HOSTNAME_FILE = "/etc/hostname";
        private const String CPUINFO_FILE = "/proc/cpuinfo";
        private const String MAC_FILE = "/sys/class/net/wlan0/address";
        private const String HOSTS_FILE = "/etc/hosts";
        private const String DEV_FOLDER = "/dev";
        private const String SYSGRAPHICS_FOLDER = "/sys/class/graphics";
        private const String VIRTUAL_SIZE_NODE = "virtual_size";
        private const String BPP_NODE = "bits_per_pixel";
        private const String FRAMEBUFFER_NODES = "fb";

        public String Hostname => File.ReadAllText(HOSTNAME_FILE);

        public Int32 ConnectedDisplays => Directory.EnumerateFiles(DEV_FOLDER, FRAMEBUFFER_NODES + "*").Count();

        public String Serial
        {
            get
            {
                String cpuInfo = File.ReadAllText(CPUINFO_FILE);
                foreach(String line in cpuInfo.Split(new Char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()))
                {
                    if(line.StartsWith("Serial"))
                    {
                        return line.Split(';').Last().Trim();
                    }
                }

                return "0000000000000000";
            }
        }

        public String Mac => File.ReadAllText(MAC_FILE);

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

        public async Task<IEnumerable<String>> ScanForWiFi()
        {
            Regex ssid = new Regex("ESSID:\"(?<ssid>[\\s\\S]{1,32})\"");

            String scanOutput = await RunBashAsync(WIFI_SCAN_CMD);

            List<String> ssidList = new List<String>();

            foreach (String line in scanOutput.Split(new Char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()))
            {
                Match match = ssid.Match(line);
                if(match.Success)
                {
                    ssidList.Add(match.Groups["ssid"].Value);
                }
            }

            return ssidList;
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

        public async Task SetImage(Int32 displayId, Bitmap image)
        {
            FramebufferInfo info = GetFramebufferInfo(displayId);

            Byte[] pixelData = await GetBitmapBytesAsync(info.Width, info.Height, info.PixelFormat, image);

            using (FileStream fs = new FileStream(info.DevNode, FileMode.Open, FileAccess.Write, FileShare.None))
            {
                await fs.WriteAsync(pixelData);
            }
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

        private static Task<Byte[]> GetBitmapBytesAsync(Int32 width, Int32 height, PixelFormat pixelFormat, Bitmap image)
        {
            return Task.Run(() =>
            {
                Bitmap dest = new Bitmap(width, height, pixelFormat);
                dest.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (Graphics gfx = Graphics.FromImage(dest))
                {
                    gfx.CompositingQuality = CompositingQuality.HighQuality;
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.SmoothingMode = SmoothingMode.HighQuality;

                    gfx.FillRectangle(Brushes.Black, 0, 0, width, height);

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
            });
        }

        private static Task<String> RunBashAsync(String cmd)
        {
            return Task.Run(() =>
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
            });
        }

        public void Reboot()
        {
            RunBashAsync(REBOOT_CMD);
        }

        public void ConfigureAdHoc()
        {
            SetHostname("pecsremote");

            String suffix = Mac.Replace(":", "").GetLast(4);
            String ssid = $"{Hostname}_{suffix}";

            StringBuilder sb = new StringBuilder();
            sb.Append($"# interfaces(5) file used by ifup(8) and ifdown(8)\n\n# Please note that this file is written to be used with dhcpcd\n");
            sb.Append($"# For static IP, consult /etc/dhcpcd.conf and 'man dhcpcd.conf'\n\n# Include files from /etc/network/interfaces.d:\n");
            sb.Append($"source-directory /etc/network/interfaces.d\n\nauto lo\niface lo inet loopback\n\niface eth0 inet dhcp\n\n");
            sb.Append($"auto wlan0\niface wlan0 inet static\n  address 192.168.1.254\n  netmask 255.255.255.0\n  wireless-channel 6\n");
            sb.Append($"  wireless-essid {ssid}\nwireless-mode ad-hoc\n");
            File.WriteAllText(INTERFACES_FILE, sb.ToString());
        }
    }
}
