using System;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IRandomService
    {
        Int32 Next();
        Int32 Next(Int32 maxValue);
        Int32 Next(Int32 minValue, Int32 maxValue);

        void NextBytes(Byte[] buffer);
        Double NextDouble();

        String NextSalt(Int32 length);
    }
}
