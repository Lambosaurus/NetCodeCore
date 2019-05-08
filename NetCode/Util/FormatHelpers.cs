using System;
using System.Collections.Generic;
using System.Linq;


namespace NetCode.Util
{
    public static class Primitive
    {
        private static string[] SIPrefix = { "", "K", "M", "G", "T", "P", "E", "Z", "Y" };
        public static string SIFormat(double value, string suffix, bool useBinaryPowers = true)
        {
            double divisor = useBinaryPowers ? 1024 : 1000;

            int power = 0;
            while ( value >= divisor)
            {
                power++;
                value /= divisor;
            }

            if (value < 10)
            {
                return string.Format("{0:0.00}{1}{2}", value, SIPrefix[power], suffix);
            }
            else if (value < 100)
            {
                return string.Format("{0:0.0}{1}{2}", value, SIPrefix[power], suffix);
            }
            else
            {
                return string.Format("{0:0.}{1}{2}", value, SIPrefix[power], suffix);
            }
        }
    }
}
