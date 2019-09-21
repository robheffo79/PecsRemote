using Heffsoft.PecsRemote.Api.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Threading;

namespace Heffsoft.PecsRemote.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IWebHost webHost = CreateWebHostBuilder(args).Build();

            StartBackgroundServices(webHost);

            await webHost.RunAsync();
        }

        private static void StartBackgroundServices(IWebHost webHost)
        {
            IThermalService thermalService = webHost.Services.GetService<IThermalService>();
            thermalService.StartControl();

            IDisplayService displayService = webHost.Services.GetService<IDisplayService>();
            displayService.IdentifyDisplays(TimeSpan.Zero);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
