using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NetCode;
using Volatile;

using NetcodeTest.Util;

namespace NetcodeTest.Entities
{

    public abstract class Physical : Entity
    {
        public VoltBody CollisionBody { get; protected set; }
        protected const float VelocityTolerance = 0.2f;
        public double Hitpoints = 100f;
        
        public override void Update(float delta)
        {
            Position = CollisionBody.Position;
            Angle = CollisionBody.Angle;

            if (Angle > MathHelper.TwoPi || Angle < MathHelper.TwoPi)
            {
                Angle = Fmath.Mod(Angle, MathHelper.TwoPi);
            }

            if ((CollisionBody.LinearVelocity - Velocity).LengthSquared() > VelocityTolerance * VelocityTolerance
                || (Fmath.Abs(CollisionBody.AngularVelocity - AngularVelocity) > VelocityTolerance) )
            {
                RequestMotionUpdate();
            }

            if (Hitpoints < 0)
            {
                IsDestroyed = true;
            }
        }

        public override void UpdateMotion(long timestamp)
        {
            Velocity = CollisionBody.LinearVelocity;
            AngularVelocity = CollisionBody.AngularVelocity;
            base.UpdateMotion(timestamp);
        }

        public override void OnDestroy()
        {
            if (CollisionBody != null && CollisionBody.World != null)
            {
                CollisionBody.World.RemoveBody(CollisionBody);
            }

            base.OnDestroy();
        }

        public override void Set(Vector2 position, float angle)
        {
            base.Set(position, angle);
            CollisionBody.Set(position, angle);
        }

        public void Push(Vector2 force)
        {
            CollisionBody.AddForce(force);
        }
        public void Push(Vector2 force, Vector2 location)
        {
            CollisionBody.AddForce(force, location);
        }

        protected abstract Vector2[] GetHitbox();
        
        public void GenerateBody(VoltWorld world)
        {
            VoltPolygon polygon = world.CreatePolygonBodySpace( GetHitbox() );
            CollisionBody = world.CreateDynamicBody(Position, Angle, polygon);
            CollisionBody.AngularVelocity = AngularVelocity;
            CollisionBody.LinearVelocity = Velocity;
            CollisionBody.UserData = this;
        }
    }
}
