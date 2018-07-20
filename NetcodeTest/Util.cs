using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace NetcodeTest
{
    public static class Util
    {
        private static Random Rand;

        static Util()
        {
            Rand = new Random();
        }

        public static Vector2 Angle(float angle, float length)
        {
            return new Vector2((float)Math.Cos(angle) * length, (float)Math.Sin(angle) * length);
        }

        public static double AngleTo(Vector2 origin, Vector2 target)
        {
            return Math.Atan2(target.Y - origin.Y, target.X - origin.X);
        }

        public static float RandAngle()
        {
            return (float)Rand.NextDouble() * MathHelper.TwoPi;
        }

        public static float RandF(float max)
        {
            return (float)Rand.NextDouble() * max;
        }

        public static Vector2 RandomVector( Vector2 bounds )
        {
            return new Vector2(
                (float)(bounds.X * Rand.NextDouble()),
                (float)(bounds.Y * Rand.NextDouble())
                );
        }
    }
}
