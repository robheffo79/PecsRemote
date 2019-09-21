using Heffsoft.PecsRemote.Api.Interfaces;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Devices
{
    public class PiSpiBus : ISpiBus
    {
        private const Int32 CHUNK_SIZE = 512;

        private ReaderWriterLockSlim busLock = new ReaderWriterLockSlim();
        private SpiDevice busDevice = null;

        public PiSpiBus(Int32 bus, Int32 cs, Int32 frequency)
        {
            busDevice = SpiDevice.Create(new SpiConnectionSettings(bus, cs)
            {
                ChipSelectLineActiveState = PinValue.Low,
                ClockFrequency = frequency,
                DataBitLength = 8,
                DataFlow = DataFlow.MsbFirst,
                Mode = SpiMode.Mode0
            });
        }

        public void Aquire()
        {
            busLock.EnterWriteLock();
        }

        public Boolean Aquire(TimeSpan timeout)
        {
            return busLock.TryEnterWriteLock(timeout);
        }

        public void Dispose()
        {
            busDevice.Dispose();
        }

        public byte Read()
        {
            return busDevice.ReadByte();
        }

        public void Read(byte[] buffer)
        {
            busDevice.Read(new Span<Byte>(buffer));
        }

        public void Read(byte[] buffer, int offset, int length)
        {
            busDevice.Read(new Span<Byte>(buffer, offset, length));
        }

        public void Release()
        {
            busLock.ExitWriteLock();
        }

        public void Write(byte data)
        {
            busDevice.WriteByte(data);
        }

        public void Write(IEnumerable<Byte> data)
        {
            foreach (Byte[] chunk in data.ToArray().Slices(CHUNK_SIZE))
            {
                busDevice.Write(new ReadOnlySpan<Byte>(chunk));
            }
        }

        public void Write(UInt16 data)
        {
            busDevice.Write(new ReadOnlySpan<Byte>(new Byte[] { (Byte)((data & 0xFF00) >> 8), (Byte)(data & 0x00FF) }));
        }

        public void Write(IEnumerable<UInt16> data)
        {
            foreach (UInt16 value in data)
            {
                Write(value);
            }
        }

        public void Write(UInt32 data)
        {
            busDevice.Write(new ReadOnlySpan<Byte>(BitConverter.GetBytes(data)));
        }

        public void Write(IEnumerable<UInt32> data)
        {
            foreach (UInt32 value in data)
            {
                Write(value);
            }
        }
    }
}
