
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NetCode;
using Volatile;

namespace NetcodeTest.Entities
{
    public abstract class Entity
    {
        public VoltBody CollisionBody { get; protected set; }
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

        const float VelocityTolerance = 0.01f;
        
        public Entity()
        {
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Angle = 0f;
            AngularVelocity = 0f;
        }

        public virtual void Update(float delta, long timestamp)
        {
            Position = CollisionBody.Position;
            Angle = CollisionBody.Angle;

            if ((CollisionBody.LinearVelocity - Velocity).LengthSquared() > VelocityTolerance * VelocityTolerance)
            {
                Velocity = CollisionBody.LinearVelocity;
                AngularVelocity = CollisionBody.AngularVelocity;
                UpdateMotion(timestamp);
            }
        }

        public virtual void Clamp( Vector2 low, Vector2 high, long timestamp )
        {
            if (Position.X < low.X || Position.X > high.X || Position.Y < low.Y || Position.Y > high.Y)
            {
                Position = new Vector2(Util.Mod(Position.X - low.X, high.X - low.X) + low.X,
                                       Util.Mod(Position.Y - low.Y, high.Y - low.Y) + low.Y);

                CollisionBody.Set(Position, Angle);

                UpdateMotion(timestamp);
            }
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

        public abstract void GenerateBody(VoltWorld world);
        public abstract void Draw(SpriteBatch batch);
    }
}
