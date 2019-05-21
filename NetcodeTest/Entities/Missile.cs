using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NetcodeTest.Util;
using NetcodeTest.Events;
using NetCode;

namespace NetcodeTest.Entities
{
    [EnumerateSynchEntity]
    public class Missile : Projectile 
    {
        const float Acceleration = 500f;
        const float Speed = 120f;
        const float Recoil = 1000;
        const float ExplosiveForce = 50000;
        const float ExplosiveDamage = 300;
        const float ExplosiveArea = 80f;

        public Missile() { }
        public Missile(Ship creator)
        {
            Position = creator.Position;
            Angle = creator.Angle;
            baseVelocity = creator.baseVelocity + Fmath.CosSin(Angle, Speed);
            AngularVelocity = 0f;

            Position += Fmath.CosSin(Angle) * creator.Size.X / 2;
            Velocity = baseVelocity;

            Creator = creator;

            Creator.Push(-Fmath.CosSin(Angle, Recoil));
        }

        public override void Update(float delta)
        {
            Velocity += Fmath.CosSin(Angle, Acceleration) * delta;
            base.Update(delta);
        }

        public override void Draw(SpriteBatch batch)
        {
            Color color = (Creator == null) ? Color.White : Color.Lerp(Creator.Color, Color.White, 0.5f);
            Drawing.DrawBullet(batch, Position, new Vector2(40, 8), Angle, color);
        }

        public override void OnCollide(Physical phys)
        {
            IsDestroyed = true;

            foreach (Physical p in Context.GetEntitiesWithin(Position, ExplosiveArea))
            {
                Vector2 deltaPos = p.Position - Position;
                float alpha = 1f - (deltaPos.Length()/(ExplosiveArea*1.2f));
                if (alpha > 0)
                {
                    deltaPos.Normalize();
                    p.Push(deltaPos * alpha * ExplosiveForce);
                    p.Hitpoints -= alpha * ExplosiveDamage;
                }
            }

            Context.AddEvent(new Explosion(Position, 40f, 1.5f, Creator.Color));
        }

        public override void Predict(long timestamp)
        {
            long delta = timestamp - baseTimestamp;
            float deltas = (delta / 1000.0f);
            Position = basePosition + (baseVelocity * deltas) + Fmath.CosSin(Angle, Acceleration * (deltas * deltas) / 2);
            Angle = baseAngle + (AngularVelocity * deltas);
        }
    }
}
