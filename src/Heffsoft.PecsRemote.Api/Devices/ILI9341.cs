using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Devices
{
    public class ILI9341 : IDisplay
    {
        #region High-Level Display Code
        private Bitmap frontBuffer = null;
        private Bitmap backBuffer = null;

        public Int32 Width { get; private set; }

        public Int32 Height { get; private set; }

        public Graphics Graphics
        {
            get
            {
                if (backBuffer != null)
                {
                    return Graphics.FromImage(backBuffer);
                }

                return null;
            }
        }

        public void SwapBuffers()
        {
            lock (this)
            {
                Bitmap buffer = frontBuffer;
                frontBuffer = backBuffer;
                backBuffer = buffer;
            }

            Task.Run(() =>
            {
                Debug.WriteLine($"Blitting Display Id #{channel}");

                Byte[] frame = GetFrame(frontBuffer);
                Blit(0, 0, Width, Height, frame);
            });
        }

        private Byte[] GetFrame(Bitmap buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            BitmapData bmpdata = null;

            try
            {
                bmpdata = buffer.LockBits(new Rectangle(0, 0, buffer.Width, buffer.Height), ImageLockMode.ReadOnly, buffer.PixelFormat);
                Int32 numbytes = bmpdata.Stride * buffer.Height;
                Byte[] bytedata = new Byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                Byte[] frame = new byte[Width * Height * 2];

                if(rgb == 1)
                {
                    Rgb888ToBgr565(bytedata, frame);
                }
                else
                {
                    Rgb888ToRgb565(bytedata, frame);
                }

                return frame;
            }
            finally
            {
                if (bmpdata != null)
                    buffer.UnlockBits(bmpdata);
            }
        }

        private void Rgb888ToRgb565(byte[] bytedata, byte[] frame)
        {
            for (Int32 pi = 0; pi < Width * Height; pi++)
            {
                Byte red = bytedata[(pi * 3) + 0];
                Byte green = bytedata[(pi * 3) + 1];
                Byte blue = bytedata[(pi * 3) + 2];

                UInt16 b = (UInt16)((blue >> 3) & 0x1f);
                UInt16 g = (UInt16)(((green >> 2) & 0x3f) << 5);
                UInt16 r = (UInt16)(((red >> 3) & 0x1f) << 11);
                UInt16 p = (UInt16)(r | g | b);

                frame[(pi * 2) + 0] = (Byte)((p >> 8) & 0xFF);
                frame[(pi * 2) + 1] = (Byte)(p & 0xFF);
            }
        }

        private void Rgb888ToBgr565(byte[] bytedata, byte[] frame)
        {
            for (Int32 pi = 0; pi < Width * Height; pi++)
            {
                Byte red = bytedata[(pi * 3) + 0];
                Byte green = bytedata[(pi * 3) + 1];
                Byte blue = bytedata[(pi * 3) + 2];

                UInt16 b = (UInt16)(((blue >> 3) & 0x1f) << 11);
                UInt16 g = (UInt16)(((green >> 2) & 0x3f) << 5);
                UInt16 r = (UInt16)((red >> 3) & 0x1f);
                UInt16 p = (UInt16)(r | g | b);

                frame[(pi * 2) + 0] = (Byte)((p >> 8) & 0xFF);
                frame[(pi * 2) + 1] = (Byte)(p & 0xFF);
            }
        }
        #endregion

        #region Low-Level Driver Code

        private const Int32 WIDTH = 240;
        private const Int32 HEIGHT = 320;

        private const Int32 MEM_Y = 7;
        private const Int32 MEM_X = 6;
        private const Int32 MEM_V = 5;
        private const Int32 MEM_L = 4;
        private const Int32 MEM_H = 2;
        private const Int32 MEM_BGR = 3;


        private ISpiBus spi = null;
        private IDisplayGpio gpio = null;
        private Byte channel;
        private Rotation rotation;
        private Byte rgb;

        public event EventHandler<PaintEventArgs> Paint;

        public ILI9341(ISpiBus spi, IDisplayGpio gpio, Byte channel, Rotation rotation = Rotation.Landscape, Boolean rgb = true)
        {
            this.Width = -1;
            this.Height = -1;
            this.spi = spi ?? throw new ArgumentNullException(nameof(spi));
            this.gpio = gpio ?? throw new ArgumentNullException(nameof(gpio));
            this.channel = channel;
            this.rotation = rotation;
            this.rgb = (Byte)(rgb ? 1 : 0);
        }

        private void AccessBus(Action action)
        {
            spi.Aquire();
            gpio.Channel = channel;

            try
            {
                action.Invoke();
            }
            finally
            {
                spi.Release();
            }
        }

        private void SetAddressWindow(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            WriteCommand(0x2A, (Byte)((x >> 8) & 0xFF), (Byte)(x & 0xFF), (Byte)(((x + width - 1) >> 8) & 0xFF), (Byte)((x + width - 1) & 0xFF));
            WriteCommand(0x2B, (Byte)((y >> 8) & 0xFF), (Byte)(y & 0xFF), (Byte)(((y + height - 1) >> 8) & 0xFF), (Byte)((y + height - 1) & 0xFF));
        }

        public void Blit(Int32 x, Int32 y, Int32 width, Int32 height, Byte[] data)
        {
            SetAddressWindow(x, y, width, height);
            WriteCommand(0x2C, data);
        }

        private void WriteCommand(Byte cmd, params Byte[] data)
        {
            gpio.DC = false;
            spi.Write(cmd);

            if (data != null && data.Length > 0)
            {
                gpio.DC = true;
                spi.Write(data);
            }
        }

        public void Init()
        {
            Debug.WriteLine($"Initialising Display Id #{channel}");

            AccessBus(() =>
            {
                WriteCommand(0x01);
                Delay.Ms(5);
                WriteCommand(0x28);

                WriteCommand(0xCF, 0x00, 0x83, 0x30);
                WriteCommand(0xED, 0x64, 0x03, 0x12, 0x81);
                WriteCommand(0xE8, 0x85, 0x01, 0x79);
                WriteCommand(0xCB, 0x39, 0X2C, 0x00, 0x34, 0x02);
                WriteCommand(0xF7, 0x20);
                WriteCommand(0xEA, 0x00, 0x00);

                /* ------------power control-------------------------------- */
                WriteCommand(0xC0, 0x26);
                WriteCommand(0xC1, 0x11);

                /* ------------VCOM --------- */
                WriteCommand(0xC5, 0x35, 0x3E);
                WriteCommand(0xC7, 0xBE);

                /* ------------memory access control------------------------ */
                WriteCommand(0x3A, 0x55); /* 16bit pixel */

                /* ------------frame rate----------------------------------- */
                WriteCommand(0xB1, 0x00, 0x1B);

                /* ------------Gamma---------------------------------------- */
                /* WriteCommand(0xF2, 0x08); */ /* Gamma Function Disable */
                WriteCommand(0x26, 0x01);

                /* ------------display-------------------------------------- */
                WriteCommand(0xB7, 0x07); /* entry mode set */
                WriteCommand(0xB6, 0x0A, 0x82, 0x27, 0x00);

                WriteCommand(0x11); /* sleep out */
                Delay.Ms(100);
                WriteCommand(0x29); /* display on */
                Delay.Ms(20);

                switch (rotation)
                {
                    case Rotation.Portrait:
                        Width = WIDTH;
                        Height = HEIGHT;
                        WriteCommand(0x36, (Byte)((1 << MEM_X) | (rgb << MEM_BGR)));
                        break;

                    case Rotation.Landscape:
                        Width = HEIGHT;
                        Height = WIDTH;
                        WriteCommand(0x36, (Byte)((1 << MEM_Y) | (1 << MEM_V) | (1 << MEM_X) | (rgb << MEM_BGR)));
                        break;

                    case Rotation.FlippedPortrait:
                        Width = WIDTH;
                        Height = HEIGHT;
                        WriteCommand(0x36, (Byte)((1 << MEM_Y) | (rgb << MEM_BGR)));
                        break;

                    case Rotation.FlippedLandscape:
                        Width = HEIGHT;
                        Height = WIDTH;
                        WriteCommand(0x36, (Byte)((1 << MEM_L) | (1 << MEM_V) | (rgb << MEM_BGR)));
                        break;
                }
            });

            frontBuffer = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            backBuffer = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
        }

        public void Dispose()
        {
            if(frontBuffer != null)
                frontBuffer.Dispose();

            if (backBuffer != null)
                backBuffer.Dispose();
        }
        #endregion
    }
}
