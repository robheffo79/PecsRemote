using System;
using System.Drawing;

namespace Heffsoft.PecsRemote.Api.Models
{
    public class PaintEventArgs : IDisposable
    {
        public Rectangle ClipRectangle { get; }
        public Graphics Graphics { get; }

        public PaintEventArgs(Bitmap image)
        {
            ClipRectangle = new Rectangle(0, 0, image.Width, image.Height);
            Graphics = Graphics.FromImage(image);
        }

        public void Dispose()
        {
            Graphics.Dispose();
        }
    }
}