using GoogleCast;
using GoogleCast.Channels;
using GoogleCast.Models.Media;
using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class CastService : ICastService
    {
        private readonly IObservable<IReceiver> deviceLocatorObserver;
        private readonly DeviceLocator deviceLocator;
        private readonly Dictionary<String, IReceiver> seenReceivers;

        private Sender sender;
        private IMediaChannel mediaChannel;
        private MediaStatus mediaStatus;

        public String CurrentReceiver { get; private set; }

        public CastService()
        {
            seenReceivers = new Dictionary<String, IReceiver>();
            deviceLocator = new DeviceLocator();
            deviceLocatorObserver = deviceLocator.FindReceiversContinuous();
            deviceLocatorObserver.Subscribe(d => ProcessDevice(d));
            CurrentReceiver = null;
        }

        private void ProcessDevice(IReceiver device)
        {
            lock(seenReceivers)
            {
                seenReceivers[device.Id] = device;
            }
        }

        public Task<IEnumerable<CastTarget>> GetCastReceivers()
        {
            IEnumerable<IReceiver> receivers;

            lock(seenReceivers)
            {
                receivers = seenReceivers.Values.ToArray();
            }

            return Task.FromResult(receivers.Select(r => new CastTarget() { Id = r.Id, Name = r.FriendlyName, Address = r.IPEndPoint.Address.ToString() }));
        }

        public async Task<Boolean> ConnectToCastReceiver(String id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            if (String.IsNullOrWhiteSpace(id))
                throw new ArgumentException($"{nameof(id)} is empty.");

            if (seenReceivers.ContainsKey(id) == false)
                throw new InvalidOperationException($"No cast device with Id of '{id}' found.");

            try
            {
                IReceiver receiver = seenReceivers[id];
                sender = new Sender();
                await sender.ConnectAsync(receiver);

                mediaChannel = sender.GetChannel<IMediaChannel>();
                await sender.LaunchAsync(mediaChannel);

                CurrentReceiver = id;
                return true;
            }
            catch
            {
                CurrentReceiver = null;
                return false;
            }
        }

        public async Task DisconnectCastReceiver()
        {
            if (sender != null)
            {
                await mediaChannel.StopAsync();
                mediaChannel = null;

                sender.Disconnect();
                sender = null;
            }
        }

        public async Task CastMedia(Uri url)
        {
            mediaStatus = await mediaChannel.LoadAsync(new MediaInformation()
            {
                ContentId = url.ToString()
            });
        }

        public async Task StopMedia()
        {
            if(sender != null && mediaChannel != null)
            {
                await mediaChannel.StopAsync();
            }
        }
    }
}
