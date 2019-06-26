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

        public CastService()
        {
            seenReceivers = new Dictionary<String, IReceiver>();
            deviceLocator = new DeviceLocator();
            deviceLocatorObserver = deviceLocator.FindReceiversContinuous();
            deviceLocatorObserver.Subscribe(d => ProcessDevice(d));
        }

        private void ProcessDevice(IReceiver device)
        {
            lock(seenReceivers)
            {
                seenReceivers[device.Id] = device;
            }
        }

        public IEnumerable<CastTarget> GetCastReceivers()
        {
            IEnumerable<IReceiver> receivers;

            lock(seenReceivers)
            {
                receivers = seenReceivers.Values.ToArray();
            }

            return receivers.Select(r => new CastTarget() { Id = r.Id, Name = r.FriendlyName, Address = r.IPEndPoint.Address });
        }

        public Boolean ConnectToCastReceiver(String id)
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
                sender.ConnectAsync(receiver).Wait();

                mediaChannel = sender.GetChannel<IMediaChannel>();
                sender.LaunchAsync(mediaChannel).Wait();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void DisconnectCastReceiver()
        {
            if (sender != null)
            {
                mediaChannel.StopAsync().Wait();
                mediaChannel = null;

                sender.Disconnect();
                sender = null;
            }
        }

        public void CastMedia(Uri url)
        {
            var mediaStatus = mediaChannel.LoadAsync(new MediaInformation()
            {
                ContentId = url.ToString()
            }).Result;
        }

        public void StopMedia()
        {
            if(sender != null && mediaChannel != null)
            {
                mediaChannel.StopAsync().Wait();
            }
        }
    }
}
