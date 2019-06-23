using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Volatile;
using NetCode;

using NetcodeTest.Util;
using NetcodeTest.Events;

namespace NetcodeTest.Entities
{
    [EnumerateSyncEntity]
    public class Ship : Physical
    {
        [Synchronisable]
        public Color Color { get; protected set; }
        [Synchronisable]
        public Vector2 Size { get; protected set; }
        
        float Thrust = 300;
        float Torque = 400;
        float FireRate = 20f;
        float Cooldown = 0.0f;
        bool Firing = false;
        float MissileCooldown = 0.0f;
        float MissilePeriod = 5f;
        float MultiMissilePeriod = 0.2f;

        int MultiMissilesLeft = 0;

        public Ship()
        {
            Color = Color.Black;
        }

        public Ship(Vector2 position, Vector2 velocity, Color color, float angle, float angleV)
        {
            Color = color;
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angleV;

            Size = new Vector2(20, 15);
        }

        public override void Update(float delta)
        {
            if (Firing && Cooldown <= 0)
            {
                Cooldown = 1.0f / FireRate;
                Context.AddEntity(new Projectile(this));
            }
            else if (Cooldown > 0) { Cooldown -= delta; }

            if (MissileCooldown > 0)
            {
                MissileCooldown -= delta;
            }

            if (MissileCooldown <= 0 && MultiMissilesLeft > 0)
            {
                MissileCooldown = MultiMissilePeriod;
                Context.AddEntity(new Missile(this, 0.3f));
                MultiMissilesLeft -= 1;
                if (MultiMissilesLeft <= 0)
                {
                    MissileCooldown = MissilePeriod;
                }
            }

            base.Update(delta);
        }

        public override void OnDestroy()
        {
            Context.AddEvent(new Explosion(Position, 32f, 2f, Color));
            base.OnDestroy();
        }

        public override void Draw(SpriteBatch batch)
        {
            Drawing.DrawTriangle(batch, Position, Size, Angle, Color.Lerp(Color, Color.White, 0.1f));
        }

        public void Control(float thrust, float torque, bool firing)
        {
            Firing = firing;

            thrust = Fmath.Clamp(thrust, 0.0f, 1.0f);
            

            if (torque == 0)
            {
                // This is negative feedback, so im unsure why torque and angular velocity have different signs.
                torque = AngularVelocity * 10f;
            }
            torque = Fmath.Clamp(torque, -1.0f, 1.0f);

            /*
            float epsilon = 0.001f;
            if (torque < epsilon && torque > -epsilon)
            {
                float dt = (CollisionBody.AngularVelocity > epsilon) ? 1f : ((CollisionBody.AngularVelocity < -epsilon) ? -1f : 0f);
                torque = dt;
            }
            */


            CollisionBody.AddForce(Fmath.CosSin(Angle, Thrust * thrust));
            CollisionBody.AddTorque(Torque * torque);
        }

        public void FireMissile()
        {
            if (MissileCooldown <= 0)
            {
                MissileCooldown = MissilePeriod;
                Context.AddEntity(new Missile(this, 1f));
            }
        }
        
        public void FireMultiMissile()
        {
            if (MissileCooldown <= 0)
            {
                MultiMissilesLeft = 4;
                MissileCooldown = MultiMissilePeriod;
                Context.AddEntity(new Missile(this, 0.3f));
            }
        }

        protected override Vector2[] GetHitbox()
        {
            float length = Size.X;
            float width = Size.Y;
            float CentroidToBack = length * (1.0f / 3.0f);

            return new Vector2[]
                {
                    new Vector2(length - CentroidToBack, 0),
                    new Vector2(-CentroidToBack, -width/2),
                    new Vector2(-CentroidToBack, width/2),
                };
        }
    }
}
