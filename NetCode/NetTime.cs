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
        private static long baseTimestamp = 0;

        static NetTime()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            realtime = true;
        }

        public static long Now()
        {
            return baseTimestamp + stopwatch.ElapsedMilliseconds;
        }
        
        public static void Advance(int ms)
        {
            if (!Realtime)
            {
                baseTimestamp += ms;
            }
        }
    }
}
