using Heffsoft.PecsRemote.Api.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IDisplayService : IDisposable
    {
        Double MaxBrightness { get; set; }

        IDisplay[] Displays { get; }

        void IdentifyDisplays(TimeSpan time);

        void SetBacklightTimeout(TimeSpan time);
    }

    public interface IDisplay : IDisposable
    {
        Int32 Width { get; }
        Int32 Height { get; }

        Graphics Graphics { get; }

        void SwapBuffers();

        void Init();

        event EventHandler<PaintEventArgs> Paint;
    }

    public enum Rotation
    {
        Zero = 0,
        Portrait = 0,
        Ninty = 90,
        Landscape = 90,
        OneEighty = 180,
        FlippedPortrait = 180,
        TwoSeventy = 270,
        FlippedLandscape = 270
    }
}