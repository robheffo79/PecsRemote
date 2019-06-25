using Heffsoft.PecsRemote.Api.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class RandomService : IRandomService
    {
        private static readonly Random seedRandom = new Random();

        private readonly Random instanceRandom;

        public RandomService()
        {
            lock(seedRandom)
            {
                instanceRandom = new Random(seedRandom.Next());
            }
        }

        public Int32 Next() => instanceRandom.Next();

        public Int32 Next(Int32 maxValue) => instanceRandom.Next(maxValue);

        public Int32 Next(Int32 minValue, Int32 maxValue) => instanceRandom.Next(minValue, maxValue);

        public void NextBytes(Byte[] buffer) => instanceRandom.NextBytes(buffer);

        public Double NextDouble() => instanceRandom.NextDouble();

        public String NextSalt(Int32 length)
        {
            String sourceChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()-_=+\\|]}[{'\";:,<.>/?";
            Char[] chars = new Char[length];

            for (Int32 i = 0; i < length; i++)
                chars[i] = sourceChars[instanceRandom.Next(0, sourceChars.Length)];

            return new String(chars);
        }
    }
}
