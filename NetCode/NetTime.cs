using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;


namespace NetCode
{
    public static class NetTime
    {
        private static bool realtime;
        public static bool Realtime {
            get
            {
                return realtime;
            }
            set
            {
                realtime = value;
                if (realtime)
                {
                    stopwatch.Start();
                }
                else
                {
                    stopwatch.Stop();
                }
            }
        }
        
        private static Stopwatch stopwatch;
        private static double timeOffsetFloating = 0;
        private static long timeOffset = 0;

        static NetTime()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            realtime = true;
        }

        public static long Now()
        {
            return timeOffset + stopwatch.ElapsedMilliseconds;
        }

        public static double Seconds()
        {
            return (timeOffsetFloating / 1000.0) + stopwatch.Elapsed.TotalSeconds;
        }
        
        public static void Advance(double ms)
        {
            if (!Realtime)
            {
                timeOffsetFloating += ms;
                timeOffset = (long)timeOffsetFloating;
            }
        }
    }
}
