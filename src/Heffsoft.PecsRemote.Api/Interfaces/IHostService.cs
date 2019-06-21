using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    interface IHostService
    {
        String Hostname { get; }
        Int32 ConnectedDisplays { get; }

        IEnumerable<String> ScanForWiFi();

        void SetHostname(String hostname);
        void ConnectToWiFi(String ssid, String key);
        void ConnectToWiFi(String ssid, String username, String password);

        void ConfigureIPSettings();
        void ConfigureIPSettings(String ipv4, String subnet, String gateway, String primaryDns, String secondaryDns);

        void SetImage(Int32 displayId, Bitmap image);
    }
}
