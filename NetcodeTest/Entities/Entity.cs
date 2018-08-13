
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NetCode;

using NetcodeTest.Util;

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
        
        public bool NeedsMotionReset { get; private set; } = false;
        public bool IsDestroyed { get; protected set; } = false;

        protected ContextToken Context;

        public Entity()
        {
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Angle = 0f;
            AngularVelocity = 0f;
        }

        protected void RequestMotionUpdate()
        {
            NeedsMotionReset = true;
        }

        public virtual void UpdateMotion(long timestamp)
        {
            baseTimestamp = timestamp;
            basePosition = Position;
            baseAngle = Angle;
            NeedsMotionReset = false;
        }

        public virtual void Predict(long timestamp)
        {
            long delta = timestamp - baseTimestamp;
            Position = basePosition + (Velocity * (delta / 1000.0f));
            Angle = baseAngle + (AngularVelocity * (delta / 1000.0f));
        }

        public virtual void Update(float delta)
        {
            Position += Velocity * delta;
            Angle += AngularVelocity * delta;

            if (Angle > MathHelper.TwoPi || Angle < MathHelper.TwoPi)
            {
                Angle = Fmath.Mod(Angle, MathHelper.TwoPi);
            }
        }

        public virtual void Set( Vector2 position, float angle )
        {
            Position = position;
            Angle = angle;
        }

        public virtual void Clamp(Vector2 low, Vector2 high)
        {
            if (Position.X < low.X || Position.X > high.X || Position.Y < low.Y || Position.Y > high.Y)
            {
                Vector2 newPos = new Vector2(Fmath.Mod(Position.X - low.X, high.X - low.X) + low.X,
                                       Fmath.Mod(Position.Y - low.Y, high.Y - low.Y) + low.Y);

                Set(newPos, Angle);

                RequestMotionUpdate();
            }
        }

        public void SetContext(ContextToken context)
        {
            Context = context;
        }

        public virtual void OnDestroy()
        {

        }
        
        public abstract void Draw(SpriteBatch batch);
    }
}
