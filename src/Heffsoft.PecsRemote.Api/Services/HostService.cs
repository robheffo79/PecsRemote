using Heffsoft.PecsRemote.Api.Data.Models;
using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class HostService : IHostService
    {
        private readonly String checkupdatesCmd;
        private readonly String rebootCmd;
        private readonly String scanwifiCmd;
        private readonly String blacklistCmd;
        private readonly String blacklist6Cmd;
        private readonly String unblacklistCmd;
        private readonly String unblacklist6Cmd;
        private readonly String strengthCmd;
        private readonly String updateCmd;

        private readonly String wlanFile;
        private readonly String hostnameFile;
        private readonly String cpuinfoFile;
        private readonly String wlanmacFile;
        private readonly String hostsFile;
        private readonly String uptimeFile;

        private readonly String devFolder;
        private readonly String languageFolder;

        private readonly IDisplayService displayService;

        public HostService(IConfiguration configuration, IDisplayService displayService)
        {
            IConfigurationSection command = configuration.GetSection("host:commands");
            checkupdatesCmd = command.GetValue<String>("checkupdates");
            rebootCmd = command.GetValue<String>("reboot");
            scanwifiCmd = command.GetValue<String>("scanwifi");
            blacklistCmd = command.GetValue<String>("blacklist");
            blacklist6Cmd = command.GetValue<String>("blacklist6");
            unblacklistCmd = command.GetValue<String>("unblacklist");
            unblacklist6Cmd = command.GetValue<String>("unblacklist6");
            strengthCmd = command.GetValue<String>("strength");
            updateCmd = command.GetValue<String>("update");

            IConfigurationSection files = configuration.GetSection("host:files");
            wlanFile = files.GetValue<String>("wlan");
            hostnameFile = files.GetValue<String>("hostname");
            cpuinfoFile = files.GetValue<String>("cpuinfo");
            wlanmacFile = files.GetValue<String>("wlanmac");
            hostsFile = files.GetValue<String>("hosts");
            uptimeFile = files.GetValue<String>("uptime");

            IConfigurationSection folders = configuration.GetSection("host:folders");
            devFolder = folders.GetValue<String>("dev");
            languageFolder = folders.GetValue<String>("language");

            this.displayService = displayService;
        }

        public String Hostname => File.ReadAllText(hostnameFile);

        public Int32 ConnectedDisplays => displayService.Displays.Count(e => e != null);

        public String Serial
        {
            get
            {
                String cpuInfo = File.ReadAllText(cpuinfoFile);
                foreach (String line in cpuInfo.Split(new Char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()))
                {
                    if (line.StartsWith("Serial"))
                    {
                        return line.Split(';').Last().Trim();
                    }
                }

                return "0000000000000000";
            }
        }

        public String Mac => File.ReadAllText(wlanmacFile);

        public Double Uptime
        {
            get
            {
                String uptime = File.ReadAllText(uptimeFile);
                String[] parts = uptime.Split(' ');
                return Double.Parse(parts[0]);
            }
        }

        public Task ConfigureIPSettings()
        {
            throw new NotImplementedException();
        }

        public Task ConfigureIPSettings(String ipv4, String subnet, String gateway, String primaryDns, String secondaryDns)
        {
            throw new NotImplementedException();
        }

        public Task ConnectToWiFi(String ssid, String key)
        {
            throw new NotImplementedException();
        }

        public Task ConnectToWiFi(String ssid, String username, String password)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<String>> ScanForWiFi()
        {
            Regex ssid = new Regex("ESSID:\"(?<ssid>[\\s\\S]{1,32})\"");

            String scanOutput = await RunBashAsync(scanwifiCmd);

            List<String> ssidList = new List<String>();

            foreach (String line in scanOutput.Split(new Char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()))
            {
                Match match = ssid.Match(line);
                if (match.Success)
                {
                    ssidList.Add(match.Groups["ssid"].Value);
                }
            }

            return ssidList;
        }

        public async Task SetHostname(String hostname)
        {
            String oldHostname = File.ReadAllText(hostnameFile).Trim();
            String newHostname = hostname.Trim().ToLower();

            // Update Hostname
            await File.WriteAllTextAsync(hostnameFile, newHostname);

            // Update Hosts
            String hostsFile = File.ReadAllText(hostsFile);
            hostsFile = hostsFile.Replace(oldHostname, newHostname);
            await File.WriteAllTextAsync(hostsFile, hostsFile);
        }

        public async Task SetImage(Int32 displayId, Bitmap image)
        {
            FramebufferInfo info = GetFramebufferInfo(displayId);

            Byte[] pixelData = await GetBitmapBytesAsync(info.Width, info.Height, info.PixelFormat, image);

            await File.WriteAllBytesAsync(info.DevNode, pixelData);
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
                DevNode = Path.Combine(devFolder, $"{FRAMEBUFFER_NODES}{displayId}"),
                Width = Int32.Parse(size.Split(',').First()),
                Height = Int32.Parse(size.Split(',').Last()),
                PixelFormat = TranslatePixelFormat(Int32.Parse(bpp))
            };

            return info;
        }

        private static PixelFormat TranslatePixelFormat(Int32 bpp)
        {
            switch (bpp)
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

        private static void ForkBash(String cmd)
        {
            String escaped = cmd.Replace("\"", "\\\"");
            Process process = Process.Start("/bin/bash", $"-c \"{escaped}\"");
        }

        private static Task<String> RunBashAsync(String cmd, Boolean captureOutput = true)
        {
            return Task.Run(async () =>
            {
                String escaped = cmd.Replace("\"", "\\\"");
                Process process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{escaped}\"",
                        RedirectStandardOutput = captureOutput,
                        RedirectStandardError = captureOutput,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                String output = null;

                if (captureOutput)
                {
                    output = await process.StandardOutput.ReadToEndAsync();
                    if (String.IsNullOrWhiteSpace(output))
                    {
                        output = await process.StandardError.ReadToEndAsync();
                    }
                }

                process.WaitForExit();
                process.Dispose();

                return output;
            });
        }

        public Task Reboot()
        {
            return RunBashAsync(rebootCmd, false);
        }

        public async Task ConfigureAdHoc()
        {
            await SetHostname("pecsremote");

            String suffix = Mac.Replace(":", "").GetLast(4);
            String ssid = $"{Hostname}_{suffix}";

            StringBuilder sb = new StringBuilder();
            sb.Append($"auto wlan0\niface wlan0 inet static\n  address 192.168.1.254\n  netmask 255.255.255.0\n  wireless-channel 6\n");
            sb.Append($"  wireless-essid {ssid}\nwireless-mode ad-hoc\n");
            File.WriteAllText(wlanFile, sb.ToString());
        }

        public Task<HostSettings> GetHostSettings()
        {
            return Task.FromResult(new HostSettings()
            {
                Hostname = Hostname
            });
        }

        public async Task<NetworkSettings> GetNetworkSettings()
        {
            NetworkSettings settings = new NetworkSettings();

            IEnumerable<String> lines = (await File.ReadAllLinesAsync(wlanFile)).Select(l => l.ToLowerInvariant());

            Boolean inSection = false;
            foreach (String line in lines)
            {
                String trimmed = line.Trim();

                if (trimmed.StartsWith("#"))
                    continue;

                if (inSection == false)
                {
                    if (trimmed.StartsWith("iface wlan0"))
                    {
                        inSection = true;
                        settings.UseDhcp = line.Contains("dhcp") && line.Contains("static") == false;
                    }
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(line) || String.IsNullOrWhiteSpace(line[0].ToString()) == false)
                    {
                        inSection = false;
                        break;
                    }

                    String[] parts = trimmed.Split(new Char[] { ' ' }, 2);
                    switch (parts[0])
                    {
                        case "address":
                            settings.IPAddress = parts[1];
                            break;

                        case "netmask":
                            settings.SubnetMask = parts[1];
                            break;

                        case "gateway":
                            settings.GatewayAddress = parts[1];
                            break;

                        case "wpa-ssid":
                        case "wireless-essid":
                            settings.SSID = Dequote(parts[1]);
                            break;

                        case "wpa-psk":
                        case "wireless-key":
                            settings.Key = Dequote(parts[1]);
                            break;

                        case "dns-nameservers":
                            settings.DNSServers = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            break;
                    }
                }
            }

            return settings;
        }

        private String Dequote(String text)
        {
            if (text.StartsWith("\"") && text.EndsWith("\""))
            {
                return text.Substring(1, text.Length - 2).Replace("\\\"", "\"");
            }

            return text;
        }

        public async Task<Int32> GetUpdatesAvailable()
        {
            String output = await RunBashAsync(checkupdatesCmd);
            String[] counts = output.Split(';');

            if (Int32.TryParse(counts[0], out Int32 updates))
                return updates;

            return 0;
        }

        public Task ApplyUpdates()
        {
            return Task.Run(() =>
            {
                ForkBash(updateCmd);
            });
        }

        public Task BlacklistIP(IPAddress ip)
        {
            String cmd = null;

            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                cmd = blacklistCmd.Replace("{ip}", ip.ToString());
            }

            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                cmd = blacklist6Cmd.Replace("{ip}", ip.ToString());
            }

            if (cmd != null)
                return Task.CompletedTask;

            return RunBashAsync(cmd, false);
        }

        public Task UnBlacklistIP(IPAddress ip)
        {
            String cmd = null;

            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                cmd = unblacklistCmd.Replace("{ip}", ip.ToString());
            }

            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                cmd = unblacklist6Cmd.Replace("{ip}", ip.ToString());
            }

            if (cmd != null)
                return Task.CompletedTask;

            return RunBashAsync(cmd, false);
        }

        public async Task<Double?> GetWiFiStrength()
        {
            Double? strength = null;

            String[] output = (await RunBashAsync(strengthCmd, true)).Split(Environment.NewLine);
            Regex regex = new Regex(@"Link Quality=(?<fig>[0-9]*)/(?<div>[0-9]*)");
            foreach (String line in output)
            {
                Match match = regex.Match(line);
                if(match.Success)
                {
                    if(Double.TryParse(match.Groups["fig"].Value, out Double figure))
                    {
                        if(Double.TryParse(match.Groups["div"].Value, out Double divisor))
                        {
                            strength = (1.0D / divisor) * figure;
                            break;
                        }
                    }
                }
            }

            return strength;
        }

        public async Task<IEnumerable<Language>> GetLanguages()
        {
            List<Language> languages = new List<Language>();

            foreach(String jsonFile in Directory.EnumerateFiles(languageFolder, "*.json"))
            {
                try
                {
                    String json = await File.ReadAllTextAsync(jsonFile);
                    Language language = JsonConvert.DeserializeObject<Language>(json);
                    language.Default = language.ISO == "en";
                    languages.Add(language);
                }
                catch
                {
                }
            }

            return languages.OrderBy(l => l.ISO);
        }
    }
}
