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
    public class Ship : Physical
    {
        [Synchronisable]
        public Color Color { get; protected set; }
        [Synchronisable]
        public Vector2 Size { get; protected set; }

        float Thrust = 300;
        float Torque = 500;
        float FireRate = 10f;
        float Cooldown = 0.0f;
        bool Firing = false;

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

            base.Update(delta);
        }

        public override void OnDestroy()
        {
            Context.AddEvent(new Explosion(Position, 32f, 2f));
            base.OnDestroy();
        }

        public override void Draw(SpriteBatch batch)
        {
            Drawing.DrawTriangle(batch, Position, Size, Angle, Color);
        }

        public void Control(float thrust, float torque, bool firing)
        {
            Firing = firing;

            thrust = Fmath.Clamp(thrust, 0.0f, 1.0f);
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
            

            RequestMotionUpdate();
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
