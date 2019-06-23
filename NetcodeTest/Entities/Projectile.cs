using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NetCode;
using NetcodeTest.Util;
using NetcodeTest.Events;

namespace NetcodeTest.Entities
{
    [EnumerateSyncEntity]
    public class Projectile : Entity
    {
        [Synchronisable(SyncFlags.Reference)]
        public Ship Creator;

        const float Speed = 500f;
        private double Duration = 3.0f;
        const double Damage = 20;

        const float Recoil = 250;
        const float Force = 1000f;

        public Projectile()
        {
        }
        
        public Projectile(Ship creator)
        {
            Position = creator.Position;
            Angle = creator.Angle;
            Velocity = creator.Velocity + Fmath.CosSin(Angle, Speed);
            AngularVelocity = 0f;

            Position += Fmath.CosSin(Angle) * creator.Size.X / 2;

            Creator = creator;

            Creator.Push(-Fmath.CosSin(Angle, Recoil));
        }

        public override void Update(float delta)
        {
            Duration -= delta;
            if (Duration <= 0) { IsDestroyed = true; }
            base.Update(delta);
        }

        public override void Draw(SpriteBatch batch)
        {
            Color color = (Creator == null) ? Color.White : Color.Lerp(Creator.Color, Color.White, 0.5f);
            Drawing.DrawBullet(batch, Position, new Vector2(20, 4), Angle, color);
        }

        public virtual void OnCollide( Physical phys )
        {
            phys.Hitpoints -= Damage;
            IsDestroyed = true;

            phys.Push(Velocity * Force / Speed, Position);

            Context.AddEvent(new Explosion(Position, 10f, 0.4f, Creator.Color));
        }
    }
}
