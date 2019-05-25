using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace NetcodeTest.Util
{
    public static class Fmath
    {
        private static Random Rand;

        static Fmath()
        {
            Rand = new Random();
        }
        
        public static Vector2 CosSin(float angle)
        {
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        public static Vector2 Rotate(Vector2 point, float angle)
        {
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);

            return new Vector2((c * point.X) - (s * point.Y),
                                (s * point.X) + (c * point.Y));
        }

        public static Vector2 CosSin(float angle, float length)
        {
            return new Vector2((float)Math.Cos(angle) * length, (float)Math.Sin(angle) * length);
        }

        public static float WrapAngle(float alpha)
        {
            alpha = alpha % MathHelper.TwoPi;
            if (alpha < 0) { return alpha + MathHelper.TwoPi; }
            return alpha;
        }

        public static float AngleTo(Vector2 target)
        {
            return (float)Math.Atan2(target.Y, target.X);
        }

        public static float AngleTo(Vector2 origin, Vector2 target)
        {
            return (float)Math.Atan2(target.Y - origin.Y, target.X - origin.X);
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

        public static float Clamp(float value, float min = 0f, float max = 1.0f)
        {
            if (value < min) { return min; }
            if (value > max) { return max; }
            return value;
        }

        // modulus with positive results only
        public static float Mod(float a, float b)
        {
            float m = a % b;
            return (m > 0) ? m : m + b;
        }

        // gets the shortest angle to the target angle from the current angle (with correct sign!)
        public static float AngleDelta(float target, float current)
        {
            float a = target - current;
            return Mod(a + MathHelper.Pi, MathHelper.TwoPi) - MathHelper.Pi;
        }

        // standard rotation by pi/2
        public static Vector2 RotatePos(Vector2 point)
        {
            return new Vector2(-point.Y, point.X);
        }
        // standard rotation by -pi/2
        public static Vector2 RotateNeg(Vector2 point)
        {
            return new Vector2(point.Y, -point.X);
        }

        public static float Cross(Vector2 one, Vector2 two)
        {
            return ((one.X * two.Y) - (one.Y * two.X));
        }

        public static float Dot(Vector2 one, Vector2 two)
        {
            return (one.X * two.X) + (one.Y * two.Y);
        }

        public static float Abs(float value)
        {
            return value < 0 ? -value : value;
        }

        public static float Sqrt(float value)
        {
            return (float)Math.Sqrt(value);
        }
    }
}
