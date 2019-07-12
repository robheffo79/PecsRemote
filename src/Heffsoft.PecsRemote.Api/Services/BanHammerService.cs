using Heffsoft.PecsRemote.Api.Data.Models;
using Heffsoft.PecsRemote.Api.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class BanHammerService : IHostedService, IDisposable
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private Timer timer;

        public BanHammerService(ILogger<BanHammerService> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("BanHammer Service is starting.");
            timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private void DoWork(Object state)
        {
            logger.LogInformation("BanHammer Service is processing.");

            IHostService hostService = serviceProvider.GetService<IHostService>();
            IDataContext dataContext = serviceProvider.GetService<IDataContext>();
            IDataRepository<BannedAddress> bannedAddressRepo = dataContext.GetRepository<BannedAddress>();

            foreach (BannedAddress ban in bannedAddressRepo.GetAll().Where(b => b.UnbanAt.HasValue && b.UnbanAt.Value <= DateTime.Now).ToArray())
            {
                try
                {
                    if (IPAddress.TryParse(ban.IPAddress, out IPAddress bannedIP))
                    {
                        hostService.UnBlacklistIP(bannedIP).Wait();
                        ban.UnbanAt = null;
                        bannedAddressRepo.Update(ban);
                    }
                }
                catch
                {
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("BanHammer Service is stopping.");
            timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}
