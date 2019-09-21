﻿using Heffsoft.PecsRemote.Api.Data.Models;
using Heffsoft.PecsRemote.Api.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IHostService
    {
        String Hostname { get; }
        String Serial { get; }
        String Mac { get; }
        Int32 ConnectedDisplays { get; }

        Double Uptime { get; }

        Task<Int32> GetUpdatesAvailable();

        Task<Double?> GetWiFiStrength();

        Task<HostSettings> GetHostSettings();
        Task<NetworkSettings> GetNetworkSettings();

        Task<IEnumerable<Language>> GetLanguages();

        Task<IEnumerable<String>> ScanForWiFi();

        Task SetHostname(String hostname);
        Task ConfigureAdHoc();
        Task ConnectToWiFi(String ssid, String key);
        Task ConnectToWiFi(String ssid, String username, String password);

        Task ConfigureIPSettings();
        Task ConfigureIPSettings(String ipv4, String subnet, String gateway, String primaryDns, String secondaryDns);

        Task SetImage(Int32 displayId, Bitmap image);

        Task ApplyUpdates();

        Task Reboot();

        Task BlacklistIP(IPAddress ip);
        Task UnBlacklistIP(IPAddress ip);
    }
}
