using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api
{
    public static class Delay
    {
        private static readonly Int64 TIMER_FREQ_MS = Stopwatch.Frequency / 1000;
        private static readonly Int64 TIMER_FREQ_US = (Stopwatch.Frequency / 1000) / 1000;
        private static readonly Stopwatch timer = Stopwatch.StartNew();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ms(Int32 ms)
        {
            Int64 waitFor = timer.ElapsedTicks + (TIMER_FREQ_MS * ms);
            while (timer.ElapsedTicks < waitFor)
            {
                // Sit And Spin!
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Us(Int32 us)
        {
            Int64 waitFor = timer.ElapsedTicks + (TIMER_FREQ_US * us);
            while (timer.ElapsedTicks < waitFor)
            {
                // Sit And Spin!
            }
        }
    }
}
