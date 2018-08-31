using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NetCode;

using NetcodeTest.Util;

namespace NetcodeTest.Events
{
    public class Explosion : Event
    {
        [Synchronisable(SyncFlags.HalfPrecisionFloats)]
        public Vector2 Position { get; protected set; }
        [Synchronisable(SyncFlags.HalfPrecisionFloats)]
        public float Scale { get; protected set; }
        [Synchronisable(SyncFlags.HalfPrecisionFloats)]
        public float Duration { get; protected set; }
       
        public float Alpha { get; protected set; }
        
        public Explosion()
        {
        }

        public Explosion(Vector2 position, float scale, float duration)
        {
            Position = position;
            Scale = scale;
            Duration = duration;
            Timestamp = NetTime.Now();
        }
        
        public override bool Expired()
        {
            return Alpha <= 0f;
        }

        public override void Predict(long timestamp)
        {
            long delta = timestamp - Timestamp;
            Alpha = 1.0f - (delta / (Duration * 1000f));
        }

        public override void Draw(SpriteBatch batch)
        {
            float s = Scale * (1.5f - (Alpha * 0.5f));
            Drawing.DrawCircle(batch, Position, new Vector2(s), 0f, Color.White * Alpha);
        }
    }
}
