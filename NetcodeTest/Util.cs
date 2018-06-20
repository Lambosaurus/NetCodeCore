using System;
using System.Collections.Generic;
using System.Linq;


using Microsoft.Xna.Framework;

namespace NetcodeTest
{
    public static class Util
    {
        public static Vector2 Angle(double angle)
        {
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        public static double AngleTo(Vector2 origin, Vector2 target)
        {
            return Math.Atan2(target.Y - origin.Y, target.X - origin.X);
        }
    }
}
