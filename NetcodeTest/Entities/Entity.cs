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
        [Synchronisable(SyncFlags.HalfPrecisionFloats)]
        public Vector2 Position { get; protected set; }
        [Synchronisable(SyncFlags.HalfPrecisionFloats)]
        public Vector2 Velocity { get; protected set; }
        [Synchronisable(SyncFlags.HalfPrecisionFloats)]
        public float Angle { get; protected set; }
        [Synchronisable]
        public float AngularVelocity { get; protected set; } 

        public Entity( Vector2 position )
        {
            Position = position;
            Velocity = new Vector2(0, 0);
            Angle = 0f;
            AngularVelocity = 0f;
        }

        public virtual void Update(float delta)
        {
            Position += Velocity * delta;
            Angle += AngularVelocity * delta;
        }

        public abstract void Draw(SpriteBatch batch);
    }
}
