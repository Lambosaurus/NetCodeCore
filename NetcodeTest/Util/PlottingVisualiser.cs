using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NetcodeTest
{
    public class Plotter
    {
        public Vector2 Size { get; set; }
        public Vector2 Origin { get; set; }

        private int Index = 0;
        private double[] Points;
        private double PendingPointTotal = 0;
        private int PendingSamples = 0;


        public double TracePointSize { get; set; } = 8.0;
        public double TraceThickness { get; set; } = 4.0;
        public double AxisThickness { get; set; } = 3.0;
        public Color AxisColor { get; set; } = Color.White * 0.8f;
        public Color TraceColor { get; set; } = Color.DarkRed * 0.8f;
        public Color BackgroundColor { get; set; } = Color.Black * 0.5f;

        public int TickCount { get; set; } = 5;

        public int FontSize { get; set; } = 18;
        public int AxisLabelSize { get; set; } = 70;

        public double MaxValue { get; set; } = 1.0;
        public string Unit { get; set; } = "";

        public bool AutoScaleUp { get; set; } = false;
        public bool AutoScaleDown { get; set; } = false;

        public int SamplesPerPoint { get; set; }

        public Plotter(int points, int samples, Vector2 size)
        {
            Points = new double[points];
            Size = size;
            SamplesPerPoint = samples;
        }

        public void AddValue(double value)
        {
            PendingPointTotal += value;
            if (++PendingSamples >= SamplesPerPoint)
            {
                double newPoint = PendingPointTotal / SamplesPerPoint;
                Points[Index++] = newPoint;
                if (Index >= Points.Length) { Index = 0; }
                PendingSamples = 0;
                PendingPointTotal = 0;

                if (AutoScaleUp && newPoint > MaxValue)
                {
                    MaxValue = AxisRounding(newPoint);
                }
                else if (AutoScaleDown)
                {
                    MaxValue = AxisRounding(Points.Max());
                }
            }
        }

        private static readonly double[] AxisScalars = { 1, 1.5, 2, 3, 4, 5, 8 };

        private double AxisRounding(double value)
        {
            if (TickCount < 2)
            {
                return Math.Round(value + 0.099, 1);
            }

            double notches = TickCount - 1;
            double magnitude = 0.1;
            while (notches * magnitude * 10 < value)
            {
                magnitude *= 10;
            }

            foreach (double d in AxisScalars)
            {
                if (d * magnitude * notches > value)
                {
                    return d * magnitude * notches;
                }
            }
            return 10 * magnitude * notches;
        }

        public void Draw(SpriteBatch batch)
        {
            Vector2 plotSize = Size - new Vector2(FontSize * 2 + AxisLabelSize, FontSize * 2);
            Vector2 plotStart = Origin + new Vector2(FontSize + AxisLabelSize, FontSize);
            Vector2 halfSize = Size / 2;

            double maxValue = MaxValue;

            Drawing.DrawHardSquare(batch, Origin + halfSize, Size, 0, BackgroundColor);

            Drawing.DrawHardLine(batch, plotStart, plotStart + new Vector2(0, plotSize.Y), (float)AxisThickness, AxisColor);
            //Drawing.DrawHardLine(batch, plotStart + new Vector2(0, plotSize.Y), plotStart + plotSize, (float)AxisThickness, AxisColor);

            float tickDepth = (float)AxisThickness * 3;
            for(int i = 0; i < TickCount; i++)
            {
                double tick = maxValue * i / (TickCount - 1);
                float ty = (float)(1.0 - (tick / maxValue)) * plotSize.Y + plotStart.Y;
                Drawing.DrawHardLine(batch, new Vector2(plotStart.X - tickDepth, ty), new Vector2(plotStart.X, ty), (float)AxisThickness, AxisColor);

                string text;
                if (maxValue < 10)
                {
                    text = string.Format("{0:0.0}{1}", tick, Unit);
                }
                else
                {
                    text = string.Format("{0}{1}", tick, Unit);
                }
                Drawing.DrawString(batch, text, new Vector2(Origin.X + 4, ty - (FontSize / 2)), FontSize, AxisColor);
            }

            double x;
            double y = 0;
            Vector2 lastPoint = Vector2.Zero;
            for (int i = 0; i < Points.Length; i++)
            {
                x = (i - ((double)PendingSamples / SamplesPerPoint)) / (Points.Length - 1);
                y = Points[(i + Index) % Points.Length] / maxValue;

                Vector2 point = new Vector2(
                    (float)(x) * plotSize.X,
                    (float)(1.0 - y) * plotSize.Y
                    ) + plotStart;


                if (i > 0)
                {
                    if (i == 1)
                    {
                        float alpha = (lastPoint.X - plotStart.X) / (lastPoint.X - point.X);
                        lastPoint = (point - lastPoint) * alpha + lastPoint;
                    }

                    Drawing.DrawLine(batch, lastPoint, point, (float)TraceThickness, TraceColor);
                    if (TracePointSize > 0)
                    {
                        Drawing.DrawCircle(batch, point, new Vector2((float)TracePointSize), 0, TraceColor);
                    }
                }
                lastPoint = point;
            }

            double yEnd = (y * (SamplesPerPoint - PendingSamples) + (PendingPointTotal / maxValue)) / SamplesPerPoint ;
            Vector2 pointEnd = new Vector2(
                plotSize.X,
                (float)(1.0 - yEnd) * plotSize.Y
                ) + plotStart;
            Drawing.DrawLine(batch, lastPoint, pointEnd, (float)TraceThickness, TraceColor);
        }
    }
}
