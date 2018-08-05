using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NetCode;

namespace NetcodeTest.Entities
{
    public abstract class Entity
    {
        public Vector2 Position { get; protected set; }
        public float Angle { get; protected set; }

        [Synchronisable(SyncFlags.HalfPrecisionFloats)]
        public Vector2 Velocity { get; protected set; }
        [Synchronisable(SyncFlags.HalfPrecisionFloats)]
        public float AngularVelocity { get; protected set; }

        [Synchronisable(SyncFlags.HalfPrecisionFloats)]
        protected Vector2 basePosition { get; set; }
        [Synchronisable(SyncFlags.HalfPrecisionFloats)]
        protected float baseAngle { get; set; }
        [Synchronisable(SyncFlags.Timestamp)]
        protected long baseTimestamp { get; set; }
        
        public Entity()
        {
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Angle = 0f;
            AngularVelocity = 0f;
        }

        public virtual void Update(float delta)
        {
            Position += Velocity * delta;
            Angle += AngularVelocity * delta;
        }

        public virtual void UpdateMotion(long timestamp)
        {
            baseTimestamp = timestamp;
            basePosition = Position;
            baseAngle = Angle;
        }

        public virtual void Predict(long timestamp)
        {
            long delta = timestamp - baseTimestamp;
            Position = basePosition + (Velocity * (delta / 1000.0f));
            Angle = baseAngle + (AngularVelocity * (delta / 1000.0f));
        }

        public abstract void Draw(SpriteBatch batch);
    }
}
